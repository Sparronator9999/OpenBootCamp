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

using OBC.Common.Win32;
using System;
using System.IO;
using System.Messaging;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.ServiceProcess;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace OBC.Common;

/// <summary>
/// Contains functions to install and manage kernel-level device drivers.
/// </summary>
public class Driver : IDisposable
{
    private readonly string DeviceName;
    private readonly string DriverPath = string.Empty;
    private IntPtr hDevice;

    /// <summary>
    /// Gets whether the driver connection is open.
    /// </summary>
    public bool IsOpen => hDevice != IntPtr.Zero && hDevice != (IntPtr)(-1);

    /// <summary>
    /// Gets whether the driver is installed to the computer.
    /// </summary>
    /// <remarks>
    /// This will be <c>false</c> if the driver has not been
    /// installed by this instance of the <see cref="Driver"/>,
    /// even if it is actaully installed to the system.
    /// </remarks>
    public bool IsInstalled { get; private set; }

    /// <summary>
    /// The underlying Win32 Error code generated
    /// by the last called method in this class instance.
    /// </summary>
    public int ErrorCode { get; private set; }

    /// <summary>
    /// Create an instance of the <see cref="Driver"/>
    /// class with the specified name and driver path,
    /// automatically installing the driver to the system.
    /// </summary>
    /// <param name="name">
    /// <para>The name of the device created by the driver.</para>
    /// <para>
    /// If (un)installing the driver, this
    /// will also be used asthe service ID.
    /// </para>
    /// </param>
    public Driver(string name)
    {
        DeviceName = name;
    }

    /// <summary>
    /// Create an instance of the <see cref="Driver"/>
    /// class with the specified name and driver path,
    /// automatically installing the driver to the system.
    /// </summary>
    /// <param name="name">
    /// The driver name. This will be used as the device driver service name.
    /// </param>
    /// <param name="path">
    /// The path to the driver file (C:\path\to\driver.sys).
    /// </param>
    public Driver(string name, string path)
    {
        DeviceName = name;
        DriverPath = path;
    }

    /// <summary>
    /// Installs the driver on the local computer.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the driver was installed
    /// successfully, otherwise <c>false</c>.
    /// </returns>
    public bool Install(ServiceStartMode startMode = ServiceStartMode.Manual)
    {
        ErrorCode = 0;

        if (string.IsNullOrEmpty(DriverPath))
        {
            throw new ArgumentException(
                Strings.GetString("drvNullPath"), DriverPath);
        }

        // Make sure the file we're trying to install actually exists:
        string fullPath = Path.GetFullPath(DriverPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(Strings.GetString("drvNotFound"), fullPath);
        }

        // Try to open the Service Control Manager:
        IntPtr hSCM = AdvApi32.OpenSCManagerW(null, null, 0xF003F);
        if (hSCM == IntPtr.Zero)
        {
            // the SCM connection wasn't opened
            // successfully, return false:
            ErrorCode = Marshal.GetLastWin32Error();
            return false;
        }

        // Try to create the service:
        IntPtr hSvc = AdvApi32.CreateServiceW(
            hSCM, DeviceName, DeviceName, 0xF01FF, ServiceType.KernelDriver,
            startMode, 1, DriverPath, null, null, null, null, null);

        if (hSvc == IntPtr.Zero)
        {
            ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode == 1073)  // ERROR_SERVICE_EXISTS
            {
                hSvc = AdvApi32.OpenServiceW(hSCM, DeviceName, 0xF01FF);
                if (hSvc == IntPtr.Zero)
                {
                    ErrorCode = Marshal.GetLastWin32Error();
                    AdvApi32.CloseServiceHandle(hSCM);
                    return false;
                }
            }
            else
            {
                ErrorCode = Marshal.GetLastWin32Error();
                AdvApi32.CloseServiceHandle(hSCM);
                return false;
            }
        }
        IsInstalled = true;

        // Try to start the service:
        if (!AdvApi32.StartServiceW(hSvc, 0, IntPtr.Zero))
        {
            int error = Marshal.GetLastWin32Error();
            if (error != 1056)  // ERROR_SERVICE_ALREADY_RUNNING
            {
                ErrorCode = error;
                AdvApi32.CloseServiceHandle(hSvc);
                AdvApi32.CloseServiceHandle(hSCM);
                return false;
            }
        }

        // Perform some cleanup:
        AdvApi32.CloseServiceHandle(hSvc);
        AdvApi32.CloseServiceHandle(hSCM);

