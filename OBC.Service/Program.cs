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

using OBC.Service.Logs;
using System;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;

namespace OBC.Service
{
    internal static class Program
    {
        private static readonly Logger Log = new()
        {
            ConsoleLevel = LogLevel.NONE,
            FileLevel = LogLevel.DEBUG,
            LogDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Sparronator9999", "OpenBootCamp", "Logs"),
        };

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            if (Environment.UserInteractive)
            {
                MessageBox.Show(Strings.GetString("errDirectRun"), "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(LogUnhandledException);
                Log.Info(Strings.GetString("svcVer", Environment.OSVersion));

                ServiceBase.Run(new OBCService(Log));
            }
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e) =>
            Log.Fatal(Strings.GetString("svcException", e.ExceptionObject));
    }
}
