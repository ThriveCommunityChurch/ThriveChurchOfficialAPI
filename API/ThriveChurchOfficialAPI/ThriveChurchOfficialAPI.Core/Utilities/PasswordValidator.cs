using System;
using System.Linq;

namespace ThriveChurchOfficialAPI.Core.Utilities
{
    /// <summary>
    /// Password validation utility for enforcing password complexity requirements
    /// </summary>
    public static class PasswordValidator
    {
        /// <summary>
        /// Minimum password length requirement
        /// </summary>
        public const int MinimumLength = 10;

        /// <summary>
        /// Validate password against complexity requirements
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password meets requirements, false otherwise</returns>
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            // Check minimum length
            if (password.Length < MinimumLength)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get password requirements description for user display
        /// </summary>
        /// <returns>Human-readable password requirements</returns>
        public static string GetPasswordRequirements()
        {
            return $"Password must be at least {MinimumLength} characters long";
        }

        /// <summary>
        /// Validate password and return detailed error message if invalid
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Error message if invalid, null if valid</returns>
        public static string ValidatePasswordWithMessage(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return "Password is required";
            }

            if (password.Length < MinimumLength)
            {
                return $"Password must be at least {MinimumLength} characters long";
            }

            return null; // Valid password
        }
    }
}
