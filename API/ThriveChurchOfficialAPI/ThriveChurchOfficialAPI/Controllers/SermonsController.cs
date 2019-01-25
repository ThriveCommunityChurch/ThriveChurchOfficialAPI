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
        public async Task<ActionResult<AllSermonsSummaryResponse>> GetAllSermons()
        {
            var response = await _sermonsService.GetAllSermons();

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<AllSermonsSummaryResponse>(response);

            return value;
        }

        /// <summary>
        /// Recieve Sermon Series in a paged format
        /// </summary>
        /// <remarks>
        /// This will return the sermon series' in a paged format. 
        /// <br />
        /// 
        /// NOTE: 
        /// <br />
        /// &#8901; The first page will contain the 5 first messagess.
        /// &#8901; Every subsequent page will contain up to 10 messages.
        /// &#8901; The response will contain the total number of pages.
        /// 
        /// </remarks>
        /// <param name="PageNumber"></param>
        /// <param name="PageCount"></param>
        /// <returns></returns>
        // GET api/sermons
        [HttpGet("paged")]
        public async Task<ActionResult<SermonsSummaryPagedResponse>> GetPagedSermons(int PageNumber)
        {
            SermonsSummaryPagedResponse response = await _sermonsService.GetPagedSermons(PageNumber);

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<SermonsSummaryPagedResponse>(response);

            return value;
        }

        [HttpPost("series")]
        public async Task<ActionResult<SermonSeries>> CreateNewSermonSeries([FromBody] SermonSeries request)
        {
            var response = await _sermonsService.CreateNewSermonSeries(request);

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<SermonSeries>(response);

            return value;
        }

        // this query string should contain an Id
        [HttpGet("series/{SeriesId}")]
        public async Task<ActionResult<SermonSeries>> GetSeriesForId(string SeriesId)
        {
            var response = await _sermonsService.GetSeriesForId(SeriesId);

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<SermonSeries>(response);

            return value;
        }

        // this query string should contain an Id
        [HttpPut("series/{SeriesId}")]
        public async Task<ActionResult<SermonSeries>> ModifySermonSeries(string SeriesId, [FromBody] SermonSeriesUpdateRequest request)
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

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<SermonSeries>(response);

            return value;
        }

        [HttpPut("series/message/{MessageId}")]
        public async Task<ActionResult<SermonMessage>> UpdateMessagesInSermonSeries(string MessageId, [FromBody] UpdateMessagesInSermonSeriesRequest request)
        {
            var response = await _sermonsService.UpdateMessageInSermonSeries(MessageId, request);

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<SermonMessage>(response);

            return value;
        }

        [HttpGet("live")]
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

        [HttpPut("live")]
        public async Task<ActionResult<LiveStreamingResponse>> UpdateLiveSermons([FromBody] LiveSermonsUpdateRequest request)
        {
            var response = await _sermonsService.UpdateLiveSermons(request);

            if (response == null)
            {
                return StatusCode(400);
            }

            return response;
        }

        [HttpPut("live/special")]
        public async Task<ActionResult<LiveStreamingResponse>> UpdateLiveForSpecialEvents([FromBody] LiveSermonsSpecialEventUpdateRequest request)
        {
            var response = await _sermonsService.UpdateLiveForSpecialEvents(request);

            if (response == null)
            {
                return StatusCode(400);
            }

            return response;
        }

        [HttpGet("live/poll")]
        public async Task<ActionResult<LiveSermonsPollingResponse>> PollForLiveEventData()
        {
            var response = await _sermonsService.PollForLiveEventData();

            if (response == null)
            {
                return StatusCode(400, "");
            }

            return response;
        }

        [HttpDelete("live")]
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