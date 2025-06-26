namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// JWT configuration settings read from appsettings.json
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key used for signing JWT tokens
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// JWT token issuer
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// JWT token audience
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// JWT token expiration time in minutes
        /// </summary>
        public int ExpirationMinutes { get; set; }

        /// <summary>
        /// Refresh token expiration time in days
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; }
    }
}
