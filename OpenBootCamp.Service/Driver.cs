using OpenBootCamp.Service.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace OpenBootCamp.Service
{
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
        public bool IsOpen { get; private set; }

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
        public bool Install()
        {
            ErrorCode = 0;

            if (string.IsNullOrEmpty(DriverPath))
            {
                throw new ArgumentException(
                    "The driver path is set to a null or empty string.", DriverPath);
            }

            // Make sure the file we're trying to install actually exists:
            string fullPath = Path.GetFullPath(DriverPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"{fullPath} was not found.", fullPath);
            }

            // Try to open the Service Control Manager:
            IntPtr hSCM = AdvApi32.OpenSCManager(null, null, AdvApi32.SCMAccess.All);
            if (hSCM == IntPtr.Zero)
            {
                // the SCM connection wasn't opened
                // successfully, return false:
                ErrorCode = Marshal.GetLastWin32Error();
                return false;
            }

            // Try to create the service:
            IntPtr hSvc = AdvApi32.CreateService(
                hSCM, DeviceName, DeviceName,
                AdvApi32.ServiceAccess.All,
                AdvApi32.ServiceType.KernelDriver,
                AdvApi32.ServiceStartType.DemandStart,
                AdvApi32.ServiceError.Normal,
                DriverPath, null, null, null, null, null);

            if (hSvc == IntPtr.Zero)
            {
                ErrorCode = Marshal.GetLastWin32Error();
                if (ErrorCode == 1073)  // ERROR_SERVICE_EXISTS
                {
                    hSvc = AdvApi32.OpenService(hSCM, DeviceName, AdvApi32.ServiceAccess.All);
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
            if (!AdvApi32.StartService(hSvc, 0, IntPtr.Zero))
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

            IntPtr hSCM = AdvApi32.OpenSCManager(null, null, AdvApi32.SCMAccess.All);
            if (hSCM == IntPtr.Zero)
            {
                // the SCM connection wasn't opened
                // successfully, return false:
                ErrorCode = Marshal.GetLastWin32Error();
                return false;
            }

            // Try to open the service:
            IntPtr hSvc = AdvApi32.OpenService(hSCM, DeviceName, AdvApi32.ServiceAccess.All);
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
            AdvApi32.ControlService(hSvc, AdvApi32.ServiceControlCode.Stop, out _);
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
                return true;

            ErrorCode = 0;

            if (hDevice == IntPtr.Zero)
            {
                hDevice = Kernel32.CreateFile(
                    $"\\\\.\\{DeviceName}",
                    Kernel32.GenericAccessRights.Read | Kernel32.GenericAccessRights.Write,
                    FileShare.None,
                    IntPtr.Zero,
                    FileMode.Open,
                    FileAttributes.Normal,
                    IntPtr.Zero);

                // Apparently CreateFileW() can return -1 instead of 0 for some reason
                if (hDevice == IntPtr.Zero || hDevice == new IntPtr(-1))
                {
                    ErrorCode = Marshal.GetLastWin32Error();
                    return false;
                }

                IsOpen = true;
                return true;
            }
            return true;
        }

        /// <summary>
        /// Closes the connection to the device driver, if open.
        /// </summary>
        public void Close()
        {
            if (hDevice != IntPtr.Zero)
            {
                Kernel32.CloseHandle(hDevice);
                hDevice = IntPtr.Zero;
                IsOpen = false;
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

        /// <inheritdoc cref="IOControl(uint, void*, uint, void*, uint, out uint)"/>
        public unsafe bool IOControl(uint ctlCode, void* inBuffer, uint inBufSize, void* outBuffer, uint outBufSize)
        {
            return IOControl(ctlCode, inBuffer, inBufSize, outBuffer, outBufSize, out _);
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
            if (hDevice == IntPtr.Zero)
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

        /// <inheritdoc cref="IOControl(uint, void*, uint, out uint, bool)"/>
        public unsafe bool IOControl(uint ctlCode, IntPtr buffer, uint bufSize, out uint bytesReturned, bool isOutBuffer = false)
        {
            return IOControl(ctlCode, buffer.ToPointer(), bufSize, out bytesReturned, isOutBuffer);
        }

        /// <inheritdoc cref="IOControl(uint, void*, uint, void*, uint)"/>
        public unsafe bool IOControl(uint ctlCode, IntPtr inBuffer, uint inBufSize, IntPtr outBuffer, uint outBufSize)
        {
            return IOControl(ctlCode, inBuffer.ToPointer(), inBufSize, outBuffer.ToPointer(), outBufSize, out _);
        }

        /// <inheritdoc cref="IOControl(uint, void*, uint, void*, uint, out uint)"/>
        public unsafe bool IOControl(uint ctlCode, IntPtr inBuffer, uint inBufSize, IntPtr outBuffer, uint outBufSize, out uint bytesReturned)
        {
            return IOControl(ctlCode,
                inBuffer.ToPointer(), inBufSize,
                outBuffer.ToPointer(), outBufSize,
                out bytesReturned);
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

        public bool IOControl(uint ctlCode, byte[] inBuffer, byte[] outBuffer)
        {
            return IOControl(ctlCode, inBuffer, outBuffer, out _);
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

        public bool IOControl<T>(uint ctlCode, ref T inBuffer, out T outBuffer)
            where T : unmanaged
        {
            return IOControl<T, T>(ctlCode, ref inBuffer, out outBuffer, out _);
        }

        public bool IOControl<T>(uint ctlCode, ref T inBuffer, out T outBuffer, out uint bytesReturned)
            where T : unmanaged
        {
            return IOControl<T, T>(ctlCode, ref inBuffer, out outBuffer, out bytesReturned);
        }

        public bool IOControl<TIn, TOut>(uint ctlCode, ref TIn inBuffer, out TOut outBuffer)
            where TIn : unmanaged
            where TOut : unmanaged
        {
            return IOControl(ctlCode, ref inBuffer, out outBuffer, out _);
        }

        public unsafe bool IOControl<TIn, TOut>(uint ctlCode, ref TIn inBuffer, out TOut outBuffer, out uint bytesReturned)
            where TIn : unmanaged
            where TOut : unmanaged
        {
            int inSize = sizeof(TIn);

            fixed (TIn* pInBuffer = &inBuffer)
            fixed (TOut* pOutBuffer = &outBuffer)
            {
                return IOControl(ctlCode,
                    pInBuffer, (uint)inSize,
                    pOutBuffer, (uint)sizeof(TOut),
                    out bytesReturned);
            }
        }


        #region Cleanup code
        ~Driver()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources associated with this <see cref="Driver"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Don't do anything if we already called Dispose:
            if (!IsOpen)
            {
                return;
            }

            // Close all open file and service handles
            Close();

            IsOpen = false;
        }
        #endregion
    }
}
