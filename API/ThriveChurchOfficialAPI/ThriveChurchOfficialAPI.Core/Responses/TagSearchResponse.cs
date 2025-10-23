using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Response object for tag-based search results
    /// </summary>
    public class TagSearchResponse
    {
        public TagSearchResponse()
        {
            Messages = new List<SermonMessageResponse>();
            Series = new List<SermonSeriesResponse>();
        }
        
        /// <summary>
        /// Collection of matching messages (populated when SearchTarget is Messages)
        /// </summary>
        public IEnumerable<SermonMessageResponse> Messages { get; set; }
        
        /// <summary>
        /// Collection of matching series (populated when SearchTarget is Series)
        /// </summary>
        public IEnumerable<SermonSeriesResponse> Series { get; set; }
    }
}

