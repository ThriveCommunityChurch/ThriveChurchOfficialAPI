namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response for a scripture reference in a study guide
    /// </summary>
    public class ScriptureReferenceResponse
    {
        /// <summary>
        /// The scripture reference (e.g., "Galatians 4:1-7")
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Context explaining how this scripture relates to the sermon
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// Whether the speaker directly quoted/read this passage
        /// </summary>
        public bool DirectlyQuoted { get; set; }
    }
}

