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
using OBC.Config;
using OBC.Service.Logs;
using OBC.Service.Modules;
using System;
using System.IO;
using System.ServiceProcess;
using System.Text;

namespace OBC.Service;

internal sealed class OBCService : ServiceBase
{
    private readonly Logger Log;

    private readonly SMC SMC = new("MacHALDriver");

    private KbdEventListener Listener;
    private FanController FanController;

    private ObcConfig Config;
    private readonly string ConfPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Sparronator9999", "OpenBootCamp", "obc.xml");

    public OBCService(Logger logger)
    {
        CanHandlePowerEvent = true;
        CanShutdown = true;
        Log = logger;
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
            SMCKeyInfo[] keys = SMC.GetSupportedKeys();
            if (keys is null || keys.Length == 0)
            {
                Log.Error(
                    "Failed to get supported SMC keys:\n" +
                    $"{Utils.GetWin32ErrMsg(SMC.ErrorCode)}");
            }
            else if (Config.LogSMCKeys)
            {
                Log.Debug("Supported SMC keys:");
                for (int i = 0; i < keys.Length; i++)
                {
                    StringBuilder sb = new($"{i:X4}");

                    sb.Append($": {keys[i].Key}, len = 0x{keys[i].Length:X2}, type = {keys[i].TypeString}, attr = {keys[i].Attributes}");
                    if (Config.LogSMCKeyData && (keys[i].Attributes & SMCKeyAttributes.Read) == SMCKeyAttributes.Read)
                    {
                        sb.Append($", data = ");
                        if (SMC.ReadRawData(keys[i].Key, keys[i].Length, out byte[] data))
                        {
                            for (int j = 0; j < data.Length; j++)
                            {
                                sb.Append($"{data[j]:X2} ");
                            }
                        }
                        else
                        {
                            sb.Append("null");
                        }
                    }
                    Log.Debug(sb.ToString());
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
                Listener?.Wake();
                FanController?.Wake();
                break;
            case PowerBroadcastStatus.Suspend:
                Listener?.Sleep();
                FanController?.Sleep();
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
}
