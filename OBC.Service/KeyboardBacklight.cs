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

namespace OBC.Service
{
    internal sealed class KeyboardBacklight
    {
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
                SetBrightness(value);
            }
        }

        private byte _brightness;

        /// <summary>
        /// The amount to change <see cref="Brightness"/> by when calling
        /// <see cref="BrightnessUp"/> or <see cref="BrightnessDown"/>.
        /// </summary>
        public byte Step;

        private readonly MacHALDriver HAL;

        public KeyboardBacklight(MacHALDriver hal, byte brightness, byte step = 16)
        {
            HAL = hal;
            Brightness = brightness;
            Step = step;
        }

        public void BrightnessUp()
        {
            if (Brightness + Step > 255)
            {
                Brightness = 255;
            }
            else
            {
                Brightness += Step;
            }
        }

        public void BrightnessDown()
        {
            if (Brightness - Step < 0)
            {
                Brightness = 0;
            }
            else
            {
                Brightness -= Step;
            }
        }

        public bool SetBacklightEnabled(bool enabled)
        {
            return enabled ? SetBrightness(Brightness) : SetBrightness(0);
        }

        private bool SetBrightness(byte brightness)
        {
            byte[] inBuffer =
            [
                // "LKSB", followed by the brightness value
                // (both with null terminators)
                0x4C, 0x4B, 0x53, 0x42, 0x00, brightness, 0x00
            ];
            Console.WriteLine(Strings.GetString("kbdBrightSet", brightness));
            bool success = HAL.IOControl(MacHALDriverIOCTL.Unknown9, inBuffer);

            if (!success)
            {
                Console.WriteLine(Strings.GetString("errIoCtl",
                    HAL.ErrorCode, new Win32Exception(HAL.ErrorCode).Message));
            }
            return success;
        }
    }
}
