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
using OBC.IPC;
using OBC.Service.Hardware;
using OBC.Service.Logs;
using System;
using System.IO;
using System.IO.Pipes;
using System.Management;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace OBC.Service
{
    internal sealed class KeyboardEventListener : IDisposable
    {
        private const int EVENT_COUNT = 29;

        private static readonly uint[] KbdEventIOCtls =
            new uint[EVENT_COUNT]
        {
            0xB403201F, // 0:  eject CD-ROM
            0xB4032043, // 1:  volume up (Windows 7 and lower?)
            0xB403204B, // 2:  volume down (Windows 7 and lower?)
            0xB4032053, // 3:  brightness up
            0xB403205B, // 4:  brightness down
            0xB403206F,
            0xB4032073,
            0xB403208B, // 7:  keyboard backlight up
            0xB403208F, // 8:  keyboard backlight down
            0xB4032093,
            0xB4032097,
            0xB403209B,
            0xB403209F,
            0xB40320B3,
            0xB40320BB,
            0xB40320C3,
            0xB40320CB,
            0xB40320D3,
            0xB40320DB,
            // only in BC6 or later:
            0xB40320E3, // 19: keyboard presence detected
            0xB40320EB,
            0xB40320F3,
            0xB40320FB,
            0xB4032103,
            0xB403210B,
            0xB4032113,
            0xB403211B,
            0xB4032123,
            0xB403212B,
            // 29: stop event listener
        };

        private readonly EventWaitHandle[] Events = new AutoResetEvent[EVENT_COUNT + 1];

        private Task ListenerTask;

        private bool CleanupComplete;

        private byte DispBright;

        private readonly AppleKeyboardDriver KeyAgent;
        //private readonly MacHALDriver HAL;

        private readonly KeyboardBacklight KeyLight;

        private readonly Logger Log;

        private readonly NamedPipeServer<ObcEvent> IPCServer;

        public KeyboardEventListener(AppleKeyboardDriver keyAgent, Logger logger, KeyboardBacklight keylight)
        {
            KeyAgent = keyAgent;
            Log = logger;
            KeyLight = keylight;

            // allow anyone to connect to the named pipe server
            // hopefully the IPC server is exploit-free...
            PipeSecurity security = new();
            security.AddAccessRule(new PipeAccessRule(
                "Users", PipeAccessRights.ReadWrite, AccessControlType.Allow));

            IPCServer = new("ObcEvents", security);
        }

        public void Start()
        {
            if (CleanupComplete)
            {
                return;
            }

            CleanupComplete = false;

            Log.Debug("Initalising events...");
            for (int i = 0; i < Events.Length; i++)
            {
                Events[i] = new AutoResetEvent(false);
            }

            for (int i = 0; i < KbdEventIOCtls.Length; i++)
            {
                bool success = false;
                Events[i].SafeWaitHandle.DangerousAddRef(ref success);
                if (success)
                {
                    if (!KeyAgent.IOControl(KbdEventIOCtls[i], Events[i].SafeWaitHandle.DangerousGetHandle(), 0))
                    {
                        Log.Error(Strings.GetString("errEventInit"));
                    }
                }
                else
                {
                    Log.Error(Strings.GetString("errEventHandle"));
                }
            }

            Log.Debug("Starting event IPC server...");
            IPCServer.Start();

            ListenerTask = new Task(HandleEvents, CancellationToken.None, TaskCreationOptions.LongRunning);
            ListenerTask.Start();
        }

        public void Stop(bool turnOffKeyLight = true)
        {
            // signal the "stop listener" event
            Events[EVENT_COUNT].Set();

            // wait for the event listener to stop
            ListenerTask.Wait();

            if (turnOffKeyLight && KeyLight is not null)
            {
                KeyLight.Enabled = false;
            }

            // perform cleanup (disposal of events, etc.)
            Cleanup();
        }

        private void HandleEvents()
        {
            DispBright = (byte)(GetBrightness() * 15 / 100);

            WorkerLog("Started listening for events.");
            while (true)
            {
                int eventID = WaitHandle.WaitAny(Events);
                switch (eventID)
                {
                    case 0:     // eject optical drive
                        IPCServer?.PushMessage(new ObcEvent(ObcEventType.Eject));
                        if (!EjectOpticalDrive())
                        {
                            WorkerLog(Strings.GetString("errCDEject"));
                        }
                        break;
                    case 3:     // display brightness up
                        if (DispBright + 1 > 15)
                        {
                            DispBright = 15;
                        }
                        else
                        {
                            DispBright++;
                        }
                        int brightnessPercent = (int)(DispBright / 15f * 100);
                        IPCServer?.PushMessage(new ObcEvent(
                            ObcEventType.DispBright, brightnessPercent));
                        SetBrightness(brightnessPercent);
                        break;
                    case 4:     // display brightness down
                        if (DispBright - 1 < 0)
                        {
                            DispBright = 0;
                        }
                        else
                        {
                            DispBright--;
                        }
                        brightnessPercent = (int)(DispBright / 15f * 100);
                        IPCServer?.PushMessage(new ObcEvent(
                            ObcEventType.DispBright, brightnessPercent));
                        SetBrightness(brightnessPercent);
                        break;
                    case 7:    // keyboard light up
                        if (KeyLight is not null)
                        {
                            if (KeyLight.Brightness + KeyLight.Step > 255)
                            {
                                KeyLight.Brightness = 255;
                            }
                            else
                            {
                                KeyLight.Brightness += KeyLight.Step;
                            }
                            IPCServer?.PushMessage(new ObcEvent(
                            ObcEventType.KeyLightBright, KeyLight.Brightness * 100 / 255));
                        }
                        break;
                    case 8:    // keyboard light down
                        if (KeyLight is not null)
                        {
                            if (KeyLight.Brightness - KeyLight.Step < 0)
                            {
                                KeyLight.Brightness = 0;
                            }
                            else
                            {
                                KeyLight.Brightness -= KeyLight.Step;
                            }
                            IPCServer?.PushMessage(new ObcEvent(
                            ObcEventType.KeyLightBright, KeyLight.Brightness * 100 / 255));
                        }
                        break;
                    case 19:    // keyboard presence detected
                        break;
                    case EVENT_COUNT:   // shut down listener - signalled by Stop()
                        WorkerLog("Stopping event listener...");
                        return;
                    case WaitHandle.WaitTimeout:
                        break;
                    default:
                        WorkerLog(Strings.GetString("warnBadEvent"));
                        break;
                }
            }
        }

        ~KeyboardEventListener()
        {
            Cleanup();
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        private void Cleanup()
        {
            if (CleanupComplete)
            {
                return;
            }

            KeyAgent?.IOControl(AppleKeyboardIOCTL.Unknown1);

            for (int i = 0; i < Events.Length; i++)
            {
                if (Events[i] is not null)
                {
                    if (i < KbdEventIOCtls.Length)
                    {
                        // only dangerous release
                        Events[i].SafeWaitHandle.DangerousRelease();
                    }
                    Events[i].Dispose();
                }
            }
            CleanupComplete = true;
        }

        private void WorkerLog(string message)
        {
            Log.Debug($"[Event Listener] {message}");
        }

        // TODO: run async since it hangs the entire event listener
        // thread while waiting for the optical drive to eject
        // (which can take quite a while in certain circumstances...)
        private static bool EjectOpticalDrive()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            // eject the first optical drive we find
            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType != DriveType.CDRom)
                {
                    continue;
                }

                // remove trailing backslash from drive name
                // since it breaks the rest of the code if present :/
                using (Driver driver = new(drive.Name.Remove(drive.Name.IndexOf('\\'))))
                {
                    if (!driver.Open())
                    {
                        return false;
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        // attempt to lock access to drive. this will
                        // fail if any other app is using the drive
                        if (!driver.IOControl(0x00090018))  // FSCTL_LOCK_VOLUME
                        {
                            if (i >= 9)
                            {
                                return false;
                            }
                            continue;
                        }
                        // dismount drive to prevent access from other apps
                        if (!driver.IOControl(0x00090020))  // FSCTL_DISMOUNT_VOLUME
                        {
                            if (i >= 9)
                            {
                                return false;
                            }
                            continue;
                        }
                        break;
                    }

                    // check if drive supports ejecting
                    byte[] preventRemoval = [0];
                    if (!driver.IOControl(0x002D4804, preventRemoval)) // IOCTL_STORAGE_MEDIA_REMOVAL
                    {
                        return false;
                    }

                    // actually eject the drive
                    return driver.IOControl(0x002D4808);    //IOCTL_STORAGE_EJECT_MEDIA
                }
            }
            return false;
        }

        private static int GetBrightness()
        {
            using (ManagementClass mclass = new("WmiMonitorBrightness")
            {
                Scope = new ManagementScope(@"\\.\root\wmi")
            })
            using (ManagementObjectCollection instances = mclass.GetInstances())
            {
                foreach (ManagementBaseObject instance in instances)
                {
                    return (byte)instance.GetPropertyValue("CurrentBrightness");
                }
                return 0;
            }
        }

        private static void SetBrightness(int brightness)
        {
            using (ManagementClass mclass = new("WmiMonitorBrightnessMethods")
            {
                Scope = new ManagementScope(@"\\.\root\wmi")
            })
            using (ManagementObjectCollection instances = mclass.GetInstances())
            {
                foreach (ManagementBaseObject instance in instances)
                {
                    ((ManagementObject)instance).InvokeMethod(
                        "WmiSetBrightness", [1, brightness]);
                }
            }
        }
    }
}
