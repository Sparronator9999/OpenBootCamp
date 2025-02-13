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

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace OBC.Common;

/// <summary>
/// A collection of miscellaneous useful utilities
/// </summary>
public static class Utils
{
    /// <summary>
    /// Shows an information dialog.
    /// </summary>
    /// <param name="message">
    /// The message to show in the info dialog.
    /// </param>
    /// <param name="title">
    /// The text to show in the title bar of the dialog.
    /// </param>
    /// <param name="buttons">
    /// One of the <see cref="MessageBoxButtons"/> values
    /// that specifies which buttons to display in the dialog.
    /// </param>
    /// <returns>
    /// One of the <see cref="DialogResult"/> values.
    /// </returns>
    public static DialogResult ShowInfo(string message, string title,
        MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        return MessageBox.Show(message, title, buttons, MessageBoxIcon.Asterisk);
    }

    /// <summary>
    /// Shows a warning dialog.
    /// </summary>
    /// <param name="message">
    /// The message to show in the warning dialog.
    /// </param>
    /// <param name="title">
    /// The text to show in the title bar of the dialog.
    /// </param>
    /// <param name="button">
    /// One of the <see cref="MessageBoxDefaultButton"/> values
    /// that specifies the default button for the dialog.
    /// </param>
    /// <returns>
    /// One of the <see cref="DialogResult"/> values.
    /// </returns>
    public static DialogResult ShowWarning(string message, string title,
        MessageBoxDefaultButton button = MessageBoxDefaultButton.Button1)
    {
        return MessageBox.Show(message, title, MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning, button);
    }

    /// <summary>
    /// Shows an error dialog.
    /// </summary>
    /// <param name="message">
    /// The message to show in the error dialog.
    /// </param>
    /// <returns>
    /// One of the <see cref="DialogResult"/> values.
    /// </returns>
    public static DialogResult ShowError(string message)
    {
        return MessageBox.Show(message, "Error",
            MessageBoxButtons.OK, MessageBoxIcon.Stop);
    }

    public static Icon GetEntryAssemblyIcon()
    {
        return Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
    }

    /// <summary>
    /// Gets whether the application is running with administrator privileges.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the application is running as
    /// an administrator, otherwise <see langword="false"/>.
    /// </returns>
    public static bool IsAdmin()
    {
        try
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Installs the specified .NET Framework
    /// service to the local computer.
    /// </summary>
    /// <remarks>
    /// The service is not started automatically. Use
    /// <see cref="StartService(string)"/> to start it if needed.
    /// </remarks>
    /// <param name="svcExe">
    /// The path to the service executable.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the service installation
    /// was successful, otherwise <see langword="false"/>.
    /// </returns>
    public static bool InstallNETService(string svcExe)
    {
        string runtimePath = RuntimeEnvironment.GetRuntimeDirectory();
        int exitCode = RunCmd($"{runtimePath}\\installutil.exe", $"\"{svcExe}.exe\"");
        DeleteInstallUtilLogs();
        return exitCode == 0;
    }

    /// <summary>
    /// Uninstalls the specified .NET Framework
    /// service from the local computer.
    /// </summary>
    /// <param name="svcExe">
    /// The path to the service executable.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the service uninstallation
    /// was successful, otherwise <see langword="false"/>.
    /// </returns>
    public static bool UninstallNETService(string svcExe)
    {
        string runtimePath = RuntimeEnvironment.GetRuntimeDirectory();
        int exitCode = RunCmd($"{runtimePath}\\installutil.exe", $"/u \"{svcExe}.exe\"");
        DeleteInstallUtilLogs();
        return exitCode == 0;
    }

    /// <summary>
    /// Starts the specified service.
    /// </summary>
    /// <param name="svcName">
    /// The service name, as shown in <c>services.msc</c>
    /// (NOT to be confused with its display name).
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the service started successfully
    /// (or is already running), otherwise <see langword="false"/>.
    /// </returns>
    public static bool StartService(string svcName)
    {
        return RunCmd("net", $"start {svcName}") == 0;
    }

    /// <summary>
    /// Stops the specified service.
    /// </summary>
    /// <param name="svcName">
    /// The service name, as shown in <c>services.msc</c>
    /// (NOT to be confused with its display name).
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the service was stopped successfully
    /// (or is already stopped), otherwise <see langword="false"/>.
    /// </returns>
    public static bool StopService(string svcName)
    {
        return RunCmd("net", $"stop {svcName}") == 0;
    }

    private static void DeleteInstallUtilLogs()
    {
        foreach (string file in Directory.GetFiles(".", "*.InstallLog", SearchOption.TopDirectoryOnly))
        {
            try
            {
                File.Delete(file);
            }
            catch (DirectoryNotFoundException) { }
        }
    }

    /// <summary>
    /// Runs the specified executable as admin,
    /// with the specified arguments.
    /// </summary>
    /// <remarks>
    /// The process will be started with <see cref="ProcessStartInfo.UseShellExecute"/>
    /// set to <see langword="false"/>, except if the calling application is not
    /// running as an administrator, in which case
    /// <see cref="ProcessStartInfo.UseShellExecute"/> is set to
    /// <see langword="true"/> instead.
    /// </remarks>
    /// <param name="exe">
    /// The path to the executable to run.
    /// </param>
    /// <param name="args">
    /// The arguments to pass to the executable.
    /// </param>
    /// <param name="waitExit">
    /// <see langword="true"/> to wait for the executable to exit
    /// before returning, otherwise <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The exit code returned by the executable (unless <paramref name="waitExit"/>
    /// is <see langword="true"/>, in which case 0 will always be returned).
    /// </returns>
    /// <exception cref="Win32Exception"/>
    public static int RunCmd(string exe, string args, bool waitExit = true)
    {
        bool shellExecute = false;
        if (!IsAdmin())
        {
            // if running unprivileged, we can't create an admin process
            // directly, so use shell execute (creating new cmd window) instead
            shellExecute = true;
        }

        using (Process p = new()
        {
            StartInfo = new ProcessStartInfo(exe, args)
            {
                CreateNoWindow = true,
                UseShellExecute = shellExecute,
                Verb = "runas",
            },
        })
        {
            p.Start();
            if (waitExit)
            {
                p.WaitForExit();
                return p.ExitCode;
            }
        }
        return 0;
    }

    public static string GetWin32ErrMsg(int err)
    {
        return $"{new Win32Exception(err).Message} ({err})";
    }
}
