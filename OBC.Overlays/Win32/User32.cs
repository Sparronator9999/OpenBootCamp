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
using System.Runtime.InteropServices;

namespace OBC.Overlays.Win32
{
    internal static class User32
    {
        internal static bool SetBlur(IntPtr hWnd, bool enable)
        {
            // TODO: support windows versions other than 10 (and probably 11)
            AccentPolicy accentPolicy = new()
            {
                AccentState = enable ? 4 : 0,
                AccentFlags = 0,
                Color = 0x01000000,
                AnimationId = 0
            };
            int accentSize = Marshal.SizeOf(accentPolicy);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentSize);
            try
            {
                Marshal.StructureToPtr(accentPolicy, accentPtr, false);

                WindowCompositionAttributeData data = new()
                {
                    Attribute = 19,
                    Data = accentPtr,
                    SizeOfData = accentSize,
                };
                return SetWindowCompositionAttribute(hWnd, ref data) == 0;
            }
            finally
            {
                Marshal.FreeHGlobal(accentPtr);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public uint Color;
            public int AnimationId;
        }

        [DllImport("User32", ExactSpelling = true, SetLastError = true)]
        private static extern int SetWindowCompositionAttribute(
            IntPtr hWnd,
            ref WindowCompositionAttributeData data);
    }
}
