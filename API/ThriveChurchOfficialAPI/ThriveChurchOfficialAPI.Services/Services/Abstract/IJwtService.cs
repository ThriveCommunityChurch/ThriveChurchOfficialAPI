using System;
using System.Security.Claims;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// JWT token generation and validation service interface
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generate a JWT token for a user
        /// </summary>
        /// <param name="user">User to generate token for</param>
        /// <returns>JWT token string</returns>
        string GenerateToken(User user);

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Get the token expiration time
        /// </summary>
        /// <returns>Token expiration DateTime</returns>
        DateTime GetTokenExpiration();

        /// <summary>
        /// Validate a JWT token and extract claims
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
        ClaimsPrincipal ValidateToken(string token);

        /// <summary>
        /// Extract user ID from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID if valid, null if invalid</returns>
        string GetUserIdFromToken(string token);
    }
}
