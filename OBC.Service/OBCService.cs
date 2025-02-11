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

using Microsoft.Win32;
using OBC.Common;
using OBC.Config;
using OBC.Service.Logs;
using System.ComponentModel;
using System.ServiceProcess;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using OBC.Service.Hardware;

namespace OBC.Service
{
    internal sealed class OBCService : ServiceBase
    {
        private readonly Logger Log;

        private readonly AppleKeyboardDriver KeyAgent = new("KeyAgent");
        private readonly AppleKeyboardDriver KeyMagic = new("AppleKeyboard");
        private readonly SMC SMC = new("MacHALDriver");

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
            if (KeyMagic.Open())
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
            else
            {
                Log.Error("Failed to connect to Keymagic.sys!\n" +
                    "Most (if not all) of OBC's functionality will not work!");
            }

            if (SMC.Open())
            {
                List<string> keys = SMC.GetSupportedKeys();
                if (keys is null || keys.Count == 0)
                {
                    Log.Error(
                        "Failed to get supported SMC keys:\n" +
                        $"{new Win32Exception(SMC.ErrorCode)} ({SMC.ErrorCode})");
                }
                else
                {
                    if (keys.Contains("LKSB"))
                    {
                        KeyLight = new(SMC, Config.KeyboardBrightness, Config.KeyboardBrightnessStep);
                    }
                    else
                    {
                        Log.Warn("SMC reports that keyboard backlight is not supported. Keyboard backlight adjustments will be unavailable.");
                    }
                }
            }
            else
            {
                Log.Warn("Failed to connect to MacHALDriver.sys!\n" +
                    "Keyboard backlight adjustments will be unavailable!");
            }

            if (KeyAgent.Open())
            {
                Listener = new(KeyAgent, Log, KeyLight);
                Listener.Start();
            }
            else
            {
                Log.Warn(
                    "Failed to connect to KeyAgent.sys!\n" +
                    "Some special Fn keys will not work!");
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
            if (KeyLight != null)
            {
                Config.KeyboardBrightness = KeyLight.Brightness;
            }
            Config.Save(ConfPath);

            Log.Info("Unloading drivers...");
            KeyAgent?.Close();
            KeyMagic?.Close();
            SMC?.Close();
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
                    if (KeyLight is not null)
                    {
                        KeyLight.Enabled = true;
                    }
                    break;
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
