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

using Microsoft.Win32;
using OBC.Common;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OBC.InstallManager;

public partial class MainForm : Form
{
    private enum ServiceStatus
    {
        NotInstalled = 0,
        Installed = 1,
        Running = 2,
    }

    public MainForm()
    {
        InitializeComponent();
        Icon = Utils.GetEntryAssemblyIcon();

        ToolTip tt = new();
        tt.SetToolTip(lblOBCS, Strings.GetString("ttOBCS"));
        tt.SetToolTip(lblOBCSState, Strings.GetString("ttOBCS"));
        tt.SetToolTip(lblOBCO, Strings.GetString("ttOBCO"));
        tt.SetToolTip(lblOBCOState, Strings.GetString("ttOBCO"));
        tt.SetToolTip(lblKA, Strings.GetString("ttKA"));
        tt.SetToolTip(lblKAState, Strings.GetString("ttKA"));
        tt.SetToolTip(lblMHD, Strings.GetString("ttMHD"));
        tt.SetToolTip(lblMHDState, Strings.GetString("ttMHD"));

        SetStatusLabel("obcsvc", false, lblOBCSState, btnOBCSInstall, btnOBCSStart);
        SetStatusLabel("KeyAgent", true, lblKAState, btnKAInstall);
        SetStatusLabel("MacHALDriver", true, lblMHDState, btnMHDInstall);

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
        {
            if (key.GetValue("ObcOverlay") is null)
            {
                lblOBCOState.ForeColor = Color.Maroon;
                lblOBCOState.Text = "Not installed";
                btnOBCOInstall.Text = "Install";
            }
            else
            {
                lblOBCOState.ForeColor = Color.DarkGreen;
                lblOBCOState.Text = "Installed";
                btnOBCOInstall.Text = "Uninstall";
            }
        }
    }

    private void tsiExit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private static void SetStatusLabel(string svcName, bool driver, Label lbl, Button installBtn, Button startBtn = null)
    {
        switch (GetServiceStatus(svcName, driver))
        {
            case ServiceStatus.Running:
                if (driver)
                {
                    lbl.ForeColor = Color.DarkGreen;
                    lbl.Text = "Installed";
                }
                else
                {
                    lbl.ForeColor = Color.Green;
                    lbl.Text = "Running";
                }
                installBtn.Text = "Uninstall";
                if (startBtn is not null)
                {
                    startBtn.Enabled = true;
                    startBtn.Text = "Stop";
                }
                break;
            case ServiceStatus.Installed:
                lbl.ForeColor = Color.Red;
                lbl.Text = "Stopped";
                installBtn.Text = "Uninstall";
                if (startBtn is not null)
                {
                    startBtn.Enabled = true;
                    startBtn.Text = "Start";
                }
                break;
            default:
                lbl.ForeColor = Color.Maroon;
                lbl.Text = "Not installed";
                installBtn.Text = "Install";
                if (startBtn is not null)
                {
                    startBtn.Enabled = false;
                }
                break;
        }
    }

    private static ServiceStatus GetServiceStatus(string svcName, bool driver)
    {
        ServiceController[] services = driver
            ? ServiceController.GetDevices()
            : ServiceController.GetServices();

        foreach (ServiceController service in services)
        {
            try
            {
                if (service.ServiceName == svcName)
                {
                    return service.Status is
                        not ServiceControllerStatus.StopPending and
                        not ServiceControllerStatus.Stopped
                        ? ServiceStatus.Running
                        : ServiceStatus.Installed;
                }
            }
            finally
            {
                service.Close();
            }
        }
        return ServiceStatus.NotInstalled;
    }

