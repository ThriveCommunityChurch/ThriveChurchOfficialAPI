using System;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Response object for importing sermon series and message data
    /// </summary>
    public class ImportSermonDataResponse
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ImportSermonDataResponse()
        {
            SkippedItems = new List<SkippedImportItem>();
        }

        /// <summary>
        /// The date and time when the import was performed (UTC)
        /// </summary>
        public DateTime ImportDate { get; set; }

        /// <summary>
        /// The total number of sermon series that were processed during the import
        /// </summary>
        public int TotalSeriesProcessed { get; set; }

        /// <summary>
        /// The total number of sermon series that were successfully updated
        /// </summary>
        public int TotalSeriesUpdated { get; set; }

        /// <summary>
        /// The total number of sermon series that were skipped (not found in database)
        /// </summary>
        public int TotalSeriesSkipped { get; set; }

        /// <summary>
        /// The total number of sermon messages that were processed during the import
        /// </summary>
        public int TotalMessagesProcessed { get; set; }

        /// <summary>
        /// The total number of sermon messages that were successfully updated
        /// </summary>
        public int TotalMessagesUpdated { get; set; }

        /// <summary>
        /// The total number of sermon messages that were skipped (not found in database)
        /// </summary>
        public int TotalMessagesSkipped { get; set; }

        /// <summary>
        /// Collection of items (series or messages) that were skipped during import with reasons
        /// </summary>
        public IEnumerable<SkippedImportItem> SkippedItems { get; set; }
    }
}
