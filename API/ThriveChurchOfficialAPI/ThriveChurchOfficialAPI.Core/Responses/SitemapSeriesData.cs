using System;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Minimal series data for sitemap generation
    /// </summary>
    public class SitemapSeriesData
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public SitemapSeriesData()
        {
            Id = null;
            Messages = new List<SitemapMessageData>();
        }

        /// <summary>
        /// The unique identifier of the sermon series
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The last time this series was updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Collection of messages in this series
        /// </summary>
        public List<SitemapMessageData> Messages { get; set; }
    }
}

