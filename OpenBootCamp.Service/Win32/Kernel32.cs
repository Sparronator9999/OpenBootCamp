using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenBootCamp.Service.Win32
{
    /// <summary>
    /// Wraps native Win32 functions from <c>kernel32.dll</c>.
    /// </summary>
    internal static class Kernel32
    {
        [DllImport("kernel32.dll",
            ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode, ExactSpelling = true,
            EntryPoint = "CreateFileW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] GenericAccessRights dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            [Optional] IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            [Optional] IntPtr hTemplateFile);

        [DllImport("kernel32.dll",
            ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            [Optional] void* lpInBuffer,
            uint nInBufferSize,
            [Optional] void* lpOutBuffer,
            uint nOutBufferSize,
            [Optional] out uint lpBytesReturned,
            [Optional] NativeOverlapped* lpOverlapped);

        internal enum GenericAccessRights : uint
        {
            None = 0,
            All = 0x10000000,
            Execute = 0x20000000,
            Write = 0x40000000,
            Read = 0x80000000,
        }
    }
}
