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
using System.ServiceProcess;

namespace OBC.Common.Win32;

/// <summary>
/// Wraps native Win32 functions from <c>advapi32.dll</c>.
/// </summary>
internal static class AdvApi32
{
    [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true,
        CharSet = CharSet.Unicode)]
    internal static extern IntPtr OpenSCManagerW(
        string machineName,
        string databaseName,
        uint desiredAccess);

    [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true,
        CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateServiceW(
        IntPtr hSCManager,
        string svcName,
        string displayName,
        uint desiredAccess,
        ServiceType svcType,
        ServiceStartMode startType,
        uint errMode,
        string binPath,
        string loadOrderGroup,
        string tagId,
        string deps,
        string userName,
        string password);

    [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true,
        CharSet = CharSet.Unicode)]
    internal static extern IntPtr OpenServiceW(
        IntPtr hSCManager,
        string svcName,
        uint desiredAccess);

    [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true,
        CharSet = CharSet.Unicode)]
    internal static extern bool StartServiceW(
        IntPtr hService,
        uint argsLen,
        IntPtr args);

    [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
    internal static extern bool ControlService(
        IntPtr hService,
        uint ctlCode,
        IntPtr svcStatus);

    [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
    internal static extern bool DeleteService(IntPtr hService);

    [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
    internal static extern bool CloseServiceHandle(IntPtr hSCObject);
}
