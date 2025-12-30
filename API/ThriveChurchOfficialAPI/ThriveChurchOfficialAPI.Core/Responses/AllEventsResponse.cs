using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Response containing a collection of events
    /// </summary>
    public class AllEventsResponse
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public AllEventsResponse()
        {
            Events = null;
            TotalCount = 0;
        }

        /// <summary>
        /// A collection of event summaries
        /// </summary>
        public IEnumerable<EventSummary> Events { get; set; }

        /// <summary>
        /// The total count of events
        /// </summary>
        public int TotalCount { get; set; }
    }
}

