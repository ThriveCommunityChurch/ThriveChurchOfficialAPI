using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Refresh token model for MongoDB storage
    /// </summary>
    public class RefreshToken : ObjectBase
    {
        /// <summary>
        /// The actual refresh token value (base64 encoded)
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// User ID this refresh token belongs to
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// When this refresh token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Whether this refresh token has been used (for one-time use tokens)
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Whether this refresh token has been revoked
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// When this refresh token was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this refresh token was used (if applicable)
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// When this refresh token was revoked (if applicable)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// IP address where this token was created
        /// </summary>
        public string CreatedByIp { get; set; }

        /// <summary>
        /// IP address where this token was used
        /// </summary>
        public string UsedByIp { get; set; }

        /// <summary>
        /// IP address where this token was revoked
        /// </summary>
        public string RevokedByIp { get; set; }

        /// <summary>
        /// Check if this refresh token is currently valid
        /// </summary>
        public bool IsValid => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAt;

        /// <summary>
        /// Check if this refresh token is expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
