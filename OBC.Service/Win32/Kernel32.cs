using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace OBC.Service.Win32
{
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
            string lpFileName,
            uint dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            FileMode dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        internal static extern unsafe bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            void* lpInBuffer,
            uint nInBufferSize,
            void* lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            NativeOverlapped* lpOverlapped);
    }
}
