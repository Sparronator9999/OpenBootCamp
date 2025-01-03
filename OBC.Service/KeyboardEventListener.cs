using OBC.Service.Logs;
using System;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace OBC.Service
{
    internal sealed class KeyboardEventListener : IDisposable
    {
        private const int EVENT_COUNT = 30;

        private static readonly uint[] KbdEventIOCtls =
            new uint[EVENT_COUNT - 1]
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

        private readonly EventWaitHandle[] Events = new AutoResetEvent[EVENT_COUNT];

        private Task ListenerTask;

        private bool CleanupComplete;

        private byte DispBright;

        private readonly AppleKeyboardDriver KeyAgent;
        //private readonly MacHALDriver HAL;

        private readonly KeyboardBacklight KeyLight;

        private readonly Logger Log;

        public KeyboardEventListener(AppleKeyboardDriver keyAgent, MacHALDriver hal, Logger logger, KeyboardBacklight keylight)
        {
            KeyAgent = keyAgent;
            //HAL = hal;
            Log = logger;
            KeyLight = keylight;
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

            ListenerTask = new Task(HandleEvents, CancellationToken.None, TaskCreationOptions.LongRunning);
            ListenerTask.Start();
        }

        public void Stop(bool turnOffKeyLight = true)
        {
            // signal the "stop listener" event
            Events[EVENT_COUNT].Set();

            // wait for the event listener to stop
            ListenerTask.Wait(Timeout.Infinite);

            if (turnOffKeyLight)
            {
                KeyLight?.SetBacklightEnabled(false);
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
                int eventID = WaitHandle.WaitAny(Events, Timeout.Infinite);
                switch (eventID)
                {
                    case 0:     // eject dvd-rom
                        WorkerLog(Strings.GetString("errCDEject"));
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
                        SetBrightness((int)(DispBright / 15f * 100));
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
                        SetBrightness((int)(DispBright / 15f * 100));
                        break;
                    case 7:    // keyboard light up
                        KeyLight?.BrightnessUp();
                        break;
                    case 8:    // keyboard light down
                        KeyLight?.BrightnessDown();
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

        private static int GetBrightness()
        {
            using ManagementClass mclass = new("WmiMonitorBrightness")
            {
                Scope = new ManagementScope(@"\\.\root\wmi")
            };
            using ManagementObjectCollection instances = mclass.GetInstances();
            foreach (ManagementObject instance in instances.Cast<ManagementObject>())
            {
                return (byte)instance.GetPropertyValue("CurrentBrightness");
            }
            return 0;
        }

        private static void SetBrightness(int brightness)
        {
            using ManagementClass mclass = new("WmiMonitorBrightnessMethods")
            {
                Scope = new ManagementScope(@"\\.\root\wmi")
            };
            using ManagementObjectCollection instances = mclass.GetInstances();
            object[] args = [1, brightness];
            foreach (ManagementObject instance in instances.Cast<ManagementObject>())
            {
                instance.InvokeMethod("WmiSetBrightness", args);
            }
        }
    }
}
