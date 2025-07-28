// This file is part of OpenBootCamp.
// Copyright © Sparronator9999 2024-2025.
//
// OpenBootCamp is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// OpenBootCamp is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
//
// You should have received a copy of the GNU General Public License along with
// OpenBootCamp. If not, see <https://www.gnu.org/licenses/>.

using OBC.Common;
using OBC.Common.Configs;
using OBC.Service.Logs;
using OBC.Service.Modules;
using System;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace OBC.Service;

internal sealed class OBCService : ServiceBase
{
    private readonly Logger Log;

    private readonly SMC SMC = new("MacHALDriver");

    private SMCKeyInfo[] SupportedSMCKeys;
    private readonly Timer KeyDumpTimer = new()
    {
        AutoReset = false,
    };

    private KbdEventListener Listener;
    private FanController FanController;
    private BattManager BattManager;

    private ObcConfig Config;
    private readonly string ConfPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Sparronator9999", "OpenBootCamp", "obc.xml");

    public OBCService(Logger logger)
    {
        CanHandlePowerEvent = true;
        CanShutdown = true;
        Log = logger;
        KeyDumpTimer.Elapsed += KeyDumpTimer_Elapsed;
    }

    protected override void OnStart(string[] args)
    {
        Log.Info(Strings.GetString("svcStarting"));

        LoadConf();

        Log.Info("Initialising drivers...");
        if (!SMC.Open())
        {
            Log.Warn(Strings.GetString("errNoMHD"));
        }
        else
        {
            SupportedSMCKeys = SMC.GetSupportedKeys();
            if (SupportedSMCKeys is null || SupportedSMCKeys.Length == 0)
            {
                Log.Error(
                    "Failed to get supported SMC keys:\n" +
                    $"{Utils.GetWin32ErrMsg(SMC.ErrorCode)}");
            }
            else
            {
                if (Config.DumpSMCKeys.HasFlag(SMCKeyDumpType.OnSvcStart))
                {
                    DumpSMCKeys(SupportedSMCKeys);
                }
                if (Config.DumpSMCKeys.HasFlag(SMCKeyDumpType.OnSvcStartDelayed))
                {
                    KeyDumpTimer.Interval = Config.KeyDumpDelayTime;
                    KeyDumpTimer.Start();
                }
            }
        }

        if (Config.KbdEventListener.Enabled)
        {
            Listener = new(Config.KbdEventListener, Log, SMC);
            Listener.Start();
        }

        if (Config.FanControl.Enabled && SMC.IsOpen)
        {
            FanController = new(Config.FanControl, Log, SMC);
            FanController.Start();
        }

        if (Config.BatteryManager.Enabled && SMC.IsOpen)
        {
            BattManager = new(Config.BatteryManager, Log, SMC);
            BattManager.Start();
        }
        Log.Info(Strings.GetString("svcStarted"));
    }

    protected override void OnStop()
    {
        StopService();
    }

    protected override void OnShutdown()
    {
        StopService();
    }

    private void StopService()
    {
        Log.Info(Strings.GetString("svcStopping"));
        Listener?.Stop();
        FanController?.Stop();
        //BattManager?.Stop();

        KeyDumpTimer.Stop();
        if (Config.DumpSMCKeys.HasFlag(SMCKeyDumpType.OnSvcStop))
        {
            DumpSMCKeys(SupportedSMCKeys);
        }

        Log.Info("Saving config...");
        Config.Save(ConfPath);

        Log.Info("Unloading drivers...");
        SMC?.Close();
        Log.Info(Strings.GetString("svcStopped"));
    }

    protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
    {
        switch (powerStatus)
        {
            case PowerBroadcastStatus.ResumeCritical:
            case PowerBroadcastStatus.ResumeSuspend:
            case PowerBroadcastStatus.ResumeAutomatic:
                if (Config.DumpSMCKeys.HasFlag(SMCKeyDumpType.OnWake))
                {
                    DumpSMCKeys(SupportedSMCKeys);
                }
                if (Config.DumpSMCKeys.HasFlag(SMCKeyDumpType.OnWakeDelayed))
                {
                    KeyDumpTimer.Interval = Config.KeyDumpDelayTime;
                    KeyDumpTimer.Start();
                }
                Listener?.Wake();
                FanController?.Wake();
                BattManager?.Wake();
                break;
            case PowerBroadcastStatus.Suspend:
                Listener?.Sleep();
                FanController?.Sleep();
                //BattManager?.Sleep();
                if (Config.DumpSMCKeys.HasFlag(SMCKeyDumpType.OnSleep))
                {
                    DumpSMCKeys(SupportedSMCKeys);
                }
                break;
        }
        return true;
    }

    private void LoadConf()
    {
        Log.Info(Strings.GetString("svcConfLoading"));

        try
        {
            Config = ObcConfig.Load(ConfPath);
            Log.Info(Strings.GetString("svcConfLoaded"));
        }
        catch (InvalidConfigException)
        {
            Log.Warn(Strings.GetString("wrnBadConf"));
            Config = new ObcConfig();
        }
        catch (FileNotFoundException)
        {
            Log.Warn(Strings.GetString("wrnNoConf"));
            Config = new ObcConfig();
        }
    }
    private void KeyDumpTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        DumpSMCKeys(SupportedSMCKeys);
    }

    private void DumpSMCKeys(SMCKeyInfo[] keys)
    {
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Sparronator9999", "OpenBootCamp", "SMCKeyDumps",
            $"smckeys-{DateTime.Now.ToString("s").Replace(':', '-')}.csv");

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (StreamWriter sw = new(path))
        {
            Log.Debug($"Dumping SMC keys to {path}...");
            sw.WriteLine("Index,Key,Length,Type,Attributes,Data");

            for (int i = 0; i < keys.Length; i++)
            {
                StringBuilder sb = new($"0x{i:X4},{keys[i].Key},0x{keys[i].Length:X2},{keys[i].TypeString},{keys[i].Attributes.ToString().Replace(',', ' ')},");
                if ((keys[i].Attributes & SMCKeyAttributes.Read) == SMCKeyAttributes.Read)
                {
                    if (SMC.ReadRawData(keys[i].Key, keys[i].Length, out byte[] data))
                    {
                        for (int j = 0; j < data.Length; j++)
                        {
                            sb.Append($"{data[j]:X2} ");
                        }
                    }
                    else
                    {
                        sb.Append("(null)");
                    }
                }
                sw.WriteLine(sb.ToString());
            }
            Log.Debug($"Finished dumping SMC keys to {path}.");
        }
    }
}
