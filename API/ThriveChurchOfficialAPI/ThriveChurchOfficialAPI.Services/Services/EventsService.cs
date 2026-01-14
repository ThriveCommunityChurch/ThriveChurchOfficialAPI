using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service implementation for Event operations
    /// </summary>
    public class EventsService : BaseService, IEventsService
    {
        private readonly IEventsRepository _eventsRepository;
        private readonly ICacheService _cache;

        // Cache TTLs for events
        private static readonly TimeSpan EventListCacheTTL = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan EventItemCacheTTL = TimeSpan.FromHours(1);

        /// <summary>
        /// Events Service constructor
        /// </summary>
        /// <param name="eventsRepository">Events repository</param>
        /// <param name="cache">Cache service</param>
        public EventsService(IEventsRepository eventsRepository, ICacheService cache)
        {
            _eventsRepository = eventsRepository;
            _cache = cache;
        }

        /// <summary>
        /// Gets all events with optional inactive filter
        /// </summary>
        public async Task<SystemResponse<AllEventsResponse>> GetAllEvents(bool includeInactive = false)
        {
            var cacheKey = CacheKeys.Format(CacheKeys.EventsAll, includeInactive);
            if (_cache.CanReadFromCache(cacheKey))
            {
                return _cache.ReadFromCache<SystemResponse<AllEventsResponse>>(cacheKey);
            }

            var events = await _eventsRepository.GetAllEvents(includeInactive);
            var summaries = events.Select(ConvertToEventSummary).ToList();

            var response = new AllEventsResponse
            {
                Events = summaries,
                TotalCount = summaries.Count
            };

            var systemResponse = new SystemResponse<AllEventsResponse>(response, "Success!");
            _cache.InsertIntoCache(cacheKey, systemResponse, EventListCacheTTL);

            return systemResponse;
        }

        /// <summary>
        /// Gets upcoming events from a specified date
        /// </summary>
        public async Task<SystemResponse<AllEventsResponse>> GetUpcomingEvents(DateTime? fromDate = null, int count = 10)
        {
            if (count <= 0 || count > 100)
            {
                return new SystemResponse<AllEventsResponse>(true, "Count must be between 1 and 100.");
            }

            var effectiveDate = fromDate ?? DateTime.UtcNow;
            var events = await _eventsRepository.GetUpcomingEvents(effectiveDate, count);
            var summaries = events.Select(ConvertToEventSummary).ToList();

            var response = new AllEventsResponse
            {
                Events = summaries,
                TotalCount = summaries.Count
            };

            return new SystemResponse<AllEventsResponse>(response, "Success!");
        }

        /// <summary>
        /// Gets events within a date range
        /// </summary>
        public async Task<SystemResponse<AllEventsResponse>> GetEventsByDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                return new SystemResponse<AllEventsResponse>(true, SystemMessages.EndDateMustBeAfterStartDate);
            }

            var events = await _eventsRepository.GetEventsByDateRange(startDate, endDate);
            var summaries = events.Select(ConvertToEventSummary).ToList();

            var response = new AllEventsResponse
            {
                Events = summaries,
                TotalCount = summaries.Count
            };

            return new SystemResponse<AllEventsResponse>(response, "Success!");
        }

        /// <summary>
        /// Gets a single event by its ID
        /// </summary>
        public async Task<SystemResponse<EventResponse>> GetEventById(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return new SystemResponse<EventResponse>(true, string.Format(SystemMessages.NullProperty, "eventId"));
            }

            var cacheKey = CacheKeys.Format(CacheKeys.EventItem, eventId);
            if (_cache.CanReadFromCache(cacheKey))
            {
                return _cache.ReadFromCache<SystemResponse<EventResponse>>(cacheKey);
            }

            var eventEntity = await _eventsRepository.GetEventById(eventId);
            if (eventEntity == null)
            {
                return new SystemResponse<EventResponse>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "event", eventId));
            }

            var response = new EventResponse { Event = eventEntity };
            var systemResponse = new SystemResponse<EventResponse>(response, "Success!");
            _cache.InsertIntoCache(cacheKey, systemResponse, EventItemCacheTTL);

            return systemResponse;
        }

        /// <summary>
        /// Gets featured events that are active and upcoming
        /// </summary>
        public async Task<SystemResponse<AllEventsResponse>> GetFeaturedEvents()
        {
            var cacheKey = CacheKeys.EventsFeatured;
            if (_cache.CanReadFromCache(cacheKey))
            {
                return _cache.ReadFromCache<SystemResponse<AllEventsResponse>>(cacheKey);
            }

            var events = await _eventsRepository.GetFeaturedEvents();
            var summaries = events.Select(ConvertToEventSummary).ToList();

            var response = new AllEventsResponse
            {
                Events = summaries,
                TotalCount = summaries.Count
            };

            var systemResponse = new SystemResponse<AllEventsResponse>(response, "Success!");
            _cache.InsertIntoCache(cacheKey, systemResponse, EventListCacheTTL);

            return systemResponse;
        }

        /// <summary>
        /// Creates a new event
        /// </summary>
        public async Task<SystemResponse<EventResponse>> CreateEvent(CreateEventRequest request)
        {
            var validationResponse = CreateEventRequest.ValidateRequest(request);
            if (validationResponse.HasErrors)
            {
                return new SystemResponse<EventResponse>(true, validationResponse.ErrorMessage);
            }

            var newEvent = new Event
            {
                Title = request.Title,
                Summary = request.Summary,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                ThumbnailUrl = request.ThumbnailUrl,
                IconName = request.IconName,
                StartTime = request.StartTime.ToUniversalTime(),
                EndTime = request.EndTime?.ToUniversalTime(),
                IsAllDay = request.IsAllDay,
                IsRecurring = request.IsRecurring,
                Recurrence = request.Recurrence,
                IsOnline = request.IsOnline,
                OnlineLink = request.OnlineLink,
                OnlinePlatform = request.OnlinePlatform,
                Location = request.Location,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                RegistrationUrl = request.RegistrationUrl,
                Tags = request.Tags ?? new List<string>(),
                IsFeatured = request.IsFeatured,
                IsActive = true
            };

            var createdEvent = await _eventsRepository.CreateEvent(newEvent);
            if (createdEvent == null)
            {
                return new SystemResponse<EventResponse>(true, string.Format(SystemMessages.ErrorOcurredInsertingIntoCollection, "Events"));
            }

            // Invalidate relevant caches
            InvalidateEventCaches();

            var response = new EventResponse { Event = createdEvent };
            return new SystemResponse<EventResponse>(response, "Success!");
        }

        /// <summary>
        /// Updates an existing event
        /// </summary>
        public async Task<SystemResponse<EventResponse>> UpdateEvent(string eventId, UpdateEventRequest request)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return new SystemResponse<EventResponse>(true, string.Format(SystemMessages.NullProperty, "eventId"));
            }

            var validationResponse = UpdateEventRequest.ValidateRequest(request);
            if (validationResponse.HasErrors)
            {
                return new SystemResponse<EventResponse>(true, validationResponse.ErrorMessage);
            }

            // Get existing event
            var existingEvent = await _eventsRepository.GetEventById(eventId);
            if (existingEvent == null)
            {
                return new SystemResponse<EventResponse>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "event", eventId));
            }

            // Apply partial updates
            ApplyUpdates(existingEvent, request);

            var updatedEvent = await _eventsRepository.UpdateEvent(eventId, existingEvent);
            if (updatedEvent == null)
            {
                return new SystemResponse<EventResponse>(true, string.Format(SystemMessages.UnableToUpdatePropertyForId, "event", eventId));
            }

            // Invalidate all event caches
            InvalidateEventCaches();

            var response = new EventResponse { Event = updatedEvent };
            return new SystemResponse<EventResponse>(response, "Success!");
        }

        /// <summary>
        /// Permanently deletes an event
        /// </summary>
        public async Task<SystemResponse<string>> DeleteEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return new SystemResponse<string>(true, string.Format(SystemMessages.NullProperty, "eventId"));
            }

            var result = await _eventsRepository.DeleteEvent(eventId);
            if (!result)
            {
                return new SystemResponse<string>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "event", eventId));
            }

            // Invalidate all event caches
            InvalidateEventCaches();

            return new SystemResponse<string>("Event deleted successfully.", "Success!");
        }

        /// <summary>
        /// Soft deletes an event by setting IsActive to false
        /// </summary>
        public async Task<SystemResponse<EventResponse>> DeactivateEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return new SystemResponse<EventResponse>(true, string.Format(SystemMessages.NullProperty, "eventId"));
            }

            // First get the event to verify it exists
            var existingEvent = await _eventsRepository.GetEventById(eventId);
            if (existingEvent == null)
            {
                return new SystemResponse<EventResponse>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "event", eventId));
            }

            // Deactivate the event
            var success = await _eventsRepository.DeactivateEvent(eventId);
            if (!success)
            {
                return new SystemResponse<EventResponse>(true, "Failed to deactivate the event.");
            }

            // Invalidate all event caches
            InvalidateEventCaches();

            // Fetch the updated event to return
            var deactivatedEvent = await _eventsRepository.GetEventById(eventId);
            var response = new EventResponse { Event = deactivatedEvent };
            return new SystemResponse<EventResponse>(response, "Success!");
        }

        #region Private Helper Methods

        /// <summary>
        /// Converts an Event entity to an EventSummary
        /// </summary>
        private static EventSummary ConvertToEventSummary(Event e)
        {
            return new EventSummary
            {
                Id = e.Id,
                Title = e.Title,
                Summary = e.Summary,
                ThumbnailUrl = e.ThumbnailUrl,
                IconName = e.IconName,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                IsRecurring = e.IsRecurring,
                RecurrencePattern = e.Recurrence?.Pattern,
                RecurrenceDayOfWeek = e.Recurrence?.DayOfWeek,
                IsOnline = e.IsOnline,
                LocationName = e.Location?.Name,
                IsFeatured = e.IsFeatured,
                IsActive = e.IsActive,
                Tags = e.Tags ?? new List<string>()
            };
        }

        /// <summary>
        /// Applies partial updates from request to existing event
        /// </summary>
        private static void ApplyUpdates(Event existingEvent, UpdateEventRequest request)
        {
            if (!string.IsNullOrEmpty(request.Title))
                existingEvent.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Summary))
                existingEvent.Summary = request.Summary;
            if (request.Description != null)
                existingEvent.Description = request.Description;
            if (request.ImageUrl != null)
                existingEvent.ImageUrl = request.ImageUrl;
            if (request.ThumbnailUrl != null)
                existingEvent.ThumbnailUrl = request.ThumbnailUrl;
            if (request.IconName != null)
                existingEvent.IconName = request.IconName;
            if (request.StartTime.HasValue)
                existingEvent.StartTime = request.StartTime.Value.ToUniversalTime();
            if (request.EndTime.HasValue)
                existingEvent.EndTime = request.EndTime.Value.ToUniversalTime();
            if (request.IsAllDay.HasValue)
                existingEvent.IsAllDay = request.IsAllDay.Value;
            if (request.IsRecurring.HasValue)
                existingEvent.IsRecurring = request.IsRecurring.Value;
            if (request.Recurrence != null)
                existingEvent.Recurrence = request.Recurrence;
            if (request.IsOnline.HasValue)
                existingEvent.IsOnline = request.IsOnline.Value;
            if (request.OnlineLink != null)
                existingEvent.OnlineLink = request.OnlineLink;
            if (request.OnlinePlatform != null)
                existingEvent.OnlinePlatform = request.OnlinePlatform;
            if (request.Location != null)
                existingEvent.Location = request.Location;
            if (request.ContactEmail != null)
                existingEvent.ContactEmail = request.ContactEmail;
            if (request.ContactPhone != null)
                existingEvent.ContactPhone = request.ContactPhone;
            if (request.RegistrationUrl != null)
                existingEvent.RegistrationUrl = request.RegistrationUrl;
            if (request.Tags != null)
                existingEvent.Tags = request.Tags;
            if (request.IsActive.HasValue)
                existingEvent.IsActive = request.IsActive.Value;
            if (request.IsFeatured.HasValue)
                existingEvent.IsFeatured = request.IsFeatured.Value;
        }

        /// <summary>
        /// Invalidates all event-related caches using pattern-based removal
        /// </summary>
        private void InvalidateEventCaches()
        {
            _cache.RemoveByPattern(CacheKeys.EventsPattern);
        }

        #endregion

        #region Recurrence Calculation

        /// <summary>
        /// Maximum number of occurrences to calculate to prevent infinite loops
        /// </summary>
        private const int MaxOccurrences = 365;

        /// <inheritdoc />
        public List<DateTime> CalculateRecurringDates(Event evt, DateTime fromDate, DateTime toDate)
        {
            var dates = new List<DateTime>();

            // Return empty if event is null or not recurring
            if (evt == null || !evt.IsRecurring || evt.Recurrence == null)
            {
                return dates;
            }

            // Return empty if pattern is None
            if (evt.Recurrence.Pattern == RecurrencePattern.None)
            {
                return dates;
            }

            var current = evt.StartTime;
            var recurrenceEnd = evt.Recurrence.EndDate ?? toDate;
            var effectiveEnd = recurrenceEnd < toDate ? recurrenceEnd : toDate;

            // Safety counter to prevent infinite loops
            int iterations = 0;

            while (current <= effectiveEnd && iterations < MaxOccurrences)
            {
                if (current >= fromDate)
                {
                    dates.Add(current);
                }

                current = GetNextOccurrenceDate(current, evt.Recurrence.Pattern, evt.Recurrence.Interval, evt.StartTime);

                // Break if we can't calculate next date
                if (current == DateTime.MinValue)
                {
                    break;
                }

                iterations++;
            }

            return dates;
        }

        /// <summary>
        /// Calculate the next occurrence date based on the pattern
        /// </summary>
        private static DateTime GetNextOccurrenceDate(DateTime currentDate, RecurrencePattern pattern, int interval, DateTime originalStart)
        {
            // Ensure interval is at least 1
            interval = Math.Max(1, interval);

            switch (pattern)
            {
                case RecurrencePattern.Daily:
                    return currentDate.AddDays(interval);

                case RecurrencePattern.Weekly:
                    return currentDate.AddDays(7 * interval);

                case RecurrencePattern.BiWeekly:
                    return currentDate.AddDays(14);

                case RecurrencePattern.Monthly:
                    return AddMonthsPreservingDay(currentDate, interval, originalStart.Day);

                case RecurrencePattern.Yearly:
                    return currentDate.AddYears(interval);

                case RecurrencePattern.None:
                default:
                    return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Add months while trying to preserve the original day of month.
        /// Handles edge cases like Jan 31 + 1 month = Feb 28/29
        /// </summary>
        private static DateTime AddMonthsPreservingDay(DateTime date, int months, int originalDay)
        {
            var newDate = date.AddMonths(months);
            var targetDay = Math.Min(originalDay, DateTime.DaysInMonth(newDate.Year, newDate.Month));
            return new DateTime(newDate.Year, newDate.Month, targetDay, date.Hour, date.Minute, date.Second, date.Kind);
        }

        #endregion
    }
}

