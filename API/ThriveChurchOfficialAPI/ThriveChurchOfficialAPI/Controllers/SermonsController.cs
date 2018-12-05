using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SermonsController : ControllerBase
    {
        private readonly ISermonsService _sermonsService;

        public SermonsController(IConfiguration configuration)
        {
            // delay the init of the repo for when we go to the service, we will grab the connection 
            // string from the IConfiguration object there instead of init-ing the repo here
            _sermonsService = new SermonsService(configuration);
        }

        // GET api/sermons
        [HttpGet]
        public async Task<ActionResult<AllSermonsResponse>> GetAllSermons()
        {
            var response = await _sermonsService.GetAllSermons();

            var value = new ActionResult<AllSermonsResponse>(response);

            return value;
        }

        [HttpGet("live")]
        public async Task<ActionResult<LiveSermons>> GetLiveSermons()
        {
            var response = await _sermonsService.GetLiveSermons();

            var value = new ActionResult<LiveSermons>(response).Value;

            return value;
        }
    }
}
