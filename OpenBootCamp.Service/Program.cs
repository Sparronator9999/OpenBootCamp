using OpenBootCamp.Service.Logs;
using System;
using System.ServiceProcess;

namespace OpenBootCamp.Service
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
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            Log.Info(
                "~~~~~ OpenBootCamp Service ~~~~~\n" +
                "Copyright © 2024 Sparronator9999\n" +
                $"OS version: {Environment.OSVersion}");

            ServiceBase.Run(new OBCService(Log));
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e) =>
            Log.Fatal(
                $"Unhandled exception occurred: {e.ExceptionObject}\n" +
                "The service will now stop.");
    }
}
