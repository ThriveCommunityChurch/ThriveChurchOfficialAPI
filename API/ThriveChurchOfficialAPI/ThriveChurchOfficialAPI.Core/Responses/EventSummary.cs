using System;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Lightweight event summary for list views
    /// </summary>
    public class EventSummary
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public EventSummary()
        {
            Id = null;
            Title = null;
            Summary = null;
            ThumbnailUrl = null;
            IconName = null;
            EndTime = null;
            IsRecurring = false;
            RecurrencePattern = null;
            IsOnline = false;
            LocationName = null;
            IsFeatured = false;
            Tags = new List<string>();
        }

        /// <summary>
        /// The Id of the event in mongo
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The title of the event
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A brief summary of the event
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The direct URL to the thumbnail image for this event
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// The FontAwesome icon name for this event
        /// </summary>
        public string IconName { get; set; }

        /// <summary>
        /// The start date and time of the event (UTC)
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The end date and time of the event (UTC, optional)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Indicates if this event recurs
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// The recurrence pattern for this event (if recurring)
        /// </summary>
        public RecurrencePattern? RecurrencePattern { get; set; }

        /// <summary>
        /// Indicates if this is an online event
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// The name of the location (flattened from Location.Name)
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Indicates if the event is featured
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Tags/categories for the event
        /// </summary>
        public List<string> Tags { get; set; }
    }
}

