using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Core
{
    public class SpeakerStats
    {
        /// <summary>
        /// The name of the speaker
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The number of messages that this speaker has given
        /// </summary>
        public int MessageCount { get; set; }
        
        /// <summary>
        /// The average duration of all messages that this speaker has given
        /// </summary>
        public double AvgLength { get; set; }
    }
}