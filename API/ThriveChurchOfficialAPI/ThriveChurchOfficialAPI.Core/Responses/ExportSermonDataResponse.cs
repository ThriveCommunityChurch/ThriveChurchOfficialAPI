using System;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Response object for exporting all sermon series and message data
    /// </summary>
    public class ExportSermonDataResponse
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExportSermonDataResponse()
        {
            Series = new List<SermonSeriesResponse>();
        }

        /// <summary>
        /// The date and time when the export was performed (UTC)
        /// </summary>
        public DateTime ExportDate { get; set; }

        /// <summary>
        /// The total number of sermon series included in the export
        /// </summary>
        public int TotalSeries { get; set; }

        /// <summary>
        /// The total number of sermon messages included in the export across all series
        /// </summary>
        public int TotalMessages { get; set; }

        /// <summary>
        /// Collection of all sermon series with their nested messages. Each series includes all properties.
        /// </summary>
        public IEnumerable<SermonSeriesResponse> Series { get; set; }
    }
}

