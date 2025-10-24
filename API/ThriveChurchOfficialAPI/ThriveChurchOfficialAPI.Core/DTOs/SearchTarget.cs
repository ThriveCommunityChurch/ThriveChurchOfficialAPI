namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Specifies the target entity type for searches
    /// </summary>
    public enum SearchTarget
    {
        /// <summary>
        /// Search for individual sermon messages by tags
        /// </summary>
        Messages = 0,

        /// <summary>
        /// Search for sermon series by tags
        /// </summary>
        Series = 1,

        /// <summary>
        /// Search for messages by speaker name
        /// </summary>
        Speaker = 2
    }
}

