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
using System.Xml;
using System.Xml.Serialization;

namespace OBC.Common.Configs;

public sealed class ObcConfig
{
    /// <summary>
    /// The version of this OBC config.
    /// </summary>
    /// <remarks>
    /// If this value is not equal to <see cref="ExpectedVer"/>, the config will
    /// not be loaded and an <see cref="InvalidConfigException"/> will be thrown.
    /// </remarks>
    [XmlAttribute]
    public int Ver { get; set; } = 1;

    [XmlIgnore]
    private const int ExpectedVer = 1;

    /// <summary>
    /// Dumps the value of all readable SMC keys to a text file
    /// at the specified times (see <see cref="SMCKeyDumpType"/>).
    /// </summary>
    /// <remarks>
    /// This option may increase the service startup
    /// and/or shutdown time when enabled.
    /// </remarks>
    [XmlElement]
    public SMCKeyDumpType DumpSMCKeys { get; set; }

    /// <summary>
    /// The time, in seconds, after which
    /// <see cref="SMCKeyDumpType.OnSvcStartDelayed"/> and
    /// <see cref="SMCKeyDumpType.OnWakeDelayed"/> should
    /// dump SMC keys.
    /// </summary>
    /// <remarks>
    /// This value has no effect if <see cref="DumpSMCKeys"/>
    /// is set to any value that doesn't include
    /// <see cref="SMCKeyDumpType.OnSvcStartDelayed"/> or
    /// <see cref="SMCKeyDumpType.OnWakeDelayed"/>.
    /// </remarks>
    [XmlElement]
    public int KeyDumpDelayTime { get; set; } = 60;

    /// <summary>
    /// Configuration settings for the OBC Service's Keyboard Event Listener module.
    /// </summary>
    /// <remarks>
    /// Must not be <see langword="null"/>.
    /// </remarks>
    [XmlElement]
    public KbdEventListenerConf KbdEventListener { get; set; } = new KbdEventListenerConf();

    /// <summary>
    /// Configuration settings for the OBC Service's Fan Control module.
    /// </summary>
    /// <remarks>
    /// Must not be <see langword="null"/>.
    /// </remarks>
    [XmlElement]
    public FanControlConf FanControl { get; set; } = new FanControlConf();

    /// <summary>
    /// Configuration settings for the OBC Service's Battery Manager module.
    /// </summary>
    /// <remarks>
    /// Must not be <see langword="null"/>.
    /// </remarks>
    [XmlElement]
    public BattManConf BatteryManager { get; set; } = new BattManConf();

    /// <summary>
    /// Parses an OpenBootCamp config XML and returns an
    /// <see cref="ObcConfig"/> object.
    /// </summary>
    /// <param name="xmlFile">The path to an XML config file.</param>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="InvalidConfigException"/>
    public static ObcConfig Load(string xmlFile)
    {
        XmlSerializer serialiser = new(typeof(ObcConfig));
        using (XmlReader reader = XmlReader.Create(xmlFile))
        {
            ObcConfig cfg = (ObcConfig)serialiser.Deserialize(reader);
            return cfg.IsValid() ? cfg : throw new InvalidConfigException();
        }
    }

    /// <summary>
    /// Saves an OpenBootCamp config to the specified location.
    /// </summary>
    /// <param name="xmlFile">The XML file to write to.</param>
    /// <exception cref="InvalidOperationException"/>
    public void Save(string xmlFile)
    {
        XmlSerializer serializer = new(typeof(ObcConfig));
        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = "\t",
        };

        using (XmlWriter writer = XmlWriter.Create(xmlFile, settings))
        {
            serializer.Serialize(writer, this);
        }
    }

    /// <summary>
    /// Performs some validation on the loaded config to make
    /// sure it is in the expected format.
    /// </summary>
    /// <remarks>
    /// This does NOT guarantee the loaded config is valid!
    /// </remarks>
    /// <returns>
    /// <see langword="true"></see> if the config is valid,
    /// otherwise <see langword="false"></see>.
    /// </returns>
    private bool IsValid()
    {
        // all OBC module configs must not be null,
        // and the config version must match the expected version by the program
        if (Ver != ExpectedVer || KbdEventListener is null ||
            FanControl is null || BatteryManager is null)
        {
            return false;
        }

        // charge limit must be between 0 and 100%
        // (although I don't see any point in making it lower than ~40%)
        if (BatteryManager.ChargeLimit > 100)
        {
            return false;
        }

        // All other values are considered to be valid; return true
        return true;
    }
}

[Flags]
public enum SMCKeyDumpType
{
    /// <summary>
    /// Don't dump SMC keys or data.
    /// </summary>
    /// <remarks>
    /// If unsure, use this setting.
    /// </remarks>
    Never = 0,
    /// <summary>
    /// Dump SMC keys during service startup.
    /// </summary>
    /// <remarks>
    /// May increase service startup time.
    /// </remarks>
    OnSvcStart = 1,
    /// <summary>
    /// Dump SMC keys <see cref="ObcConfig.KeyDumpDelayTime"/>
    /// seconds after service start.
    /// </summary>
    OnSvcStartDelayed = 2,
    /// <summary>
    /// Dump SMC keys during service shutdown.
    /// </summary>
    /// <remarks>
    /// May increase service shutdown time.
    /// </remarks>
    OnSvcStop = 4,
    /// <summary>
    /// Dump SMC keys just before system sleep.
    /// </summary>
    OnSleep = 8,
    /// <summary>
    /// Dump SMC keys just after system wake.
    /// </summary>
    OnWake = 0x10,
    /// <summary>
    /// Dump SMC keys <see cref="ObcConfig.KeyDumpDelayTime"/>
    /// seconds after system wake.
    /// </summary>
    OnWakeDelayed = 0x20,
}
