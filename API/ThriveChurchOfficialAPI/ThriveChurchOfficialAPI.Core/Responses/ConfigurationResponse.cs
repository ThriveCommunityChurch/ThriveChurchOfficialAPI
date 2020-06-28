using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class ConfigurationResponse
    {
        /// <summary>
        /// The key for the setting. This is the piece that we look for
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// the value for this setting. This is the value that gets used
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The type of configuration value
        /// </summary>
        public ConfigType Type { get; set; }
    }
}