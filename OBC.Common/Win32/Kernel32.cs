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
using System.IO;
using System.Messaging;
using System.Runtime.InteropServices;

namespace OBC.Common.Win32;

/// <summary>
/// Wraps native Win32 functions from <c>kernel32.dll</c>.
/// </summary>
internal static class Kernel32
{
    [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
    internal static extern bool CloseHandle(IntPtr hObject);

    [DllImport("Kernel32", ExactSpelling = true, SetLastError = true,
        CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateFileW(
        string name,
        GenericAccessRights desiredAccess,
        FileShare shareMode,
        IntPtr securityAttr,
        FileMode createMode,
        FileAttributes attr,
        IntPtr template);

    [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
    internal static extern unsafe bool DeviceIoControl(
        IntPtr hDevice,
        uint ctlCode,
        void* inBuffer,
        uint inBufSize,
        void* outBuffer,
        uint outBufSize,
        out uint bytesReturned,
        IntPtr lpOverlapped);
}
