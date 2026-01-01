using System;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Request object for updating an existing event (partial update support)
    /// </summary>
    public class UpdateEventRequest
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public UpdateEventRequest()
        {
            Title = null;
            Summary = null;
            Description = null;
            ImageUrl = null;
            ThumbnailUrl = null;
            IconName = null;
            StartTime = null;
            EndTime = null;
            IsAllDay = null;
            IsRecurring = null;
            Recurrence = null;
            IsOnline = null;
            OnlineLink = null;
            OnlinePlatform = null;
            Location = null;
            ContactEmail = null;
            ContactPhone = null;
            RegistrationUrl = null;
            Tags = null;
            IsActive = null;
            IsFeatured = null;
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
        /// The direct URL to the full resolution image for this event
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
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// The end date and time of the event (UTC)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Indicates if this is an all-day event
        /// </summary>
        public bool? IsAllDay { get; set; }

        /// <summary>
        /// Indicates if this event recurs
        /// </summary>
        public bool? IsRecurring { get; set; }

        /// <summary>
        /// The recurrence settings for this event
        /// </summary>
        public EventRecurrence Recurrence { get; set; }

        /// <summary>
        /// Indicates if this is an online event
        /// </summary>
        public bool? IsOnline { get; set; }

        /// <summary>
        /// The link to the online event
        /// </summary>
        public string OnlineLink { get; set; }

        /// <summary>
        /// The platform for the online event
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
        /// Indicates if the event is active
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Indicates if the event is featured
        /// </summary>
        public bool? IsFeatured { get; set; }

        /// <summary>
        /// Validate the request object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ValidationResponse ValidateRequest(UpdateEventRequest request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            // Validate EndTime is after StartTime if both are being updated
            if (request.StartTime.HasValue && request.EndTime.HasValue && request.EndTime <= request.StartTime)
            {
                return new ValidationResponse(true, SystemMessages.EndDateMustBeAfterStartDate);
            }

            // Validate OnlineLink is provided if IsOnline is being set to true
            if (request.IsOnline.HasValue && request.IsOnline.Value && string.IsNullOrEmpty(request.OnlineLink))
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(OnlineLink)));
            }

            // Validate recurrence settings if IsRecurring is being set to true
            if (request.IsRecurring.HasValue && request.IsRecurring.Value)
            {
                if (request.Recurrence == null)
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(Recurrence)));
                }

                // Validate DayOfWeek is required for weekly recurrence
                if (request.Recurrence.Pattern == RecurrencePattern.Weekly && !request.Recurrence.DayOfWeek.HasValue)
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Recurrence.DayOfWeek"));
                }

                // Validate DayOfMonth is required for monthly recurrence
                if (request.Recurrence.Pattern == RecurrencePattern.Monthly && !request.Recurrence.DayOfMonth.HasValue)
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Recurrence.DayOfMonth"));
                }
            }

            return new ValidationResponse("Success!");
        }
    }
}

