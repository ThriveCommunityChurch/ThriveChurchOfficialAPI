using System;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonMessageSummary
    {
        /// <summary>
        /// A numeric value representing the number of seconds of the message audio file
        /// </summary>
        public double AudioDuration { get; set; }

        /// <summary>
        /// The title of the message
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The individual giving this message
        /// </summary>
        public string Speaker { get; set; }

        /// <summary>
        /// The name of the series that this message was in
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// The image for the series
        /// </summary>
        public string SeriesArt { get; set; }

        /// <summary>
        /// The date that this message was given
        /// </summary>
        public DateTime Date { get; set; }
    }
}