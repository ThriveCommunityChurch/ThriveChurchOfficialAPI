using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class ConfigurationMap
    {
        /// <summary>
        /// The configuration key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The type of configuration value
        /// </summary>
        public ConfigType Type { get; set; }

        /// <summary>
        /// The configuration value
        /// </summary>
        public string Value { get; set; }
    }
}