using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SermonsController : ControllerBase
    {
        private readonly ISermonsService _sermonsService;

        public SermonsController(ISermonsService sermonsService)
        {
            // delay the init of the repo for when we go to the service, we will grab the connection 
            // string from the IConfiguration object there instead of init-ing the repo here
            _sermonsService = sermonsService;
        }

        // GET api/sermons
        [HttpGet]
        public async Task<ActionResult<AllSermonsResponse>> GetAllSermons()
        {
            var response = await _sermonsService.GetAllSermons();

            var value = new ActionResult<AllSermonsResponse>(response);

            return value;
        }

        [HttpPost("series")]
        public async Task<ActionResult<SermonSeries>> CreateNewSermonSeries([FromBody] SermonSeries request)
        {
            var response = await _sermonsService.CreateNewSermonSeries(request);

            var value = new ActionResult<SermonSeries>(response);

            return value;
        }

        // this query string should contain an Id
        [HttpPut("series/{SeriesId}")]
        public async Task<ActionResult<SermonSeries>> ModifySermonSeries([FromQuery] string SeriesId, [FromBody] SermonSeriesUpdateRequest request)
        {
            var response = await _sermonsService.ModifySermonSeries(SeriesId, request);

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<SermonSeries>(response);

            return value;
        }

        [HttpPost("series/{SeriesId}/message")]
        public async Task<ActionResult<SermonSeries>> AddMessagesToSermonSeries(string SeriesId, [FromBody] AddMessagesToSeriesRequest request)
        {
            var response = await _sermonsService.AddMessageToSermonSeries(SeriesId, request);

            var value = new ActionResult<SermonSeries>(response);

            return value;
        }

        [HttpPut("series/{SeriesId}/message")]
        public async Task<ActionResult<SermonSeries>> UpdateMessagesInSermonSeries(string SeriesId, [FromBody] UpdateMessagesInSermonSeriesRequest request)
        {
            //var response = await _sermonsService.UpdateMessagesInSermonSeries(request);

            //var value = new ActionResult<SermonSeries>(response);

            return null;
        }

        [HttpGet("live")]
        public async Task<ActionResult<LiveStreamingResponse>> GetLiveSermons()
        {
            var response = await _sermonsService.GetLiveSermons();

            var value = new ActionResult<LiveStreamingResponse>(response).Value;

            return value;
        }

        [HttpPut("live")]
        public async Task<ActionResult<LiveStreamingResponse>> UpdateLiveSermons([FromBody] LiveSermonsUpdateRequest request)
        {
            var response = await _sermonsService.UpdateLiveSermons(request);

            return response;
        }

        [HttpPut("live/special")]
        public async Task<ActionResult<LiveStreamingResponse>> UpdateLiveForSpecialEvents([FromBody] LiveSermonsSpecialEventUpdateRequest request)
        {
            var response = await _sermonsService.UpdateLiveForSpecialEvents(request);

            return response;
        }

        [HttpGet("live/poll")]
        public async Task<ActionResult<LiveSermonsPollingResponse>> PollForLiveEventData()
        {
            var response = await _sermonsService.PollForLiveEventData();

            return response;
        }

        [HttpDelete("live")]
        public async Task<ActionResult<LiveSermons>> UpdateLiveSermonsInactive()
        {
            var response = await _sermonsService.UpdateLiveSermonsInactive();

            return response;
        }
    }
}