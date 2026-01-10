namespace ThriveChurchOfficialAPI.Repositories
{
    public static class IndexKeys
    {
        // Configs Collection
        public const string ConfigsByKeyAsc_Unique = "Configs_Keys_1";

        // Users Collection
        public const string UsersByUsernameAsc_Unique = "Users_Username_1";
        public const string UsersByEmailAsc_Unique = "Users_Email_1";

        
        // RefreshTokens Collection
        public const string RefreshTokensByTokenAsc_Unique = "RefreshTokens_Token_1";
        public const string RefreshTokensByUserIdAsc = "RefreshTokens_UserId_1";
        public const string RefreshTokensByExpiresAtTTL = "RefreshTokens_ExpiresAt_TTL";
        public const string RefreshTokensByActiveAsc = "RefreshTokens_IsUsed_1_IsRevoked_1_ExpiresAt_1";

        // Events Collection
        public const string EventsByStartTimeAsc = "Events_StartTime_1";
        public const string EventsByIsActiveAsc = "Events_IsActive_1";
        public const string EventsByIsFeaturedAsc = "Events_IsFeatured_1";
        public const string EventsByIsActiveStartTimeAsc = "Events_IsActive_1_StartTime_1";
    }
}
