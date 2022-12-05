using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Core
{
    public class LongestSermonSeriesSummary: SermonSeriesSummary
    {
        /// <summary>
        /// The number of messages in a series
        /// </summary>
        public int SeriesLength { get; set; }
    }
}