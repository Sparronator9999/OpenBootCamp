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
using System.Configuration.Install;
using System.ServiceProcess;

namespace OBC.Service
{
    [RunInstaller(true)]
    public sealed class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            ServiceInstaller installer = new()
            {
                Description = Strings.GetString("svcDesc"),
                DisplayName = "OpenBootCamp service",
                ServiceName = "obcsvc",
                StartType = ServiceStartMode.Automatic,
            };

            ServiceProcessInstaller processInstaller = new()
            {
                Account = ServiceAccount.LocalSystem,
            };

            Installers.AddRange(
            [
                installer,
                processInstaller,
            ]);
        }
    }
}
