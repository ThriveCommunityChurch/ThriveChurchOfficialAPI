using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    /// <summary>
    /// Config Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfigService _configService;

        /// <summary>
        /// Sermons Controller C'tor
        /// </summary>
        /// <param name="configService"></param>
        public ConfigController(IConfigService configService)
        {
            // delay the init of the repo for when we go to the service, we will grab the connection 
            // string from the IConfiguration object there instead of init-ing the repo here
            _configService = configService;
        }

        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <returns>SermonsSummary object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ConfigurationResponse>> GetConfigValue([BindRequired] string setting)
        {
            var response = await _configService.GetConfigValue(setting);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <returns>SermonsSummary object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("list")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ConfigurationCollectionResponse>> GetConfigValues(ConfigKeyRequest request)
        {
            var response = await _configService.GetConfigValues(request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Set values for config settings
        /// </summary>
        /// <returns>SermonsSummary object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpPost("values")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<string>> SetConfigValues([FromBody] SetConfigRequest request)
        {
            var response = await _configService.SetConfigValues(request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Set values for config settings from a CSV
        /// </summary>
        /// <returns>SermonsSummary object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpPost("values/csv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<string>> SetConfigValuesFromCSV([FromBody] string csv)
        {
            var response = await _configService.SetConfigValuesFromCSV(csv);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }
    }
}