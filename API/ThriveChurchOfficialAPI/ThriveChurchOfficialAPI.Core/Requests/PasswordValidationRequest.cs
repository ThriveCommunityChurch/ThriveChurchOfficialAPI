using System.ComponentModel.DataAnnotations;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Request model for password validation
    /// </summary>
    public class PasswordValidationRequest
    {
        /// <summary>
        /// Password to validate
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
