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

using OBC.Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace OBC.Overlays;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        // multi-instance detection
        // NOTE: GUID is used to prevent conflicts with potential
        // identically named but different program
        // based on: https://stackoverflow.com/a/184143
        using (Mutex mutex = new(true, "{93a501d4-386d-4eb6-a26c-bc3e76bb10c0}", out bool createdNew))
        {
            // this instance is the first to open; proceed as normal:
            if (createdNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(args.Length > 0 && args[0] == "--startup"));
                return;
            }

            // the overlay app is already running,
            // ask if we should close it
            Process current = Process.GetCurrentProcess();
            foreach (Process p in Process.GetProcessesByName(current.ProcessName))
            {
                if (p.Id == current.Id)
                {
                    continue;
                }

                if (Utils.ShowWarning(
                    $"The OBC overlay service is already running! (PID: {p.Id})\n" +
                    $"Would you like to kill it?", "Already running") == DialogResult.Yes)
                {
                    p.Kill();
                }
                break;
            }
        }
    }
}
