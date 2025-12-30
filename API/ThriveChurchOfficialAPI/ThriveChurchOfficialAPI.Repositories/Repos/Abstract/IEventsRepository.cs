using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Interface for Events repository operations
    /// </summary>
    public interface IEventsRepository
    {
        /// <summary>
        /// Get all events, optionally including inactive events
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive (soft-deleted) events</param>
        /// <returns>Collection of events</returns>
        Task<IEnumerable<Event>> GetAllEvents(bool includeInactive = false);

        /// <summary>
        /// Get upcoming events starting from a specific date
        /// </summary>
        /// <param name="fromDate">The date to start from</param>
        /// <param name="count">Maximum number of events to return</param>
        /// <returns>Collection of upcoming events</returns>
        Task<IEnumerable<Event>> GetUpcomingEvents(DateTime fromDate, int count = 10);

        /// <summary>
        /// Get events within a specific date range
        /// </summary>
        /// <param name="startDate">Start of the date range</param>
        /// <param name="endDate">End of the date range</param>
        /// <returns>Collection of events within the date range</returns>
        Task<IEnumerable<Event>> GetEventsByDateRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get a single event by its ID
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <returns>The event if found, null otherwise</returns>
        Task<Event> GetEventById(string eventId);

        /// <summary>
        /// Get all featured events that are active and upcoming
        /// </summary>
        /// <returns>Collection of featured events</returns>
        Task<IEnumerable<Event>> GetFeaturedEvents();

        /// <summary>
        /// Create a new event
        /// </summary>
        /// <param name="newEvent">The event to create</param>
        /// <returns>The created event with generated ID</returns>
        Task<Event> CreateEvent(Event newEvent);

        /// <summary>
        /// Update an existing event
        /// </summary>
        /// <param name="eventId">The ID of the event to update</param>
        /// <param name="updatedEvent">The updated event data</param>
        /// <returns>The updated event</returns>
        Task<Event> UpdateEvent(string eventId, Event updatedEvent);

        /// <summary>
        /// Permanently delete an event (hard delete)
        /// </summary>
        /// <param name="eventId">The ID of the event to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteEvent(string eventId);

        /// <summary>
        /// Deactivate an event (soft delete)
        /// </summary>
        /// <param name="eventId">The ID of the event to deactivate</param>
        /// <returns>True if deactivation was successful</returns>
        Task<bool> DeactivateEvent(string eventId);
    }
}

