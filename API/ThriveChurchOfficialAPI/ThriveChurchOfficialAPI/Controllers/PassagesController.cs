using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        /// 
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        [HttpGet]
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