        // Security fix for WinRing0 access from unprivileged processes.
        // This fix is present in the WinRing0 driver itself (WinRing0.sys)
        // in an updated fork (https://github.com/GermanAizek/WinRing0), but no
        // public production-signed build of the driver exists with the fixes.
        // This fix was "borrowed" from OpenHardwareMonitor:
        // https://github.com/openhardwaremonitor/openhardwaremonitor/
        FileInfo fi = new($"\\\\.\\{DeviceName}");
        FileSecurity security = fi.GetAccessControl();
        security.SetSecurityDescriptorSddlForm("O:BAG:SYD:(A;;FA;;;SY)(A;;FA;;;BA)");
        fi.SetAccessControl(security);

        return true;
    }

    /// <summary>
    /// Uninstalls the driver from the local computer.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the driver was uninstalled
    /// successfully, otherwise <c>false</c>.
    /// </returns>
    public bool Uninstall()
    {
        ErrorCode = 0;

        // Close the driver file handle (if it's open)
        Close();

        IntPtr hSCM = AdvApi32.OpenSCManagerW(null, null, 0xF003F);
        if (hSCM == IntPtr.Zero)
        {
            // the SCM connection wasn't opened
            // successfully, return false:
            ErrorCode = Marshal.GetLastWin32Error();
            return false;
        }

        // Try to open the service:
        IntPtr hSvc = AdvApi32.OpenServiceW(hSCM, DeviceName, 0xF01FF);
        if (hSvc == IntPtr.Zero)
        {
            // Ignore ERROR_SERVICE_DOES_NOT_EXIST:
            int error = Marshal.GetLastWin32Error();
            bool success = error == 1060;
            if (success)
            {
                IsInstalled = false;
            }
            else
            {
                ErrorCode = error;
            }

            AdvApi32.CloseServiceHandle(hSCM);
            return success;
        }

        // Stop and delete the service:
        AdvApi32.ControlService(hSvc, 1, IntPtr.Zero);
        AdvApi32.DeleteService(hSvc);
        IsInstalled = false;

        // Close service handles
        AdvApi32.CloseServiceHandle(hSvc);
        AdvApi32.CloseServiceHandle(hSCM);
        return true;
    }

    /// <summary>
    /// Opens a connection to the driver.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the driver connection was
    /// opened successfully, otherwise <c>false</c>.
    /// </returns>
    public bool Open()
    {
        if (IsOpen)
        {
            return true;
        }

        ErrorCode = 0;

        if (!IsOpen)
        {
            hDevice = Kernel32.CreateFileW(
                $"\\\\.\\{DeviceName}",
                GenericAccessRights.Read | GenericAccessRights.Write,
                FileShare.ReadWrite,
                IntPtr.Zero,
                FileMode.Open,
                FileAttributes.Normal,
                IntPtr.Zero);

            if (!IsOpen)
            {
                ErrorCode = Marshal.GetLastWin32Error();
                return false;
            }
            return true;
        }
        return true;
    }

    /// <summary>
    /// Closes the connection to the device driver, if open.
    /// </summary>
    public void Close()
    {
        if (IsOpen)
        {
            Kernel32.CloseHandle(hDevice);
            hDevice = IntPtr.Zero;
        }
    }


    /// <inheritdoc cref="IOControl(uint, void*, uint, void*, uint, out uint)"/>
    public unsafe bool IOControl(uint ctlCode)
    {
        return IOControl(ctlCode, null, 0, null, 0, out _);
    }

    /// <inheritdoc cref="IOControl(uint, void*, uint, out uint, bool)"/>
    public unsafe bool IOControl(uint ctlCode, void* buffer, uint bufSize, bool isOutBuffer = false)
    {
        return IOControl(ctlCode, buffer, bufSize, out _, isOutBuffer);
    }

    /// <param name="buffer">
    /// <para>
    /// A pointer to the buffer that will be passed to the driver.
    /// </para>
    /// <para>
    /// Whether the buffer is passed as an input or output buffer is
    /// dependent on the setting of <paramref name="isOutBuffer"/>.
    /// </para>
    /// </param>
    /// <param name="bufSize">
    /// The size of the buffer, in bytes.
    /// </param>
    /// <param name="isOutBuffer">
    /// <para>
    /// Set to <c>true</c> to pass the provided
    /// buffer to the driver as an output buffer.
    /// </para>
    /// <para>
    /// Set to <c>false</c> to pass the buffer
    /// to the driver as an input buffer.
    /// </para>
    /// <para>The default is <c>false</c>.</para>
    /// </param>
    /// <inheritdoc cref="IOControl(uint, void*, uint, void*, uint, out uint)"/>
    public unsafe bool IOControl(uint ctlCode, void* buffer, uint bufSize, out uint bytesReturned, bool isOutBuffer = false)
    {
        return isOutBuffer
            ? IOControl(ctlCode, null, 0, buffer, bufSize, out bytesReturned)
            : IOControl(ctlCode, buffer, bufSize, null, 0, out bytesReturned);
    }

    /// <summary>
    /// Sends a control code to the driver, causing the
    /// corresponding operation to be performed by the driver.
    /// </summary>
    /// <param name="ctlCode">
    /// The control code that represents the
    /// operation that the driver should perform.
    /// </param>
    /// <param name="inBuffer">
    /// A pointer to the buffer that contains the
    /// data required to perform the operation.
    /// </param>
    /// <param name="inBufSize">
    /// The size of the input buffer, in bytes.
    /// </param>
    /// <param name="outBuffer">
    /// A pointer to the buffer that will receive the
    /// data returned by the operation.
    /// </param>
    /// <param name="outBufSize">
    /// The size of the output buffer, in bytes.
    /// </param>
    /// <param name="bytesReturned">
    /// The variable that will receive the size of the data stored
    /// in the output buffer returned by the driver, in bytes.
    /// </param>
    /// <returns>
    /// <c>true</c> if the operation completed
    /// successfully, otherwise <c>false</c>.
    /// </returns>
    public unsafe bool IOControl(uint ctlCode, void* inBuffer, uint inBufSize, void* outBuffer, uint outBufSize, out uint bytesReturned)
    {
        if (!IsOpen)
        {
            bytesReturned = 0;
            return false;
        }

        bool success = Kernel32.DeviceIoControl(
            hDevice, ctlCode,
            inBuffer, inBufSize,
            outBuffer, outBufSize,
            out bytesReturned, null);

        ErrorCode = success
            ? 0
            : Marshal.GetLastWin32Error();

        return success;
    }


    /// <inheritdoc cref="IOControl(uint, void*, uint, bool)"/>
    public unsafe bool IOControl(uint ctlCode, IntPtr buffer, uint bufSize, bool isOutBuffer = false)
    {
        return IOControl(ctlCode, buffer.ToPointer(), bufSize, isOutBuffer);
    }

    /// <param name="buffer">
    /// <para>
    /// The buffer that will be passed to the driver.
    /// </para>
    /// <para>
    /// Whether the buffer is passed as an input or output buffer is
    /// dependent on the setting of <paramref name="isOutBuffer"/>.
    /// </para>
    /// </param>
    /// <inheritdoc cref="IOControl(uint, void*, uint, bool)"/>
    public bool IOControl(uint ctlCode, byte[] buffer, bool isOutBuffer = false)
    {
        return IOControl(ctlCode, buffer, out _, isOutBuffer);
    }

    public bool IOControl(uint ctlCode, byte[] buffer, out uint bytesReturned, bool isOutBuffer = false)
    {
        return isOutBuffer
            ? IOControl(ctlCode, null, buffer, out bytesReturned)
            : IOControl(ctlCode, buffer, null, out bytesReturned);
    }

    public unsafe bool IOControl(uint ctlCode, byte[] inBuffer, byte[] outBuffer, out uint bytesReturned)
    {
        fixed (byte* pInBuffer = inBuffer)
        fixed (byte* pOutBuffer = outBuffer)
        {
            return IOControl(
                ctlCode,
                pInBuffer, inBuffer is null ? 0 : (uint)inBuffer.Length,
                pOutBuffer, outBuffer is null ? 0 : (uint)outBuffer.Length,
                out bytesReturned);
        }
    }

    public bool IOControl<T>(uint ctlCode, ref T buffer, bool isOutBuffer = false)
        where T : unmanaged
    {
        return IOControl(ctlCode, ref buffer, out _, isOutBuffer);
    }

    public unsafe bool IOControl<T>(uint ctlCode, ref T buffer, out uint bytesReturned, bool isOutBuffer = false)
        where T : unmanaged
    {
        fixed (T* pBuffer = &buffer)
        {
            return isOutBuffer
                ? IOControl(ctlCode,
                    null, 0,
                    pBuffer, (uint)sizeof(T),
                    out bytesReturned)
                : IOControl(ctlCode,
                    pBuffer, (uint)sizeof(T),
                    null, 0,
                    out bytesReturned);
        }
    }

    ~Driver()
    {
        Close();
    }



#pragma warning disable IDE0079 // IDE0079: Remove unnecessary suppression
#pragma warning disable CA1816  // CA1816: Dispose methods should call SuppressFinalize (the Driver may be re-opened after calling Dispose)
    public void Dispose()
#pragma warning restore CA1816
#pragma warning restore IDE0079
    {
        Close();
    }
}
