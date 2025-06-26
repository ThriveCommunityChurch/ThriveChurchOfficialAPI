using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// User model for MongoDB storage
    /// </summary>
    public class User : ObjectBase
    {
        /// <summary>
        /// User's unique username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User's email address (used in JWT claims)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// BCrypt hashed password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// When the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the user was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// User roles for future permission system
        /// </summary>
        public string[] Roles { get; set; } = new string[0];

        /// <summary>
        /// Number of consecutive failed login attempts
        /// </summary>
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// When the account lockout ends (null if not locked out)
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// When the last failed login attempt occurred
        /// </summary>
        public DateTime? LastFailedLoginAttempt { get; set; }

        /// <summary>
        /// Check if the account is currently locked out
        /// </summary>
        public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

        /// <summary>
        /// Check if the account is both active and not locked out
        /// </summary>
        public bool CanLogin => IsActive && !IsLockedOut;
    }
}
