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

namespace OBC.Service;

// names taken from SMCKit and adapted for MacHALDriver.
// link: https://github.com/beltex/SMCKit/blob/master/SMCKit/SMC.swift#L153
// this is an exhaustive list of all IOCTLs MacHALDriver v6.200.6.0 will respond to
internal enum MacHALDriverIoCtl : uint
{
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Unused by Boot Camp Manager but present in MacHALDriver.sys.
    /// </remarks>
    Unknown1 = 0x9C402410u,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Unused by Boot Camp Manager but present in MacHALDriver.sys.
    /// </remarks>
    Unknown2 = 0x9C402414u,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Called by Boot Camp Manager but its purpose is unknown.
    /// </remarks>
    Unknown3 = 0x9C402440u,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Called by Boot Camp Manager but its purpose is unknown.
    /// </remarks>
    Unknown4 = 0x9C402444u,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Called by Boot Camp Manager but its purpose is unknown.
    /// </remarks>
    Unknown5 = 0x9C402448u,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Called by Boot Camp Manager but its purpose is unknown.
    /// </remarks>
    Unknown6 = 0x9C40244Cu,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Called by Boot Camp Manager but its purpose is unknown.
    /// </remarks>
    Unknown7 = 0x9C402450u,
    /// <summary>
    /// Reads a value associated with an SMC key.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Expects an input buffer of 5 bytes (null-terminated string of 4 characters
    /// denoting the SMC key to read), and a variable output buffer size (should be
    /// set to the value retrieved when calling <see cref="GetKeyInfo"/>).
    /// </para>
    /// <para>
    /// Used by Boot Camp Manager to retrieve the following key values:
    /// ALI0, ALV0, ALV1 and MSLD, but can be used to read any readable SMC key
    /// (obtained using <see cref="GetKeyByIndex"/> and <see cref="GetKeyInfo"/>).
    /// </para>
    /// </remarks>
    ReadKey = 0x9C402454u,
    /// <summary>
    /// Writes a value to an SMC key.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Expects an input buffer of 5 + (data-length), where the first 5 bytes
    /// are the null-terminated SMC key to write to, and the remaining bytes
    /// are the data to write to the key.
    /// </para>
    /// <para>
    /// Used only once in Boot Camp Manager, to set LKSB
    /// (i.e. the keyboard backlight brightness).
    /// </para>
    /// </remarks>
    WriteKey = 0x9C402458u,
    /// <summary>
    /// Gets an SMC key by its index (call <see cref="ReadKey"/> with
    /// the "#KEY" key to get how many SMC keys are present).
    /// </summary>
    /// <remarks>
    /// Unused by Boot Camp Manager.
    /// </remarks>
    GetKeyByIndex = 0x9C40245Cu,
    /// <summary>
    /// Gets information (data length, type, and attributes) for the provided SMC key.
    /// </summary>
    /// <remarks>
    /// Used by Boot Camp Manager, presumably to check for keyboard
    /// backlight (LKSB) and ambient light sensor (ALV0) support.
    /// </remarks>
    GetKeyInfo = 0x9C402460u,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Unused by Boot Camp Manager but present in MacHALDriver.sys.
    /// </remarks>
    Unknown12 = 0x9C402464u,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// Unused by Boot Camp Manager but present in MacHALDriver.sys.
    /// </remarks>
    Unknown13 = 0x9C402480u,
    /// <summary>
    /// Unknown function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Expects no input buffer, and an output buffer of 0x3C bytes.
    /// </para>
    /// <para>
    /// Called by Boot Camp Manager to retrieve some data
    /// (looks to be some sort of display information).
    /// </para>
    /// </remarks>
    Unknown14 = 0x9C402484u,
    /// <summary>
    /// Increases the display brightness by one step.
    /// </summary>
    /// <remarks>
    /// This IOCTL expects no input or output buffer.
    /// </remarks>
    DispBrightUp = 0x9C402488u,
    /// <summary>
    /// Decreases the display brightness by one step.
    /// </summary>
    /// <remarks>
    /// This IOCTL expects no input or output buffer.
    /// </remarks>
    DispBrightDown = 0x9C40248Cu
}
