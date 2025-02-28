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

namespace OBC.Service.Logs;

/// <summary>
/// The verbosity of logs
/// </summary>
internal enum LogLevel
{
    /// <summary>
    /// Do not log anything.
    /// </summary>
    None,

    /// <summary>
    /// Only log Fatal events.
    /// </summary>
    Fatal,

    /// <summary>
    /// Log Errors and Fatal events.
    /// </summary>
    Error,

    /// <summary>
    /// Log Warnings, Errors, and Fatal events.
    /// </summary>
    Warn,

    /// <summary>
    /// Log all events, except for Debug events.
    /// </summary>
    Info,

    /// <summary>
    /// Log all events.
    /// </summary>
    Debug,
}
