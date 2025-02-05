using System;
using System.Runtime.InteropServices;

namespace OBC.Service.Win32
{
    /// <summary>
    /// Wraps native Win32 functions from <c>advapi32.dll</c>.
    /// </summary>
    internal static class AdvApi32
    {
        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true,
            CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenSCManagerW(
            string lpMachineName,
            string lpDatabaseName,
            uint dwDesiredAccess);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true,
            CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateServiceW(
            IntPtr hSCManager,
            string lpServiceName,
            string lpDisplayName,
            uint dwDesiredAccess,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            string lpdwTagId,
            string lpDependencies,
            string lpServiceStartName,
            string lpPassword);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true,
            CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenServiceW(
            IntPtr hSCManager,
            string lpServiceName,
            uint dwDesiredAccess);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true,
            CharSet = CharSet.Unicode)]
        internal static extern bool StartServiceW(
            IntPtr hService,
            uint dwNumServiceArgs,
            IntPtr lpServiceArgVectors);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ControlService(
            IntPtr hService,
            uint dwControl,
            IntPtr lpServiceStatus);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
        internal static extern bool DeleteService(IntPtr hService);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
        internal static extern bool CloseServiceHandle(IntPtr hSCObject);
    }
}
