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

using System.Xml.Serialization;

namespace OBC.Common.Configs;

/// <summary>
/// Represents a configuration for OBC's Keyboard Event Listener module.
/// </summary>
public sealed class KbdEventListenerConf
{
    /// <summary>
    /// Gets or sets whether the Keyboard Event Listener should be enabled.
    /// </summary>
    /// <remarks>
    /// The default setting is <see langword="true"/>.
    /// If using the Apple official Boot Camp Manager, this should
    /// be set to <see langword="false"/> to avoid conflicts.
    /// </remarks>
    [XmlElement]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether standard Function keys or their special
    /// printed function should be the "primary" key, i.e. the key that
    /// gets pressed when the Fn key is also not held.
    /// </summary>
    [XmlElement]
    public bool OSXFnBehaviour { get; set; }

    /// <summary>
    /// If <see langword="true"/>, Windows handles brightness key presses.
    /// If <see langword="false"/>, OBC handles brightness key presses.
    /// </summary>
    [XmlElement]
    public bool SystemDispBright { get; set; } = true;

    /// <summary>
    /// The last keyboard backlight brightness set on this computer.
    /// </summary>
    [XmlElement]
    public byte KeyLightBright { get; set; }

    /// <summary>
    /// How much the keyboard backlight brightness should be
    /// changed in response to keyboard backlight key presses.
    /// </summary>
    [XmlElement]
    public byte KeyLightBrightStep { get; set; } = 16;

    /// <summary>
    /// The time, in seconds, before the keyboard backlight
    /// turns off due to keyboard inactivity. Set to 0 to disable.
    /// </summary>
    [XmlElement]
    public int KeyLightTimeout { get; set; } = 15;
}
