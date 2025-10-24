using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Response model for successful login
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// When the token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Refresh token for getting new access tokens
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
