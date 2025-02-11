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

using System;
using System.ComponentModel;

namespace OBC.Service.Hardware
{
    internal sealed class KeyboardBacklight
    {
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                SetBrightness(_enabled ? _brightness : (byte)0);
            }
        }

        /// <summary>
        /// The brightness of the keyboard backlight,
        /// as a value between 0 and 255.
        /// </summary>
        public byte Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                if (_enabled)
                {
                    SetBrightness(value);
                }
            }
        }

        /// <summary>
        /// The amount to change <see cref="Brightness"/>
        /// by when the keyboard shortcuts are used.
        /// </summary>
        public byte Step { get; set; }

        private bool _enabled = true;
        private byte _brightness;
        private readonly SMC SMC;

        public KeyboardBacklight(SMC smc, byte brightness, byte step = 16)
        {
            SMC = smc;
            Brightness = brightness;
            Step = step;
        }

        private bool SetBrightness(byte brightness)
        {
            Console.WriteLine(Strings.GetString("kbdBrightSet", brightness));
            bool success = SMC.WriteData("LKSB", brightness, 0);

            if (!success)
            {
                int err = SMC.ErrorCode;
                Console.WriteLine(Strings.GetString("errIoCtl",
                    err, new Win32Exception(err).Message));
            }
            return success;
        }
    }
}
