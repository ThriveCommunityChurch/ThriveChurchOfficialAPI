using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class ConfigurationCollectionResponse
    {
        /// <summary>
        /// A collection of configuration values
        /// </summary>
        public IEnumerable<ConfigurationResponse> Configs { get; set; }
    }
}