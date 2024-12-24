using OBC.Service.Logs;
using System;
using System.ServiceProcess;
using System.Windows.Forms;

namespace OBC.Service
{
    internal static class Program
    {
        private static readonly Logger Log = new()
        {
            ConsoleLogLevel = LogLevel.None,
            FileLogLevel = LogLevel.Debug,
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
                AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
                Log.Info(Strings.GetString("svcVer", Environment.OSVersion));

                ServiceBase.Run(new OBCService(Log));
            }
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e) =>
            Log.Fatal(Strings.GetString("svcException", e.ExceptionObject));
    }
}
