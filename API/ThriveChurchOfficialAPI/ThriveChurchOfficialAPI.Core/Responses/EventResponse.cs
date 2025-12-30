namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Response wrapper for a single event
    /// </summary>
    public class EventResponse
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public EventResponse()
        {
            Event = null;
        }

        /// <summary>
        /// The full event entity
        /// </summary>
        public Event Event { get; set; }
    }
}

