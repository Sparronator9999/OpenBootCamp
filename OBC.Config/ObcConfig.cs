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

namespace OBC.Config;

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
    /// Set to <see langword="true"/> to log the computer's supported SMC keys
    /// on service startup. The default is <see langword="false"/>.
    /// </summary>
    [XmlElement]
    public bool LogSMCKeys { get; set; }

    /// <summary>
    /// If <see cref="LogSMCKeys"/> is set to <see langword="true"/>,
    /// also logs the initial value of all readable SMC keys if set to
    /// <see langword="true"/>. The default is <see langword="false"/>
    /// </summary>
    /// <remarks>
    /// This option may increase the service startup time when enabled.
    /// </remarks>
    [XmlElement]
    public bool LogSMCKeyData { get; set; }

    /// <summary>
    /// Configuration settings for the OBC Service's Keyboard Event Listener module.
    /// </summary>
    /// <remarks>
    /// Must not be <see langword="null"/>.
    /// </remarks>
    [XmlElement]
    public KbdEventListenerConf KbdEventListener { get; set; } =
        new KbdEventListenerConf();

    /// <summary>
    /// Configuration settings for the OBC Service's Fan Control module.
    /// </summary>
    /// <remarks>
    /// Must not be <see langword="null"/>.
    /// </remarks>
    [XmlElement]
    public FanControlConf FanControl { get; set; }
        = new FanControlConf();

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
    /// <c>true</c> if the config is valid, otherwise <c>false</c>.
    /// </returns>
    private bool IsValid()
    {
        if (Ver != ExpectedVer || KbdEventListener is null || FanControl is null)
        {
            return false;
        }

        // All other values are considered to be valid; return true
        return true;
    }
}
