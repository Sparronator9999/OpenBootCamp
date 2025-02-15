using System.Xml.Serialization;

namespace OBC.Config;

public sealed class FanConf
{
    /// <summary>
    /// Should OBC control this fan?
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see langword="true"/>, this fan's speed will be set by
    /// OBC will using the other settings in this <see cref="FanConf"/>.
    /// </para>
    /// <para>
    /// If <see langword="false"/>, this fan's speed will be
    /// controlled by the SMC (system default).
    /// </para>
    /// </remarks>
    [XmlElement]
    public bool Enabled { get; set; }

    /// <summary>
    /// The SMC key for the sensor that controls this fan's speed.
    /// </summary>
    [XmlElement]
    public string SensorKey { get; set; } = string.Empty;

    /// <summary>
    /// The sensor temperature that sets this fan's minimum speed.
    /// </summary>
    [XmlElement]
    public float Tmin { get; set; }

    /// <summary>
    /// The sensor temperature that sets this fan's maximum speed.
    /// </summary>
    [XmlElement]
    public float Tmax { get; set; }
}
