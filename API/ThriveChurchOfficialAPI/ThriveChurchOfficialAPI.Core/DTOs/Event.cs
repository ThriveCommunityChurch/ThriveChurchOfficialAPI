using System;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Represents a church event
    /// </summary>
    public class Event : ObjectBase
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public Event()
        {
            Title = null;
            Summary = null;
            Description = null;
            ImageUrl = null;
            ThumbnailUrl = null;
            IconName = null;
            EndTime = null;
            IsAllDay = false;
            IsRecurring = false;
            Recurrence = null;
            IsOnline = false;
            OnlineLink = null;
            OnlinePlatform = null;
            Location = null;
            ContactEmail = null;
            ContactPhone = null;
            RegistrationUrl = null;
            Tags = new List<string>();
            IsActive = true;
            IsFeatured = false;
            LastUpdated = DateTime.UtcNow;
            CreatedBy = null;
        }

        /// <summary>
        /// The title of the event
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A brief summary of the event for preview displays
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The full description of the event (supports markdown)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The direct URL to the full resolution image for this event (S3 URL)
        /// </summary>
        public string ImageUrl { get; set; }

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
        /// Indicates if this is an all-day event
        /// </summary>
        public bool IsAllDay { get; set; }

        /// <summary>
        /// Indicates if this event recurs
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// The recurrence settings for this event
        /// </summary>
        public EventRecurrence Recurrence { get; set; }

        /// <summary>
        /// Indicates if this is an online event
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// The link to the online event (e.g., Zoom link)
        /// </summary>
        public string OnlineLink { get; set; }

        /// <summary>
        /// The platform for the online event (e.g., 'Zoom', 'YouTube Live')
        /// </summary>
        public string OnlinePlatform { get; set; }

        /// <summary>
        /// The physical location of the event
        /// </summary>
        public EventLocation Location { get; set; }

        /// <summary>
        /// The contact email for event inquiries
        /// </summary>
        public string ContactEmail { get; set; }

        /// <summary>
        /// The contact phone number for event inquiries
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// The URL for event registration
        /// </summary>
        public string RegistrationUrl { get; set; }

        /// <summary>
        /// Tags/categories for the event
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Indicates if the event is active (soft delete support)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates if the event is featured
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Timestamp for when this event was last updated (UTC)
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// The user who created this event
        /// </summary>
        public string CreatedBy { get; set; }
    }
}

