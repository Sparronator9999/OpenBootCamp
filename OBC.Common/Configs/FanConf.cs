using System.Xml.Serialization;

namespace OBC.Common.Configs;

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

    /// <summary>
    /// How much the sensor's temperature needs to drop by after
    /// peaking before selecting a lower speed.
    /// </summary>
    /// <remarks>
    /// For example, with the default setting of 5°, if a sensor reaches
    /// 60°, the "effective" temperature used to calculate fan RPM wouldn't
    /// begin lowering until the "real" temperature drops below 55°.
    /// </remarks>
    [XmlElement]
    public float Tdown { get; set; } = 5;
}
