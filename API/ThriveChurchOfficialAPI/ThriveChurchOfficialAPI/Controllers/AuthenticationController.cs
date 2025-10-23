using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Core.Constants;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    /// <summary>
    /// Authentication Controller for user login and JWT token management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        /// <summary>
        /// Authentication Controller Constructor
        /// </summary>
        /// <param name="authenticationService">Authentication service for user validation</param>
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Authenticate user and generate JWT token
        /// </summary>
        /// <param name="request">Login request with username/email and password</param>
        /// <returns>JWT token and user information if successful</returns>
        /// <response code="200">OK - Returns JWT token and user info</response>
        /// <response code="400">Bad Request - Invalid credentials or validation error</response>
        /// <response code="401">Unauthorized - Authentication failed</response>
        [Produces("application/json")]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authenticationService.LoginAsync(Request.HttpContext, request);

            if (response.HasErrors)
            {
                // Return 401 for authentication failures, 400 for validation errors
                if (response.ErrorMessage == AuthenticationMessages.LoginFailed || 
                    response.ErrorMessage== AuthenticationMessages.InvalidRequest)
                {
                    return Unauthorized(new { message = response.ErrorMessage });
                }
                
                return BadRequest(new { message = response.ErrorMessage });
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New JWT token if successful</returns>
        /// <response code="200">OK - Returns new JWT token</response>
        /// <response code="400">Bad Request - Invalid refresh token</response>
        /// <response code="401">Unauthorized - Refresh token expired or invalid</response>
        [Produces("application/json")]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authenticationService.RefreshTokenAsync(Request.HttpContext, request);

            if (response.HasErrors)
            {
                // Return 401 for token-related failures, 400 for validation errors
                if (response.ErrorMessage == AuthenticationMessages.InvalidRequest || 
                    response.ErrorMessage == AuthenticationMessages.RefreshTokenFailed)
                {
                    return Unauthorized(new { message = response.ErrorMessage });
                }
                
                return BadRequest(new { message = response.ErrorMessage });
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Logout user (optional endpoint for token invalidation)
        /// </summary>
        /// <returns>Success message</returns>
        /// <response code="200">OK - Successfully logged out</response>
        [Produces("application/json")]
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        public ActionResult Logout()
        {
            // In a stateless JWT implementation, logout is typically handled client-side
            // by simply discarding the token. However, you could implement refresh token removal
            // here or token blacklisting if needed for enhanced security.

            return Ok(new { message = "Successfully logged out" });
        }

        /// <summary>
        /// Unlock a user account (Admin only)
        /// </summary>
        /// <param name="userId">User ID to unlock</param>
        /// <returns>Success message</returns>
        /// <response code="200">OK - Account unlocked successfully</response>
        /// <response code="400">Bad Request - Invalid user ID</response>
        /// <response code="401">Unauthorized - Admin access required</response>
        /// <response code="403">Forbidden - Insufficient permissions</response>
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [HttpPost("unlock/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> UnlockAccount(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { message = "User ID is required" });
            }

            var response = await _authenticationService.UnlockUserAccountAsync(userId);

            if (response.HasErrors)
            {
                return BadRequest(new { message = response.ErrorMessage });
            }

            return Ok(new { message = response.Result });
        }

        /// <summary>
        /// Validate password complexity requirements
        /// </summary>
        /// <param name="request">Password validation request</param>
        /// <returns>Validation result</returns>
        /// <response code="200">OK - Password is valid</response>
        /// <response code="400">Bad Request - Password doesn't meet requirements</response>
        [Produces("application/json")]
        [HttpPost("validate-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult ValidatePassword([FromBody] PasswordValidationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Password is required" });
            }

            var response = _authenticationService.ValidatePasswordComplexity(request.Password);

            if (response.HasErrors)
            {
                return BadRequest(new { message = response.ErrorMessage });
            }

            return Ok(new { message = "Password meets complexity requirements" });
        }
    }
}
