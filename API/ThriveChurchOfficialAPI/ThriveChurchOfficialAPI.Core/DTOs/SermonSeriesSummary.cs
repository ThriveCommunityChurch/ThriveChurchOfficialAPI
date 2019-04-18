using System;
namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Sermon series summary
    /// </summary>
    public class SermonSeriesSummary
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public SermonSeriesSummary()
        {
            Id = null;
            Title = null;
            ArtUrl = null;
        }

        /// <summary>
        /// The Id of the object in mongo
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title of the sermon series
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Start date of this sermon series, Primary sorting key DESC order
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Direct link for the series' graphic
        /// </summary>
        public string ArtUrl { get; set; }
    }
}
