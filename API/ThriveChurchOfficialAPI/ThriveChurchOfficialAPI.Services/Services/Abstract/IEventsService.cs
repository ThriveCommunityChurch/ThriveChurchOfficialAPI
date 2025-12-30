using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service interface for Event operations
    /// </summary>
    public interface IEventsService
    {
        /// <summary>
        /// Gets all events with optional inactive filter
        /// </summary>
        /// <param name="includeInactive">Include inactive events in results</param>
        /// <returns>Collection of event summaries</returns>
        Task<SystemResponse<AllEventsResponse>> GetAllEvents(bool includeInactive = false);

        /// <summary>
        /// Gets upcoming events from a specified date
        /// </summary>
        /// <param name="fromDate">Date to start from (defaults to UTC now)</param>
        /// <param name="count">Maximum number of events to return</param>
        /// <returns>Collection of upcoming event summaries</returns>
        Task<SystemResponse<AllEventsResponse>> GetUpcomingEvents(DateTime? fromDate = null, int count = 10);

        /// <summary>
        /// Gets events within a date range
        /// </summary>
        /// <param name="startDate">Start of date range</param>
        /// <param name="endDate">End of date range</param>
        /// <returns>Collection of event summaries within the range</returns>
        Task<SystemResponse<AllEventsResponse>> GetEventsByDateRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets a single event by its ID
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <returns>Full event details</returns>
        Task<SystemResponse<EventResponse>> GetEventById(string eventId);

        /// <summary>
        /// Gets featured events that are active and upcoming
        /// </summary>
        /// <returns>Collection of featured event summaries</returns>
        Task<SystemResponse<AllEventsResponse>> GetFeaturedEvents();

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <param name="request">Event creation request</param>
        /// <returns>The created event</returns>
        Task<SystemResponse<EventResponse>> CreateEvent(CreateEventRequest request);

        /// <summary>
        /// Updates an existing event
        /// </summary>
        /// <param name="eventId">The event ID to update</param>
        /// <param name="request">Event update request</param>
        /// <returns>The updated event</returns>
        Task<SystemResponse<EventResponse>> UpdateEvent(string eventId, UpdateEventRequest request);

        /// <summary>
        /// Permanently deletes an event
        /// </summary>
        /// <param name="eventId">The event ID to delete</param>
        /// <returns>Success message</returns>
        Task<SystemResponse<string>> DeleteEvent(string eventId);

        /// <summary>
        /// Soft deletes an event by setting IsActive to false
        /// </summary>
        /// <param name="eventId">The event ID to deactivate</param>
        /// <returns>The deactivated event</returns>
        Task<SystemResponse<EventResponse>> DeactivateEvent(string eventId);

        /// <summary>
        /// Calculates occurrence dates for a recurring event within a date range
        /// </summary>
        /// <param name="evt">The event with recurrence settings</param>
        /// <param name="fromDate">Start of the date range</param>
        /// <param name="toDate">End of the date range</param>
        /// <returns>List of occurrence dates</returns>
        List<DateTime> CalculateRecurringDates(Event evt, DateTime fromDate, DateTime toDate);
    }
}

