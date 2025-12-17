/*
    MIT License

    Copyright (c) 2026 Thrive Community Church

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Controllers
{
    /// <summary>
    /// Health Check Controller for AWS App Runner and load balancer health checks.
    /// This endpoint is excluded from JWT authentication to allow internal health probes.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        /// <summary>
        /// Health Controller Constructor
        /// </summary>
        /// <param name="healthCheckService">ASP.NET Core Health Check Service</param>
        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Simple health check endpoint for AWS App Runner.
        /// Returns 200 OK if the application is running.
        /// This endpoint is anonymous and does not require authentication.
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HealthCheckResponse), 200)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> GetHealth()
        {
            var result = await _healthCheckService.CheckHealthAsync();

            var response = new HealthCheckResponse
            {
                Status = result.Status.ToString(),
                TotalDuration = result.TotalDuration.TotalMilliseconds
            };

            if (result.Status == HealthStatus.Healthy)
            {
                return Ok(response);
            }

            return StatusCode(503, response);
        }

        /// <summary>
        /// Lightweight liveness probe for container orchestration.
        /// Returns 200 OK immediately without any dependency checks.
        /// Use this for AWS App Runner's TCP health check.
        /// </summary>
        /// <returns>200 OK</returns>
        [HttpGet("live")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public IActionResult GetLiveness()
        {
            return Ok(new { Status = "Alive" });
        }

        /// <summary>
        /// Readiness probe that checks if the application is ready to serve traffic.
        /// Includes dependency checks (database connectivity, etc.)
        /// </summary>
        /// <returns>Health status with dependency details</returns>
        [HttpGet("ready")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HealthCheckResponse), 200)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> GetReadiness()
        {
            var result = await _healthCheckService.CheckHealthAsync();

            var response = new HealthCheckResponse
            {
                Status = result.Status.ToString(),
                TotalDuration = result.TotalDuration.TotalMilliseconds
            };

            if (result.Status == HealthStatus.Healthy)
            {
                return Ok(response);
            }

            return StatusCode(503, response);
        }
    }

    /// <summary>
    /// Health check response model
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Current health status (Healthy, Degraded, Unhealthy)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Total time taken to perform health checks in milliseconds
        /// </summary>
        public double TotalDuration { get; set; }
    }
}

