using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecentlyPlayedController : ControllerBase
    {
        private readonly ISermonsService _sermonsService;

        public RecentlyPlayedController(ISermonsService sermonsService)
        {
            _sermonsService = sermonsService;
        }

        // GET api/recentlyPlayed/{userId}
        [HttpGet("{UserId}")]
        public async Task<ActionResult<RecentlyWatchedMessagesResponse>> GetRecentlyWatched(string UserId)
        {
            var response = await _sermonsService.GetRecentlyWatched(UserId);

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<RecentlyWatchedMessagesResponse>(response);

            return value;
        }

        /// <summary>
        /// Store a user's recently played sermon messages
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[HttpPost("{UserId}")]
        //public async Task<ActionResult<RecentlyWatchedMessagesResponse>> CreateRecentlyWatched([FromBody] CreateRecentlyWatchedMessagesRequest request)
        //{
        //    var response = await _sermonsService.CreateRecentlyWatched(request);

        //    if (response == null)
        //    {
        //        return StatusCode(400);
        //    }

        //    var value = new ActionResult<RecentlyWatchedMessagesResponse>(response);

        //    return value;
        //}
    }
}
