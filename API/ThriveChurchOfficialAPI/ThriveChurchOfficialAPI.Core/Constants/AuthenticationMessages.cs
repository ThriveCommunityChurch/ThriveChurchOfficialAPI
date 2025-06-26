namespace ThriveChurchOfficialAPI.Core.Constants
{
    /// <summary>
    /// Standardized authentication error messages that are safe to return to users
    /// These messages are intentionally generic and the same for all error cases within each operation
    /// to prevent information disclosure attacks
    /// </summary>
    public static class AuthenticationMessages
    {
        /// <summary>
        /// Generic message for ALL login failures (wrong password, user not found, account locked, etc.)
        /// </summary>
        public const string LoginFailed = "Invalid username or password";

        /// <summary>
        /// Generic message for ALL refresh token failures (invalid token, expired, revoked, etc.)
        /// </summary>
        public const string RefreshTokenFailed = "Invalid or expired refresh token";

        /// <summary>
        /// Generic message for ALL unlock operation failures
        /// </summary>
        public const string UnlockFailed = "Unable to process unlock request";

        /// <summary>
        /// Generic message for ALL validation failures
        /// </summary>
        public const string InvalidRequest = "Invalid request";

        /// <summary>
        /// Success messages (these are safe to be specific)
        /// </summary>
        public const string LoginSuccessful = "Login successful";
        public const string TokenRefreshSuccessful = "Token refreshed successfully";
        public const string AccountUnlocked = "Account unlocked successfully";
    }
}
