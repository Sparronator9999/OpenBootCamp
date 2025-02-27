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
