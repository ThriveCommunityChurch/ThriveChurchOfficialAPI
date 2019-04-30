using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    /// <summary>
    /// Sermons Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SermonsController : ControllerBase
    {
        private readonly ISermonsService _sermonsService;

        /// <summary>
        /// Sermons Controller C'tor
        /// </summary>
        /// <param name="sermonsService"></param>
        public SermonsController(ISermonsService sermonsService)
        {
            // delay the init of the repo for when we go to the service, we will grab the connection 
            // string from the IConfiguration object there instead of init-ing the repo here
            _sermonsService = sermonsService;
        }

        /// <summary>
        /// Get a summary of every sermon series
        /// </summary>
        /// <returns>SermonsSummary object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<AllSermonsSummaryResponse>> GetAllSermons()
        {
            var response = await _sermonsService.GetAllSermons();

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Recieve Sermon Series in a paged format
        /// </summary>
        /// <remarks>
        /// This will return the sermon series' in a paged format. 
        /// <br />
        /// NOTE: 
        /// <br />
        /// &#8901; The first page will contain the 5 first messagess.
        /// &#8901; Every subsequent page will contain up to 10 messages.
        /// &#8901; The response will contain the total number of pages.
        /// </remarks>
        /// <param name="PageNumber"></param>
        /// <returns>Paged Sermon Data</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("paged")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<SermonsSummaryPagedResponse>> GetPagedSermons([BindRequired] int PageNumber)
        {
            var response = await _sermonsService.GetPagedSermons(PageNumber);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Start a new sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpPost("series")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<SermonSeries>> CreateNewSermonSeries([FromBody] SermonSeries request)
        {
            var response = await _sermonsService.CreateNewSermonSeries(request);

            if (response == null)
            {
                return StatusCode(400);
            }

            if (response.SuccessMessage == "202")
            {
                // Return a 202 here because this is valid, however there is something else active and nothing was done
                // "The request has been received but not yet acted upon" is what I would expect to be a correct response
                // More on that here https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/202
                return StatusCode(202, response.Result);
            }

            return response.Result;
        }

        /// <summary>
        /// Get a sermon series
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <returns>SermonSeries object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("series/{SeriesId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<SermonSeries>> GetSeriesForId([BindRequired] string SeriesId)
        {
            var response = await _sermonsService.GetSeriesForId(SeriesId);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Update a sermon series including messages
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <param name="request"></param>
        /// <returns>SermonSeries object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpPut("series/{SeriesId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<SermonSeries>> ModifySermonSeries([BindRequired] string SeriesId, [FromBody] SermonSeriesUpdateRequest request)
        {
            var response = await _sermonsService.ModifySermonSeries(SeriesId, request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Add a message to a sermon series
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <param name="request"></param>
        /// <returns>SermonSeries Object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpPost("series/{SeriesId}/message")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<SermonSeries>> AddMessagesToSermonSeries([BindRequired] string SeriesId, [FromBody] AddMessagesToSeriesRequest request)
        {
            var response = await _sermonsService.AddMessageToSermonSeries(SeriesId, request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Modify a sermon message
        /// </summary>
        /// <param name="MessageId"></param>
        /// <param name="request"></param>
        /// <returns>An updated sermon message</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpPut("series/message/{MessageId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<SermonMessage>> UpdateMessagesInSermonSeries([BindRequired] string MessageId, [FromBody] UpdateMessagesInSermonSeriesRequest request)
        {
            var response = await _sermonsService.UpdateMessageInSermonSeries(MessageId, request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get Livestreaming information
        /// </summary>
        /// <returns>Live Streaming info</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("live")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<LiveStreamingResponse>> GetLiveSermons()
        {
            var response = await _sermonsService.GetLiveSermons();

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<LiveStreamingResponse>(response).Value;

            return value;
        }

        /// <summary>
        /// Set and activate the livestream
        /// </summary>
        /// <param name="request"></param>
        /// <returns>LiveSermon Object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpPost("live")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<LiveStreamingResponse>> GoLive([FromBody] LiveSermonsUpdateRequest request)
        {
            var response = await _sermonsService.GoLive(request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Set an active livestream for a special event
        /// </summary>
        /// <param name="request"></param>
        /// <returns>LiveSermon Object</returns>
        [HttpPut("live/special")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<LiveStreamingResponse>> UpdateLiveForSpecialEvents([FromBody] LiveSermonsSpecialEventUpdateRequest request)
        {
            var response = await _sermonsService.UpdateLiveForSpecialEvents(request);

            if (response == null)
            {
                return StatusCode(400);
            }

            return response;
        }

        /// <summary>
        /// Get active livstream data
        /// </summary>
        /// <remarks>
        /// This is a polling route, meaning that you can call this in a loop if you wish. However, please keep in mind the Rate Limits. <br />
        /// <br />
        /// This response includes an end time, so you may not need to call this in a loop.
        /// </remarks>
        /// <returns>LiveSermon info</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("live/poll")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<LiveSermonsPollingResponse>> PollForLiveEventData()
        {
            var response = await _sermonsService.PollForLiveEventData();

            if (response == null)
            {
                return StatusCode(400, "");
            }

            return response;
        }

        /// <summary>
        /// End a livestream 
        /// </summary>
        /// <returns>LiveSermon Object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpDelete("live")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<LiveSermons>> UpdateLiveSermonsInactive()
        {
            var response = await _sermonsService.UpdateLiveSermonsInactive();

            if (response == null)
            {
                return StatusCode(400);
            }

            return response;
        }
    }
}