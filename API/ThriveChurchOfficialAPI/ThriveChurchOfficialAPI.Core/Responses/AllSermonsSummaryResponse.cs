using System;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// All sermons summarized response
    /// </summary>
    public class AllSermonsSummaryResponse
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public AllSermonsSummaryResponse()
        {
            Summaries = null;
        }

        /// <summary>
        /// Collection of Sermon Series summaries
        /// </summary>
        public IEnumerable<AllSermonSeriesSummary> Summaries { get; set; }
    }
}