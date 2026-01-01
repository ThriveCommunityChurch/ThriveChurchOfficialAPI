using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    /// <summary>
    /// Events Controller - Manages church events
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventsService _eventsService;

        /// <summary>
        /// Events Controller Constructor
        /// </summary>
        /// <param name="eventsService">Events service instance</param>
        public EventsController(IEventsService eventsService)
        {
            _eventsService = eventsService;
        }

        /// <summary>
        /// Get all events
        /// </summary>
        /// <remarks>
        /// Returns all events. By default, only active events are returned.
        /// Set includeInactive to true to include inactive events.
        /// </remarks>
        /// <param name="includeInactive">Include inactive events in results</param>
        /// <returns>Collection of event summaries</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AllEventsResponse>> GetAllEvents(bool includeInactive = false)
        {
            var response = await _eventsService.GetAllEvents(includeInactive);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get upcoming events
        /// </summary>
        /// <remarks>
        /// Returns upcoming events starting from the specified date (or current UTC time if not specified).
        /// The count parameter limits the number of events returned (1-100, default 10).
        /// </remarks>
        /// <param name="fromDate">Date to start from (defaults to current UTC time)</param>
        /// <param name="count">Maximum number of events to return (1-100, default 10)</param>
        /// <returns>Collection of upcoming event summaries</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("upcoming")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AllEventsResponse>> GetUpcomingEvents(DateTime? fromDate = null, int count = 10)
        {
            var response = await _eventsService.GetUpcomingEvents(fromDate, count);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get events within a date range
        /// </summary>
        /// <remarks>
        /// Returns all active events that fall within the specified date range.
        /// Both startDate and endDate are required. EndDate must be after StartDate.
        /// </remarks>
        /// <param name="startDate">Start of date range</param>
        /// <param name="endDate">End of date range</param>
        /// <returns>Collection of event summaries within the range</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("range")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AllEventsResponse>> GetEventsByDateRange(
            [BindRequired] DateTime startDate, 
            [BindRequired] DateTime endDate)
        {
            var response = await _eventsService.GetEventsByDateRange(startDate, endDate);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get a single event by ID
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <returns>Full event details</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        [Produces("application/json")]
        [HttpGet("{eventId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EventResponse>> GetEventById([BindRequired] string eventId)
        {
            var response = await _eventsService.GetEventById(eventId);

            if (response.HasErrors)
            {
                if (response.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(response.ErrorMessage);
                }
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get featured events
        /// </summary>
        /// <remarks>
        /// Returns all active featured events that are upcoming or currently happening.
        /// </remarks>
        /// <returns>Collection of featured event summaries</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("featured")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AllEventsResponse>> GetFeaturedEvents()
        {
            var response = await _eventsService.GetFeaturedEvents();

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        /// <param name="request">Event creation request</param>
        /// <returns>The created event</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest request)
        {
            var response = await _eventsService.CreateEvent(request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        /// <remarks>
        /// Updates an event with the provided data. Only fields that are set will be updated.
        /// </remarks>
        /// <param name="eventId">The event ID to update</param>
        /// <param name="request">Event update request</param>
        /// <returns>The updated event</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPut("{eventId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EventResponse>> UpdateEvent(
            [BindRequired] string eventId,
            [FromBody] UpdateEventRequest request)
        {
            var response = await _eventsService.UpdateEvent(eventId, request);

            if (response.HasErrors)
            {
                if (response.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(response.ErrorMessage);
                }
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Delete an event permanently
        /// </summary>
        /// <remarks>
        /// Permanently deletes an event. This action cannot be undone.
        /// Consider using the deactivate endpoint instead for a soft delete.
        /// </remarks>
        /// <param name="eventId">The event ID to delete</param>
        /// <returns>Success message</returns>
        /// <response code="200">OK - Event deleted</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [Authorize]
        [Produces("application/json")]
        [HttpDelete("{eventId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<string>> DeleteEvent([BindRequired] string eventId)
        {
            var response = await _eventsService.DeleteEvent(eventId);

            if (response.HasErrors)
            {
                if (response.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(response.ErrorMessage);
                }
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Deactivate an event (soft delete)
        /// </summary>
        /// <remarks>
        /// Sets an event's IsActive flag to false, effectively hiding it from normal queries.
        /// This is a reversible operation - the event can be reactivated via the update endpoint.
        /// </remarks>
        /// <param name="eventId">The event ID to deactivate</param>
        /// <returns>The deactivated event</returns>
        /// <response code="200">OK - Event deactivated</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPut("{eventId}/deactivate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EventResponse>> DeactivateEvent([BindRequired] string eventId)
        {
            var response = await _eventsService.DeactivateEvent(eventId);

            if (response.HasErrors)
            {
                if (response.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(response.ErrorMessage);
                }
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }
    }
}

