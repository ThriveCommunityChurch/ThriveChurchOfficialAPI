using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    /// <summary>
    /// Passages Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PassagesController : ControllerBase
    {
        private readonly IPassagesService _passagesService;

        /// <summary>
        /// </summary>
        /// <param name="passageService"></param>
        public PassagesController(IPassagesService passageService)
        {
            _passagesService = passageService;
        }

        /// <summary>
        /// Get bible passage for free text search
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns>Formatted Bible Passages</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<SermonPassageResponse>> Get([FromQuery] string searchCriteria)
        {
            var response = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }
    }
}
