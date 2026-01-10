namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response for an illustration/story from the sermon
    /// </summary>
    public class IllustrationResponse
    {
        /// <summary>
        /// Brief summary of the illustration or story
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The point the illustration was making
        /// </summary>
        public string Point { get; set; }
    }
}

