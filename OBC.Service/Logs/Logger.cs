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

using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace OBC.Service.Logs;

/// <summary>
/// A simple logger class for writing logs to
/// the console or a configurable file path.
/// </summary>
internal sealed class Logger : IDisposable
{
    /// <summary>
    /// The <see cref="StreamWriter"/> to write log files to.
    /// </summary>
    private StreamWriter LogWriter;

    /// <summary>
    /// The newline characters to split provided log message lines by.
    /// </summary>
    private static readonly char[] NewLine = ['\r', '\n'];

    /// <summary>
    /// The directory in which log files are saved.
    /// </summary>
    public string LogDir;

    /// <summary>
    /// The base name of the log file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Log files will have the <c>.log</c> extension appended.
    /// </para>
    /// <para>
    /// Archives will have a number appended before the <c>.log</c>
    /// extension, with higher numbers indicating older logs.
    /// </para>
    /// </remarks>
    public string LogName;

    private string LogPath => Path.Combine(LogDir, LogName);

    /// <summary>
    /// The maximum number of logs to archive.
    /// </summary>
    public int MaxArchived = 9;

    public Logger()
    {
        string exePath = Assembly.GetEntryAssembly().Location;

        LogDir = Path.GetDirectoryName(exePath);
        LogName = Path.GetFileName(exePath);
    }

    /// <summary>
    /// Writes a Debug event to the <see cref="Logger"/>.
    /// </summary>
    /// <param name="msg">
    /// The message to write to the log.
    /// </param>
    public void Debug(string msg)
    {
        LogFile(msg, LogLevel.DEBUG);
    }

    /// <summary>
    /// Writes an Info event to the <see cref="Logger"/>.
    /// </summary>
    /// <param name="msg">
    /// The message to write to the log.
    /// </param>
    public void Info(string msg)
    {
        LogFile(msg, LogLevel.INFO);
    }

    /// <summary>
    /// Writes a Warning to the <see cref="Logger"/>.
    /// </summary>
    /// <param name="msg">
    /// The message to write to the log.
    /// </param>
    public void Warn(string msg)
    {
        LogFile(msg, LogLevel.WARN);
    }

    /// <summary>
    /// Writes an Error to the <see cref="Logger"/>.
    /// </summary>
    /// <param name="msg">
    /// The message to write to the log.
    /// </param>
    public void Error(string msg)
    {
        LogFile(msg, LogLevel.ERROR);
    }

    /// <summary>
    /// Writes a Fatal error to the <see cref="Logger"/>. Use when an
    /// application is about to terminate due to a fatal error.
    /// </summary>
    /// <param name="msg">
    /// The message to write to the log.
    /// </param>
    public void Fatal(string msg)
    {
        LogFile(msg, LogLevel.FATAL);
    }

    /// <summary>
    /// Deletes all archived logs (files ending with .[number].log.gz).
    /// </summary>
    public void DeleteArchived()
    {
        for (int i = 1; i <= MaxArchived; i++)
        {
            try
            {
                File.Delete($"{LogPath}.{i}.log.gz");
            }
            catch (FileNotFoundException) { }
        }
    }

    private void LogFile(string msg, LogLevel level)
    {
        if (msg is null)
        {
            return;
        }

        if (LogWriter is null)
        {
            InitLogFile();
        }

        lock (LogWriter)
        {
            foreach (string str in msg.Split(NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                LogWriter.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.fff}] {$"[{level}]",-8} {str}");
            }
        }
    }

    /// <summary>
    /// Initialises the log file.
    /// Call before any attempts to write a log file.
    /// </summary>
    private void InitLogFile()
    {
        // create log directory if it doesn't exist already
        Directory.CreateDirectory(LogDir);

        // Rename old log files, and delete the oldest file if
        // there's too many log files
        for (int i = MaxArchived; i >= 0; i--)
        {
            try
            {
                if (i == MaxArchived)
                {
                    File.Delete($"{LogPath}.{i}.log.gz");
                }
                else
                {
                    File.Move($"{LogPath}.{i}.log.gz", $"{LogPath}.{i + 1}.log.gz");
                }
            }
            catch (FileNotFoundException) { }
        }

        try
        {
            // Set up file streams
            using (FileStream original = File.OpenRead($"{LogPath}.log"))
            using (FileStream compressed = File.Create($"{LogPath}.{1}.log.gz"))
            using (GZipStream gzStream = new(compressed, CompressionLevel.Optimal))
            {
                // Compress the file
                original.CopyTo(gzStream);
            }

            // Delete the unarchived copy of the log
            File.Delete($"{LogPath}.log");
        }
        catch (FileNotFoundException)
        {
            // Log files probably don't exist yet,
            // do nothing to avoid crash
        }

        LogWriter = new StreamWriter($"{LogPath}.log")
        {
            AutoFlush = true
        };
    }

    /// <summary>
    /// Releases all resources used by this <see cref="Logger"/> instance.
    /// </summary>
    public void Dispose()
    {
        LogWriter.Dispose();
    }
}
