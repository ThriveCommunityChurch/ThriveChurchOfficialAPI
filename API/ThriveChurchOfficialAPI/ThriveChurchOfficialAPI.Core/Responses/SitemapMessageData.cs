using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Minimal message data for sitemap generation
    /// </summary>
    public class SitemapMessageData
    {
        /// <summary>
        /// The unique identifier of the message
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The date this message was given
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Whether this message has a video available
        /// </summary>
        public bool HasVideo { get; set; }
    }
}

