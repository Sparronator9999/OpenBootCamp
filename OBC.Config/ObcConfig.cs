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

namespace OBC.Config
{
    public sealed class ObcConfig
    {
        [XmlAttribute]
        public int Ver { get; set; }

        [XmlIgnore]
        private const int ExpectedVer = 1;

        [XmlElement]
        public bool UseAcpiBrightness { get; set; }

        [XmlElement]
        public byte KeyboardBrightness { get; set; }

        [XmlElement]
        public byte KeyboardBrightnessStep { get; set; }

        public ObcConfig() { }

        public ObcConfig(bool acpiBright, byte keyBright, byte keyBrightStep)
        {
            Ver = ExpectedVer;
            UseAcpiBrightness = acpiBright;
            KeyboardBrightness = keyBright;
            KeyboardBrightnessStep = keyBrightStep;
        }

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
            if (Ver != ExpectedVer)
            {
                return false;
            }

            // All other values are considered to be valid; return true
            return true;
        }
    }
}
