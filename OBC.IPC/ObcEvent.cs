using MessagePack;

namespace OBC.IPC;

/// <summary>
/// Represents a list of possible OpenBootCamp events.
/// </summary>
public enum ObcEventType
{
    /// <summary>
    /// Fallback value if empty (zero-length) message received by client.
    /// </summary>
    None,
    /// <summary>
    /// Sent when OBC handles a brightness key press.
    /// </summary>
    /// <remarks>
    /// This event's <see cref="ObcEvent.Value"/> field will be set
    /// to the new display brightness, as a percentage (0-100%).
    /// </remarks>
    DispBright,
    /// <summary>
    /// Sent when OBC handles a keyboard backlight key press.
    /// </summary>
    /// <remarks>
    /// This event's <see cref="ObcEvent.Value"/> field will be set to
    /// the new keyboard backlight brightness, as a percentage (0-100%).
    /// </remarks>
    KeyLightBright,
    /// <summary>
    /// Sent when OBC handles a volume key press.
    /// </summary>
    /// <remarks>
    /// This event's <see cref="ObcEvent.Value"/> field will be
    /// set to the new system volume, as a percentage (0-100%).
    /// </remarks>
    Volume,
    /// <summary>
    /// Sent when OBC handles an eject key press.
    /// </summary>
    /// <remarks>
    /// This event does not include any data, and the
    /// <see cref="ObcEvent.Value"/> field will be set to -1.
    /// </remarks>
    Eject,
    /// <summary>
    /// Sent when OBC detects the laptop lid is opened/closed.
    /// </summary>
    /// <remarks>
    /// This event's <see cref="ObcEvent.Value"/> field will be set to
    /// a value indicating whether the lid is open (1) or closed (0).
    /// </remarks>
    LidSwitchChange,
}

/// <summary>
/// Represents an OpenBootCamp event, sent between the Core and Overlay services.
/// </summary>
[MessagePackObject]
public class ObcEvent
{
    /// <summary>
    /// The <see cref="ObcEventType"/> to send to the service.
    /// </summary>
    [Key(0)]
    public ObcEventType Event { get; set; } = ObcEventType.None;

    /// <summary>
    /// The value associated with the <see cref="ObcEventType"/>.
    /// </summary>
    [Key(1)]
    public int Value { get; set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="ServiceResponse"/>
    /// struct with the specified message and return value.
    /// </summary>
    /// <param name="keyEvent"></param>
    /// <param name="value"></param>
    public ObcEvent(ObcEventType keyEvent, int value = -1)
    {
        Event = keyEvent;
        Value = value;
    }
}
