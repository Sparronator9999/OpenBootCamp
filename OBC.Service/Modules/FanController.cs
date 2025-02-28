using OBC.Common;
using OBC.Common.Configs;
using OBC.Service.Logs;
using System;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

namespace OBC.Service.Modules;

internal sealed class FanController : IDisposable
{
    private readonly FanControlConf Config;
    private readonly Logger Log;

    private Timer PollTimer;
    private readonly SMC SMC;

    private Fan[] Fans;

    public FanController(FanControlConf cfg, Logger logger, SMC smc)
    {
        Config = cfg;
        Log = logger;
        SMC = smc;
    }

    ~FanController()
    {
        Dispose();
    }

    public void Dispose()
    {
        PollTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Start()
    {
        Log.Info("Getting fan information...", nameof(FanController));
        int fanCount = GetFanCount();
        if (fanCount == -1)
        {
            Log.Error("Failed to get fan count!", nameof(FanController));
            return;
        }

        Config.FanConfs ??= [];

        while (Config.FanConfs.Count < fanCount)
        {
            Config.FanConfs.Add(new FanConf());
        }

        Fans = new Fan[fanCount];
        short ctrlBits = 0;
        for (int i = 0; i < fanCount; i++)
        {
            Fans[i] = new()
            {
                Name = GetFanName(i) ?? "NO NAME",
                MinSpeed = GetMinFanSpeed(i),
                MaxSpeed = GetMaxFanSpeed(i)
            };
            if (Fans[i].MinSpeed < 0 || Fans[i].MaxSpeed < 0)
            {
                Log.Error($"Failed to get min/max speed for fan #{i} ({Fans[i].Name})!", nameof(FanController));
                return;
            }
            Log.Debug($"Found fan #{i} (name = {Fans[i].Name}, minSpeed = {Fans[i].MinSpeed}, maxSpeed = {Fans[i].MaxSpeed})", nameof(FanController));
            FanConf cfg = Config.FanConfs[i];
            if (cfg.Enabled)
            {
                ctrlBits |= (short)(1 << i);
                if (string.IsNullOrEmpty(cfg.SensorKey) || cfg.SensorKey.Length != 4)
                {
                    Log.Warn($"Fan #{i}'s associated SMC sensor key is invalid/missing!", nameof(FanController));
                    cfg.Enabled = false;
                }
                if (cfg.Tmax <= cfg.Tmin)
                {
                    Log.Warn($"Fan #{i}'s Tmax <= Tmin!", nameof(FanController));
                    cfg.Enabled = false;
                }
            }

            if (!cfg.Enabled)
            {
                Log.Warn($"Fan #{i} is disabled in config!", nameof(FanController));
            }
        }

        Log.Info("Starting fan control thread...", nameof(FanController));
        PollTimer = new(Config.PollRate);
        PollTimer.Elapsed += PollFans;

        SetFanCtrl(ctrlBits);
        PollFans(null, null);
        PollTimer.Start();
    }

    public void Stop()
    {
        if (PollTimer is null)
        {
            return;
        }

        // stop the fan control thread
        Log.Info("Stopping fan control module...", nameof(FanController));
        ResetFanCtrl();
        PollTimer.Stop();

        // cleanup stuff
        PollTimer.Dispose();
        PollTimer = null;
    }

    public void Wake()
    {
        short ctrlBits = 0;
        for (int i = 0; i < Config.FanConfs.Count; i++)
        {
            // reset target speed and effective temp.
            // so that fan speeds get re-applied properly
            Fans[i].Temp = 0;
            Fans[i].TargetSpeed = 0;
            if (Config.FanConfs[i].Enabled)
            {
                ctrlBits |= (short)(1 << i);
            }
        }
        SetFanCtrl(ctrlBits);
        PollFans(null, null);
        PollTimer?.Start();
    }

    public void Sleep()
    {
        ResetFanCtrl();
        PollTimer?.Stop();
    }

    private void PollFans(object sender, ElapsedEventArgs e)
    {
        try
        {
            for (int i = 0; i < Fans.Length; i++)
            {
                FanConf cfg = Config.FanConfs[i];
                if (!cfg.Enabled)
                {
                    continue;
                }

                Fan fan = Fans[i];

                // get target speed based on config's Tmin + Tmax
                float tRpm;
                float temp = GetTemp(cfg.SensorKey);

                // increase effective temperature if
                // real temp goes above effective
                if (temp > fan.Temp || cfg.Tdown <= 0)
                {
                    fan.Temp = temp;
                }
                // decrease effective temperature if real temp
                // drops more than Tdown degrees below effective
                else if (temp + cfg.Tdown < fan.Temp)
                {
                    fan.Temp = temp + cfg.Tdown;
                }

                if (fan.Temp < cfg.Tmin)
                {
                    tRpm = fan.MinSpeed;
                }
                else if (fan.Temp > cfg.Tmax)
                {
                    tRpm = fan.MaxSpeed;
                }
                else
                {
                    float dT = cfg.Tmax - cfg.Tmin,
                        dS = fan.MaxSpeed - fan.MinSpeed,
                        tAdj = fan.Temp - cfg.Tmin;

                    // round to nearest 100 rpm
                    tRpm = (int)((tAdj * dS / dT + fan.MinSpeed) / 100 + 0.5) * 100;
                }

                if (fan.TargetSpeed != tRpm)
                {
                    if (SetFanSpeed(i, tRpm))
                    {
                        Log.Debug($"changed fan #{i}'s speed from {fan.TargetSpeed} to {tRpm}", nameof(FanController));
                        fan.TargetSpeed = tRpm;
                    }
                    else
                    {
                        Log.Error($"Failed to change fan #{i}'s speed to {tRpm}!\n" +
                            $"{Utils.GetWin32ErrMsg(SMC.ErrorCode)}", nameof(FanController));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(Strings.GetString("svcBgException", ex), nameof(FanController));
            ResetFanCtrl();
            PollTimer.Stop();
        }
    }

    private int GetFanCount()
    {
        return SMC.ReadUInt8("FNum", out byte value) ? value : -1;
    }

    private string GetFanName(int fan)
    {
        return SMC.ReadRawData($"F{fan}ID", 16, out byte[] data)
            ? Encoding.UTF8.GetString(data, 4, 12).TrimEnd(' ', '\0') : null;
    }

    private float GetMinFanSpeed(int fan)
    {
        return SMC.ReadFPE2($"F{fan}Mn", out float value)
            ? value : -1;
    }

    private float GetMaxFanSpeed(int fan)
    {
        return SMC.ReadFPE2($"F{fan}Mx", out float value)
            ? value : -1;
    }

    /*private float GetTargetFanSpeed(int fan)
    {
        return SMC.ReadFPE2($"F{fan}Tg", out float value)
            ? value : -1;
    }

    private float GetCurFanSpeed(int fan)
    {
        return SMC.ReadFPE2($"F{fan}Ac", out float value)
            ? value : -1;
    }*/

    private float GetTemp(string key)
    {
        return SMC.ReadSP78(key, out float value) ? (float)Math.Round(value, 2) : -1;
    }

    private bool ResetFanCtrl()
    {
        return SMC.WriteInt16("FS! ", 0);
    }

    private bool SetFanCtrl(short bits)
    {
        return SMC.WriteInt16("FS! ", bits);
    }

    private bool SetFanSpeed(int fan, float speed)
    {
        return SMC.WriteFPE2($"F{fan}Tg", speed);
    }

    private sealed class Fan
    {
        internal string Name;
        internal float MinSpeed;
        internal float MaxSpeed;
        internal float TargetSpeed;
        internal float Temp;
    }
}
