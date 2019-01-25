using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Sermons Summarized Paged Response
    /// </summary>
    public class SermonsSummaryPagedResponse
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public SermonsSummaryPagedResponse()
        {
            Summaries = null;
            PagingInfo = null;
        }

        /// <summary>
        /// Collection of Sermon Series summaries
        /// </summary>
        public IEnumerable<SermonSeriesSummary> Summaries { get; set; }

        /// <summary>
        /// Info about the paging, including the current request and the total number of pages
        /// </summary>
        public PageInfo PagingInfo { get; set; }
    }
}
