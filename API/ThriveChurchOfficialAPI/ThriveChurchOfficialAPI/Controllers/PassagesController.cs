﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassagesController : ControllerBase
    {
        private readonly IPassagesService _passagesService;
        private readonly IPassagesRepository _passagesRepository;

        public PassagesController(IConfiguration configuration)
        {
            _passagesRepository = new PassagesRepository();
            _passagesService = new PassagesService(configuration, _passagesRepository);
        }

        // GET api/passage
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            return await _passagesService.GetAllPassages();
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
