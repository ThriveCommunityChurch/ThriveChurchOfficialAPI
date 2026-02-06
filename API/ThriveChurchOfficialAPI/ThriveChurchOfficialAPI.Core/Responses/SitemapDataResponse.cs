using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Response containing minimal sermon data optimized for sitemap generation
    /// </summary>
    public class SitemapDataResponse
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public SitemapDataResponse()
        {
            Series = new List<SitemapSeriesData>();
        }

        /// <summary>
        /// Collection of sermon series with their messages
        /// </summary>
        public List<SitemapSeriesData> Series { get; set; }
    }
}

