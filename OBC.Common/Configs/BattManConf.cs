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
/// Represents a configuration for OBC's Battery Manager module.
/// </summary>
public sealed class BattManConf
{
    /// <summary>
    /// Gets or sets whether the Battery Manager module should be enabled.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="false"/>
    /// (no charge limit will be applied by default).
    /// </remarks>
    [XmlElement]
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the battery charge limit when the Battery Manager
    /// module is enabled, as a percentage between 0-100.
    /// </summary>
    /// <remarks>
    /// <para>The default value is 80%.</para>
    /// <para>Recommended values are between 60% and 100%.</para>
    /// </remarks>
    [XmlElement]
    public byte ChargeLimit { get; set; } = 80;
}
