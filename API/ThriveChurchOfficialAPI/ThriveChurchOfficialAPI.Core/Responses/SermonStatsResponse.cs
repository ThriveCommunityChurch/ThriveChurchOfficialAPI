using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonStatsResponse
    {
        /// <summary>
        /// The total number of sermon series that we have data for
        /// </summary>
        public int TotalSeriesNum { get; set; }

        /// <summary>
        /// The total number of sermon messages that we have data for
        /// </summary>
        public int TotalMessageNum { get; set; }

        /// <summary>
        /// The average number of messages in each series 
        /// </summary>
        public double AvgMessagesPerSeries { get; set; }

        /// <summary>
        /// The total amount of time for each message recording (in seconds)
        /// </summary>
        public double TotalAudioLength { get; set; }

        /// <summary>
        /// The average duration of every message recording (in seconds)
        /// </summary>
        public double AvgAudioLength { get; set; }

        /// <summary>
        /// The total file size of all message recordings (in MB)
        /// </summary>
        public double TotalFileSize { get; set; }

        /// <summary>
        /// The average file size of every message recording (in MB)
        /// </summary>
        public double AvgFileSize { get; set; }

        /// <summary>
        /// A collection of each speaker and stats associated with each speaker
        /// </summary>
        public IEnumerable<SpeakerStats> SpeakerStats { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //public LongestSeriesData LongestSeriesInfo { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //public LongestMessagedata LongestMessageInfo { get; set; }
    }
}