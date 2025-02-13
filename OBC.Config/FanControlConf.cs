using System.Collections.Generic;
using System.Xml.Serialization;

namespace OBC.Config;

/// <summary>
/// Represents a configuration for OBC's Fan Control module.
/// </summary>
/// <remarks>
/// None of these settings currently have any effect,
/// since the Fan Control module is not implemented yet.
/// </remarks>
public sealed class FanControlConf
{
    /// <summary>
    /// Gets or sets whether the Fan Control module should be enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default setting is <see langword="true"/>,
    /// however no fans will actually be controlled by default.
    /// </para>
    /// <para>
    /// If using another fan control application, this should
    /// be set to <see langword="false"/> to avoid conflicts.
    /// </para>
    /// </remarks>
    [XmlElement]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the rate (in milliseconds) at which the OBC service
    /// polls the SMC for temperature changes (and updates fan speeds accordingly).
    /// </summary>
    /// <remarks>
    /// The default value is 2500 ms (2.5 seconds).
    /// </remarks>
    [XmlElement]
    public int PollRate { get; set; } = 2500;

    /// <summary>
    /// An array of fan configurations for all fans present in the computer.
    /// </summary>
    /// <remarks>
    /// If this is <see langword="null"/>, empty, or has too few configs for
    /// the fans in the computer, OBC will add a default <see cref="FanConf"/>
    /// for every fan with a missing config.
    /// </remarks>
    [XmlArray]
    public List<FanConf> FanConfs { get; set; }
}
