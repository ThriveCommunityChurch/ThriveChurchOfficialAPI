using System.ComponentModel.DataAnnotations;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Request model for user login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Username or email for authentication
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// User password
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
