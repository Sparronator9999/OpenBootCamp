using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace OpenBootCamp.Service
{
    [RunInstaller(true)]
    public sealed class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            ServiceInstaller installer = new()
            {
                DisplayName = "OpenBootCamp Service",
                ServiceName = "obcsvc",
                StartType = ServiceStartMode.Automatic,
            };

            ServiceProcessInstaller processInstaller = new()
            {
                Account = ServiceAccount.LocalService,
            };

            Installers.AddRange(
            [
                installer,
                processInstaller,
            ]);
        }
    }
}
