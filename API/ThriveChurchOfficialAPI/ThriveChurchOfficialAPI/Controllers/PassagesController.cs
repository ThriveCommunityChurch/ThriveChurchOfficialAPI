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

        public PassagesController(IPassagesService passageService)
        {
            _passagesService = passageService;
        }

        // GET api/passage
        [HttpGet]
        public async Task<ActionResult<PassagesResponse>> Get(string searchCriteria)
        {
            searchCriteria = "Psalm 119";

            var response = await _passagesService.GetPassagesForSearch(searchCriteria);

            if (response == default(PassagesResponse))
            {
                return new ActionResult<PassagesResponse>(new PassagesResponse());
            }

            var value = new ActionResult<PassagesResponse>(response);

            return value;
        }
    }
}
