namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response for confidence assessment of generated content
    /// </summary>
    public class ConfidenceResponse
    {
        /// <summary>
        /// Confidence level for scripture accuracy ("high" or "medium")
        /// </summary>
        public string ScriptureAccuracy { get; set; }

        /// <summary>
        /// Confidence level for content coverage ("high" or "medium")
        /// </summary>
        public string ContentCoverage { get; set; }
    }
}

