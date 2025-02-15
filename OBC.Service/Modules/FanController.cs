using OBC.Common;
using OBC.Config;
using OBC.Service.Logs;
using System;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

namespace OBC.Service.Modules;

internal class FanController : IDisposable
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
        if (PollTimer is not null)
        {
            throw new InvalidOperationException("The fan controller is already running.");
        }

        WorkerLog("Getting fan count...");
        int fanCount = GetFanCount();
        if (fanCount == -1)
        {
            WorkerLog("ERROR: failed to get fan count!");
            return;
        }

        Config.FanConfs ??= [];

        while (Config.FanConfs.Count < fanCount)
        {
            Config.FanConfs.Add(new FanConf());
        }

        WorkerLog("Getting fan information...");
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
                WorkerLog($"ERROR: failed to get min/max speed for fan #{i} ({Fans[i].Name})!");
                return;
            }
            WorkerLog($"Found fan #{i} (name = {Fans[i].Name}, minSpeed = {Fans[i].MinSpeed}, maxSpeed = {Fans[i].MaxSpeed})");
            FanConf cfg = Config.FanConfs[i];
            if (cfg.Enabled)
            {
                ctrlBits |= (short)(1 << i);
                if (string.IsNullOrEmpty(cfg.SensorKey) || cfg.SensorKey.Length != 4)
                {
                    WorkerLog($"ERROR (fan #{i}): SMC key of associated temp. sensor is invalid/missing!");
                    cfg.Enabled = false;
                }
                if (cfg.Tmax <= cfg.Tmin)
                {
                    WorkerLog($"ERROR (fan #{i}): Tmax <= Tmin!");
                    cfg.Enabled = false;
                }
            }
            else
            {
                WorkerLog($"WARN: fan #{i} is disabled in config!");
            }
        }

        Log.Debug("Starting fan control poll thread...");
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
            throw new InvalidOperationException("The fan controller is not running.");
        }

        // stop the fan control thread
        Log.Debug("Stopping fan control module...");
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
                if (temp < cfg.Tmin)
                {
                    tRpm = fan.MinSpeed;
                }
                else if (temp > cfg.Tmax)
                {
                    tRpm = fan.MaxSpeed;
                }
                else
                {
                    float dT = cfg.Tmax - cfg.Tmin,
                        dS = fan.MaxSpeed - fan.MinSpeed,
                        tAdj = temp - cfg.Tmin;

                    // round to nearest 100 rpm
                    tRpm = (int)((tAdj * dS / dT + fan.MinSpeed) / 100 + 0.5) * 100;
                }

                WorkerLog($"Fan #{i}: curSpd {GetCurFanSpeed(i)}, target {tRpm}, temp {GetTemp(cfg.SensorKey)} °C");
                if (fan.TargetSpeed != tRpm)
                {
                    if (SetFanSpeed(i, tRpm))
                    {
                        WorkerLog($"changed fan speed from {fan.TargetSpeed} to {tRpm}");
                        fan.TargetSpeed = tRpm;
                    }
                    else
                    {
                        WorkerLog($"FAILED to change fan speed to {tRpm}!\n" +
                            $"{Utils.GetWin32ErrMsg(SMC.ErrorCode)}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Unhandled exception occurred in FanController background thread:\n{ex}");
            ResetFanCtrl();
            PollTimer.Stop();
        }
    }

    private void WorkerLog(string message)
    {
        Log.Debug($"[FanController] {message}");
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

    private float GetSysTargetFanSpeed(int fan)
    {
        return SMC.ReadFPE2($"F{fan}Tg", out float value)
            ? value : -1;
    }

    private float GetCurFanSpeed(int fan)
    {
        return SMC.ReadFPE2($"F{fan}Ac", out float value)
            ? value : -1;
    }

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

    private class Fan
    {
        public string Name;
        public float MinSpeed;
        public float MaxSpeed;
        public float TargetSpeed;
    }
}
