using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ThriveChurchOfficialAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassagesController : ControllerBase
    {
        private readonly IOptions<AppSettings> config;

        public PassagesController(IOptions<AppSettings> config)
        {
            this.config = EsvApiKey;
        }

        // GET api/passage
        [HttpGet]
        public ActionResult<string> Get()
        {
            return  "value1 value2";
        }

        // GET api/passage/{id}
        [HttpGet("{id}")]
        public ActionResult<string> GetPassageForId(int id)
        {
            return "value";
        }

        // GET api/passage/{query}
        [HttpGet("{id}")]
        public ActionResult<string> GetPassageForQuery(int id)
        {
            return "value";
        }
    }
}
