using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace OBC.Service.Logs
{
    /// <summary>
    /// A simple logger class for writing logs to
    /// the console or a configurable file path.
    /// </summary>
    public sealed class Logger : IDisposable
    {
        /// <summary>
        /// The <see cref="StreamWriter"/> to write log files to.
        /// </summary>
        private StreamWriter LogWriter;

        /// <summary>
        /// Used with <c>lock</c> to prevent more than one thread
        /// writing to the file at once.
        /// </summary>
        private readonly object
            fileLock = new(),
            consoleLock = new();

        /// <summary>
        /// The newline characters to split provided log message lines by.
        /// </summary>
        private static readonly char[] NewlineChars = ['\r', '\n'];

        private static string LogString(string text, LogLevel level, bool showDate) =>
            (showDate ? $"[{DateTime.Now:dd/MM/yyyy @ HH:mm:ss.fff}] " : "") + $"[{level}]".PadRight(8).ToUpper(CultureInfo.InvariantCulture) + text;

        /// <summary>
        /// The path to which the log file will be written.
        /// </summary>
        public string LogDir { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Sparronator9999", "OpenBootCamp", "Logs");

        private string LogPath => Path.Combine(LogDir, AppDomain.CurrentDomain.FriendlyName);

        /// <summary>
        /// The maximum number of logs to archive.
        /// </summary>
        public int MaxArchivedLogs { get; set; } = 9;

        /// <summary>
        /// How verbose should console logs be?
        /// </summary>
        public LogLevel ConsoleLogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// How verbose should logs written to disk be?
        /// </summary>
        public LogLevel FileLogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Should the log time be shown in console logs?
        /// </summary>
        public bool ShowTimeInConsole { get; set; }

        /// <summary>
        /// Should the log time be shown in logs written to disk?
        /// </summary>
        public bool ShowTimeInFile { get; set; } = true;

        /// <summary>
        /// Writes a Debug event to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="message">The event to write to the log.</param>
        public void Debug(string message)
        {
            if (FileLogLevel >= LogLevel.Debug)
            {
                WriteFile(message, LogLevel.Debug);
            }

            if (ConsoleLogLevel >= LogLevel.Debug)
            {
                WriteConsole(message, LogLevel.Debug);
            }
        }

        /// <summary>
        /// Writes a Debug event to the <see cref="Logger"/>,
        /// replacing format items with the objects in <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        /// Equivalent to passing a <see cref="string.Format(string, object[])"/>
        /// to the <paramref name="message"/> argument.
        /// </remarks>
        /// <param name="message">The event to write to the log.</param>
        /// <param name="args">The objects to format.</param>
        public void Debug(string message, params object[] args)
        {
            Debug(string.Format(CultureInfo.InvariantCulture, message, args));
        }

        /// <summary>
        /// Writes an Info event to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="message">The event to write to the log.</param>
        public void Info(string message)
        {
            if (FileLogLevel >= LogLevel.Info)
            {
                WriteFile(message, LogLevel.Info);
            }

            if (ConsoleLogLevel >= LogLevel.Info)
            {
                WriteConsole(message, LogLevel.Info);
            }
        }

        /// <summary>
        /// Writes an Info event to the <see cref="Logger"/>,
        /// replacing format items with the objects in <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        /// Equivalent to passing a <see cref="string.Format(string, object[])"/>
        /// to the <paramref name="message"/> argument.
        /// </remarks>
        /// <param name="message">The event to write to the log.</param>
        /// <param name="args">The objects to format.</param>
        public void Info(string message, params object[] args)
        {
            Info(string.Format(CultureInfo.InvariantCulture, message, args));
        }

        /// <summary>
        /// Writes a Warning to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="message">The event to write to the log.</param>
        public void Warn(string message)
        {
            if (FileLogLevel >= LogLevel.Warn)
            {
                WriteFile(message, LogLevel.Warn);
            }

            if (ConsoleLogLevel >= LogLevel.Warn)
            {
                WriteConsole(message, LogLevel.Warn);
            }
        }

        /// <summary>
        /// Writes a Warning to the <see cref="Logger"/>,
        /// replacing format items with the objects in <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        /// Equivalent to passing a <see cref="string.Format(string, object[])"/>
        /// to the <paramref name="message"/> argument.
        /// </remarks>
        /// <param name="message">The event to write to the log.</param>
        /// <param name="args">The objects to format.</param>
        public void Warn(string message, params object[] args)
        {
            Warn(string.Format(CultureInfo.InvariantCulture, message, args));
        }

        /// <summary>
        /// Writes an Error to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="message">The event to write to the log.</param>
        public void Error(string message)
        {
            if (FileLogLevel >= LogLevel.Error)
            {
                WriteFile(message, LogLevel.Error);
            }

            if (ConsoleLogLevel >= LogLevel.Error)
            {
                WriteConsole(message, LogLevel.Error);
            }
        }

        /// <summary>
        /// Writes an Error to the <see cref="Logger"/>,
        /// replacing format items with the objects in <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        /// Equivalent to passing a <see cref="string.Format(string, object[])"/>
        /// to the <paramref name="message"/> argument.
        /// </remarks>
        /// <param name="message">The event to write to the log.</param>
        /// <param name="args">The objects to format.</param>
        public void Error(string message, params object[] args)
        {
            Error(string.Format(CultureInfo.InvariantCulture, message, args));
        }

        /// <summary>
        /// Writes a Fatal Error to the <see cref="Logger"/>. Use when an
        /// application is about to terminate due to a fatal error.
        /// </summary>
        /// <param name="message">The event to write to the log.</param>
        public void Fatal(string message)
        {
            if (FileLogLevel >= LogLevel.Fatal)
            {
                WriteFile(message, LogLevel.Fatal);
            }

            if (ConsoleLogLevel >= LogLevel.Fatal)
            {
                WriteConsole(message, LogLevel.Fatal);
            }
        }

        /// <summary>
        /// Writes a Fatal Error to the <see cref="Logger"/>,
        /// replacing format items with the objects in <paramref name="args"/>.
        /// Use when an application is about to terminate due to a fatal error.
        /// </summary>
        /// <remarks>
        /// Equivalent to passing a <see cref="string.Format(string, object[])"/>
        /// to the <paramref name="message"/> argument.
        /// </remarks>
        /// <param name="message">The event to write to the log.</param>
        /// <param name="args">The objects to format.</param>
        public void Fatal(string message, params object[] args)
        {
            Fatal(string.Format(CultureInfo.InvariantCulture, message, args));
        }

        /// <summary>
        /// Deletes all archived logs (files ending with .[number].log.gz).
        /// </summary>
        public void DeleteArchivedLogs()
        {
            for (int i = 1; i <= MaxArchivedLogs; i++)
            {
                try
                {
                    File.Delete($"{LogPath}.{i}.log.gz");
                }
                catch (FileNotFoundException) { }
            }
        }

        private void WriteFile(string message, LogLevel level)
        {
            lock (fileLock)
            {
                if (LogWriter is null || LogWriter == null || LogWriter.Equals(null))
                {
                    InitLogFile();
                }

                foreach (string str in message.Split(NewlineChars, StringSplitOptions.RemoveEmptyEntries))
                {
                    LogWriter.WriteLine(LogString(str, level, ShowTimeInFile));
                }
            }
        }

        private void WriteConsole(string message, LogLevel level)
        {
            lock (consoleLock)
            {
                ConsoleColor bgColour = Console.BackgroundColor;
                ConsoleColor fgColour = Console.ForegroundColor;

                switch (level)
                {
                    case LogLevel.Fatal:
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                }

                foreach (string str in message.Split(NewlineChars, StringSplitOptions.RemoveEmptyEntries))
                {
                    Console.WriteLine(LogString(str, level, ShowTimeInConsole));
                }

                Console.BackgroundColor = bgColour;
                Console.ForegroundColor = fgColour;
            }
        }

        /// <summary>
        /// Initialises the log file.
        /// Call before any attempts to write a log file.
        /// </summary>
        private void InitLogFile()
        {
            // Rename old log files, and delete the oldest file if
            // there's too many log files
            for (int i = MaxArchivedLogs; i >= 0; i--)
            {
                try
                {
                    if (i == MaxArchivedLogs)
                    {
                        File.Delete($"{LogPath}.{i}.log.gz");
                    }
                    else
                    {
                        File.Move($"{LogPath}.{i}.log.gz", $"{LogPath}.{i + 1}.log.gz");
                    }
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
            }

            try
            {
                FileInfo fi = new($"{LogPath}.log");

                // Set up file streams
                FileStream original = fi.OpenRead();
                FileStream compressed = File.Create($"{LogPath}.{1}.log.gz");
                GZipStream gzStream = new(compressed, CompressionLevel.Optimal);

                // Compress the file
                original.CopyTo(gzStream);

                // Close file streams
                gzStream.Close();
                compressed.Close();
                original.Close();

                // Delete the unarchived copy of the log
                fi.Delete();
            }
            catch (FileNotFoundException)
            {
                // Log files probably don't exist yet,
                // do nothing to avoid crash
            }

            // if anyone knows why the fuck directory.createdirectory
            // is doing nothing when running as yamdccsvc.exe and
            // pretending everything is ok i would LOVE to know
            // (workaround in YAMDCC.GUI/Program.cs)
            Directory.CreateDirectory(LogDir);

            LogWriter = new StreamWriter($"{LogPath}.log")
            {
                AutoFlush = true
            };
        }

        public void Dispose()
        {
            LogWriter.Dispose();
        }
    }
}
