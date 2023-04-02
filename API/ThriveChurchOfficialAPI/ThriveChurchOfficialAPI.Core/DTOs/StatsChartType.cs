using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Select the series of data in which to chart over the requested time range
    /// </summary>
    public enum StatsChartType
    {
        // Extend this object with each new series of data

        /// <summary>
        /// Default cahrt type of unknown is not supported and only used for validation
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Chart audio duration over time
        /// </summary>
        AudioDuration,

        /// <summary>
        /// Aggregate file size increase over time. Value at start date contains aggregate of data until that point in time.
        /// </summary> 
        TotAudioFileSize,

        /// <summary>
        /// Aggregate audio duration increase over time. Value at start date contains aggregate of data until that point in time.
        /// </summary>
        TotAudioDuration
    }
}