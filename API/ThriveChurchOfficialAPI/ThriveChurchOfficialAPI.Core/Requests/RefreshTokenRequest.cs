using System.ComponentModel.DataAnnotations;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Request model for refreshing JWT tokens
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Refresh token to exchange for new JWT
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }
    }
}
