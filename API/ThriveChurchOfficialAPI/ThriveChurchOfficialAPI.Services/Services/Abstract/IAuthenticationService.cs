using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Authentication service interface for user login and validation
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate a user with username/email and password
        /// </summary>
        /// <param name="request">Login request with credentials</param>
        /// <returns>Login response with JWT token if successful</returns>
        Task<SystemResponse<LoginResponse>> LoginAsync(HttpContext webRequest, LoginRequest request);

        /// <summary>
        /// Refresh a JWT token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New login response with fresh JWT token</returns>
        Task<SystemResponse<LoginResponse>> RefreshTokenAsync(HttpContext webRequest, RefreshTokenRequest request);

        /// <summary>
        /// Validate user credentials against stored password hash
        /// </summary>
        /// <param name="user">User from database</param>
        /// <param name="password">Plain text password to verify</param>
        /// <returns>True if password is valid, false otherwise</returns>
        bool ValidatePassword(User user, string password);

        /// <summary>
        /// Hash a password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password</returns>
        string HashPassword(string password);

        /// <summary>
        /// Unlock a user account (admin function)
        /// </summary>
        /// <param name="userId">User ID to unlock</param>
        /// <returns>Success response</returns>
        Task<SystemResponse<string>> UnlockUserAccountAsync(string userId);

        /// <summary>
        /// Validate password complexity requirements
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Validation result with error message if invalid</returns>
        SystemResponse<string> ValidatePasswordComplexity(string password);
    }
}
