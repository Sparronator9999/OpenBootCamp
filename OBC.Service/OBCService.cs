using Microsoft.Win32;
using OBC.Service.Logs;
using OBC.Config;
using System.ComponentModel;
using System.ServiceProcess;
using System.IO;
using System;

namespace OBC.Service
{
    internal sealed class OBCService : ServiceBase
    {
        private readonly Logger Log;

        private readonly AppleKeyboardDriver KeyAgent = new("KeyAgent");
        private readonly AppleKeyboardDriver KeyMagic = new("AppleKeyboard");
        private readonly MacHALDriver HAL = new("MacHALDriver");

        private KeyboardEventListener Listener;
        private KeyboardBacklight KeyLight;

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
            Log.Info("Starting the OpenBootCamp service...");

            LoadConf();

            Log.Info("Initialising drivers...");
            if (OpenDriver(KeyMagic, "Keymagic"))
            {
                Log.Info("Setting OSXFnBehavior from registry key...");

                RegistryKey aksKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Apple Inc.\Apple Keyboard Support");
                int value = 0;
                if (aksKey is not null)
                {
                    value = (int)aksKey.GetValue("OSXFnBehavior", 0);
                    aksKey.Close();
                }

                if (!KeyMagic.IOControl(AppleKeyboardIOCTL.SetOSXFnBehaviour, ref value))
                {
                    Log.Error("Failed to set OSXFnBehavior!");
                    LogIoControlError(KeyMagic);
                }

                value = Config.UseAcpiBrightness ? 1 : 0;
                if (!KeyMagic.IOControl(AppleKeyboardIOCTL.AcpiBrightnessAvailable, ref value))
                {
                    Log.Error("Failed to make an IOCTL_ACPI_BRIGHTNESS_AVAILABLE call to Keymagic!");
                    LogIoControlError(KeyMagic);
                }
            }

            if (OpenDriver(KeyAgent, "KeyAgent") && OpenDriver(HAL, "MacHALDriver"))
            {
                KeyLight = new(HAL, Config.KeyboardBrightness, Config.KeyboardBrightnessStep);
                Listener = new(KeyAgent, HAL, Log, KeyLight);
                Listener.Start();
            }
            Log.Info("Started the OpenBootCamp service.");
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
            Log.Info("Stopping the OpenBootCamp service...");
            Listener?.Stop();

            Log.Info("Saving config...");
            Config.KeyboardBrightness = KeyLight.Brightness;
            Config.Save(ConfPath);

            Log.Info("Unloading drivers...");
            KeyAgent?.Close();
            KeyMagic?.Close();
            HAL?.Close();
            Log.Info("The OpenBootCamp service has stopped successfully.");
        }

        //protected override void OnPause() { }

        //protected override void OnContinue() { }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.ResumeCritical:
                case PowerBroadcastStatus.ResumeSuspend:
                case PowerBroadcastStatus.ResumeAutomatic:
                    KeyLight?.SetBacklightEnabled(true);
                    break;
            }
            return true;
        }

        private bool OpenDriver(Driver driver, string driverName)
        {
            Log.Info($"Attempting to connect to {driverName}.sys...");
            if (!driver.Open())
            {
                Log.Error($"Failed to connect to {driverName}.sys!");
                return false;
            }
            return true;
        }

        private void LogIoControlError(Driver driver)
        {
            Log.Info($"DeviceIoControl failed with error {driver.ErrorCode} " +
                $"({new Win32Exception(driver.ErrorCode).Message})");
        }

        private void LoadConf()
        {
            Log.Info("Loading config (obc.xml)...");

            if (File.Exists(ConfPath))
            {
                try
                {
                    Config = ObcConfig.Load(ConfPath);
                }
                catch (InvalidConfigException)
                {
                    Log.Warn(
                        "Invalid OpenBootCamp config!\n" +
                        "Using default settings...");
                    Config = new ObcConfig(true, 0, 16);
                }

                Log.Info("Loaded the OpenBootCamp config successfully!");
            }
            else
            {
                Log.Warn(
                    "OpenBootCamp config not found!\n" +
                    "The default settings will be used.");
                Config = new ObcConfig(true, 0, 16);
            }
        }
    }
}
