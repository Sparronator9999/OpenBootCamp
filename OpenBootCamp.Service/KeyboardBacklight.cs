using System;
using System.ComponentModel;

namespace OpenBootCamp.Service
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
        public byte Step { get; set; }

        private readonly MacHALDriver HAL;

        public KeyboardBacklight(MacHALDriver hal, byte brightness, byte step = 16)
        {
            HAL = hal;
            Brightness = brightness;
            Step = step;
        }

        public bool BrightnessUp()
        {
            if (Brightness + Step > 255)
                Brightness = 255;
            else
                Brightness += Step;

            return SetBrightness(Brightness);
        }

        public bool BrightnessDown()
        {
            if (Brightness - Step < 0)
                Brightness = 0;
            else
                Brightness -= Step;

            return SetBrightness(Brightness);
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
            Console.WriteLine($"Setting keyboard backlight to {brightness}...");
            bool success = HAL.IOControl(0x9c402458, inBuffer);

            if (!success)
            {
                Console.WriteLine(
                     "DeviceIoControl failed with error:\n" +
                     $"{HAL.ErrorCode}: {new Win32Exception(HAL.ErrorCode).Message}");
            }
            return success;
        }
    }
}
