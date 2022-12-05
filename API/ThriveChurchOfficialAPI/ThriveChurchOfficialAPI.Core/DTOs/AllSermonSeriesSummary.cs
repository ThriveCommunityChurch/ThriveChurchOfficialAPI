using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Sermon series summary
    /// </summary>
    public class AllSermonSeriesSummary
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public AllSermonSeriesSummary()
        {
            Id = null;
            Title = null;
        }

        /// <summary>
        /// The ID of the sermon series
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The title of the sermon series
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The start date of this sermon series
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Direct link for the series' graphic
        /// </summary>
        public string ArtUrl { get; set; }

        /// <summary>
        /// The end date of this sermon series
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The number of messages currently in this series
        /// </summary>
        public int? MessageCount { get; set; }

        /// <summary>
        /// The last time that the series was updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}