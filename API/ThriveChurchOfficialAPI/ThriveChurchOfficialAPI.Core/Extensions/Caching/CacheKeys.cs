namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Cache key templates using Redis-style hierarchical naming.
    /// Format: thrive:{domain}:{sub}:{identifier}
    /// </summary>
    public static class CacheKeys
    {
        // ============================================
        // Sermon Cache Keys
        // ============================================
        
        /// <summary>
        /// All sermons summary. Format: thrive:sermons:summary:{highRes}
        /// </summary>
        public const string SermonsSummary = "thrive:sermons:summary:{0}";

        /// <summary>
        /// Paged sermons. Format: thrive:sermons:paged:{pageNumber}
        /// </summary>
        public const string SermonsPage = "thrive:sermons:paged:{0}";

        /// <summary>
        /// Individual sermon series. Format: thrive:sermons:series:{seriesId}
        /// </summary>
        public const string SermonSeries = "thrive:sermons:series:{0}";

        /// <summary>
        /// Pattern for invalidating all sermon caches
        /// </summary>
        public const string SermonsPattern = "thrive:sermons:*";

        // ============================================
        // Configuration Cache Keys
        // ============================================

        /// <summary>
        /// System configuration value. Format: thrive:config:{key}
        /// </summary>
        public const string Config = "thrive:config:{0}";

        /// <summary>
        /// Pattern for invalidating all config caches
        /// </summary>
        public const string ConfigPattern = "thrive:config:*";

        // ============================================
        // Event Cache Keys
        // ============================================

        /// <summary>
        /// All events list. Format: thrive:events:all:{includeInactive}
        /// </summary>
        public const string EventsAll = "thrive:events:all:{0}";

        /// <summary>
        /// Individual event. Format: thrive:events:item:{eventId}
        /// </summary>
        public const string EventItem = "thrive:events:item:{0}";

        /// <summary>
        /// Featured events list
        /// </summary>
        public const string EventsFeatured = "thrive:events:featured";

        /// <summary>
        /// Pattern for invalidating all event caches
        /// </summary>
        public const string EventsPattern = "thrive:events:*";

        // ============================================
        // Transcript Cache Keys
        // ============================================

        /// <summary>
        /// Sermon transcript blob. Format: thrive:transcripts:blob:{messageId}
        /// </summary>
        public const string TranscriptBlob = "thrive:transcripts:blob:{0}";

        /// <summary>
        /// Pattern for invalidating all transcript caches
        /// </summary>
        public const string TranscriptsPattern = "thrive:transcripts:*";

        // ============================================
        // Bible Passage Cache Keys
        // ============================================

        /// <summary>
        /// Cached Bible passage from ESV API. Format: thrive:passages:{reference}
        /// </summary>
        public const string Passage = "thrive:passages:{0}";

        /// <summary>
        /// Pattern for invalidating all passage caches
        /// </summary>
        public const string PassagesPattern = "thrive:passages:*";

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Format a cache key template with a single value (lowercased for consistency)
        /// </summary>
        public static string Format(string template, object value)
        {
            return string.Format(template, value?.ToString()?.ToLowerInvariant());
        }

        /// <summary>
        /// Format a cache key template with multiple values (lowercased for consistency)
        /// </summary>
        public static string Format(string template, params object[] values)
        {
            var lowercasedValues = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                lowercasedValues[i] = values[i]?.ToString()?.ToLowerInvariant();
            }
            return string.Format(template, lowercasedValues);
        }
    }
}