    private async void btnOBCSInstall_Click(object sender, EventArgs e)
    {
        if (btnOBCSInstall.Text == "Install")
        {
            btnOBCSInstall.Enabled = false;
            btnOBCSInstall.Text = "Installing";

            if (!await Task.Run(() => Utils.InstallNETService("obcsvc")))
            {
                Utils.ShowError(Strings.GetString("svcInstallFail"));
            }
        }
        else if (btnOBCSInstall.Text == "Uninstall")
        {
            if (Utils.ShowInfo(Strings.GetString("dlgSvcUninstall"),
                "Uninstall?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                btnOBCSInstall.Enabled = false;
                btnOBCSInstall.Text = "Uninstalling";

                if (!await Task.Run(() => Utils.UninstallNETService("obcsvc")))
                {
                    Utils.ShowError(Strings.GetString("svcUninstallFail"));
                }
            }
        }
        SetStatusLabel("obcsvc", false, lblOBCSState, btnOBCSInstall, btnOBCSStart);
    }

    private async void btnOBCSStart_Click(object sender, EventArgs e)
    {
        btnOBCSStart.Enabled = false;
        if (btnOBCSStart.Text == "Start")
        {
            btnOBCSStart.Text = "Starting";

            if (!await Task.Run(() => Utils.StartService("obcsvc")))
            {
                Utils.ShowError("Failed to start the OpenBootCamp service!");
            }
        }
        else if (btnOBCSStart.Text == "Stop")
        {
            btnOBCSStart.Text = "Stopping";

            if (!await Task.Run(() => Utils.StopService("obcsvc")))
            {
                Utils.ShowError("Failed to stop the OpenBootCamp service!");
            }
        }
        SetStatusLabel("obcsvc", false, lblOBCSState, btnOBCSInstall, btnOBCSStart);
    }

    private void btnOBCOInstall_Click(object sender, EventArgs e)
    {
        RegistryKey key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        if (btnOBCOInstall.Text == "Install")
        {
            if (File.Exists(@".\ObcOverlay.exe"))
            {
                key.SetValue("ObcOverlay", $"\"{Path.GetFullPath(@".\ObcOverlay.exe")}\" --startup");
                lblOBCOState.ForeColor = Color.DarkGreen;
                lblOBCOState.Text = "Installed";
                btnOBCOInstall.Text = "Uninstall";
            }
            else
            {
                Utils.ShowError("Could not find ObcOverlay.exe!");
            }
        }
        else if (btnOBCOInstall.Text == "Uninstall")
        {
            key.DeleteValue("ObcOverlay", false);
            lblOBCOState.ForeColor = Color.Maroon;
            lblOBCOState.Text = "Not installed";
            btnOBCOInstall.Text = "Install";
        }
    }

    private async void btnKAInstall_Click(object sender, EventArgs e)
    {
        btnKAInstall.Enabled = false;
        if (btnKAInstall.Text == "Install")
        {
            btnKAInstall.Text = "Installing";

            // errors are displayed via the InstallDriver function
            DialogResult result = Utils.ShowInfo(
                "The newer KeyManager.sys driver was found in your BootCamp folder." +
                "This driver replaces KeyAgent.sys on newer Macs, but is untested with OpenBootCamp.\n\n" +
                "Would you like to install KeyManager.sys?\n" +
                "(click `No` to install the old KeyAgent.sys instead, if present)",
                "KeyManager.sys?", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Yes && File.Exists(@"BootCamp\KeyManager.sys"))
            {
                await Task.Run(() => InstallDriver(
                    @"BootCamp\KeyManager.sys", ServiceStartMode.Automatic));
            }
            else if (result == DialogResult.No)
            {
                await Task.Run(() => InstallDriver(
                    @"BootCamp\KeyAgent.sys", ServiceStartMode.Automatic));
            }
        }
        else
        {
            btnKAInstall.Text = "Uninstalling";
            await Task.Run(() => UninstallDriver("KeyAgent.sys"));
        }
        SetStatusLabel("KeyAgent", true, lblKAState, btnKAInstall);
        btnKAInstall.Enabled = true;
    }

    private async void btnMHDInstall_Click(object sender, EventArgs e)
    {
        btnMHDInstall.Enabled = false;
        if (btnMHDInstall.Text == "Install")
        {
            btnMHDInstall.Text = "Installing";
            // MacHALDriver is installed as boot-start by official Boot Camp
            await Task.Run(() => InstallDriver(
                @"BootCamp\MacHALDriver.sys", ServiceStartMode.Boot));
        }
        else
        {
            btnMHDInstall.Text = "Uninstalling";
            await Task.Run(() => UninstallDriver("MacHALDriver.sys"));
        }
        SetStatusLabel("MacHALDriver", true, lblMHDState, btnMHDInstall);
        btnMHDInstall.Enabled = true;
    }

    private static void InstallDriver(string srcPath, ServiceStartMode startMode = ServiceStartMode.Manual)
    {
        srcPath = Path.GetFullPath(srcPath);
        string driverName = Path.GetFileName(srcPath);
        string destPath = $"C:\\Windows\\System32\\drivers\\{driverName}";

        using (Driver driver = new(Path.GetFileNameWithoutExtension(srcPath), destPath))
        {
            try
            {
                File.Copy(srcPath, destPath);
            }
            catch (FileNotFoundException)
            {
                Utils.ShowError($"Could not find driver: {srcPath}");
                return;
            }

            if (!driver.Install(startMode))
            {
                Utils.ShowError($"Failed to install {driverName}!");
                return;
            }
            Utils.ShowInfo(Strings.GetString(
                "drvInstallReboot", driverName), "Success");
        }
    }

    private static void UninstallDriver(string name)
    {
        if (Utils.ShowWarning(
            $"Are you sure you want to uninstall {name}?",
            "Uninstall?") != DialogResult.Yes)
        {
            return;
        }

        string path = $"C:\\Windows\\System32\\drivers\\{name}";

        using (Driver driver = new(Path.GetFileNameWithoutExtension(name)))
        {
            if (!driver.Uninstall())
            {
                Utils.ShowError($"Failed to uninstall {name}!");
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch (UnauthorizedAccessException)
            {
                // driver might still be in use.
                // mark driver file for deletion on reboot
                if (!MoveFileExW(path, null, 4)) // MOVEFILE_DELAY_UNTIL_REBOOT
                {
                    Utils.ShowError(Strings.GetString(
                        "drvDelFail", Marshal.GetLastWin32Error()));
                }
                Utils.ShowInfo(Strings.GetString(
                    "drvUninstallReboot", name), "Success");
            }
        }
    }

    [DllImport("Kernel32", ExactSpelling = true, SetLastError = true,
        CharSet = CharSet.Unicode)]
    private static extern bool MoveFileExW(string src, string dest, int flags);

    private void tsiAbout_Click(object sender, EventArgs e)
    {
        // TODO: port YAMDCC's About dialog here
        Utils.ShowInfo(Strings.GetString("dlgAbout"), "About");
    }

    private void tsiSource_Click(object sender, EventArgs e)
    {
        Process.Start("https://github.com/Sparronator9999/OpenBootCamp");
    }
}
