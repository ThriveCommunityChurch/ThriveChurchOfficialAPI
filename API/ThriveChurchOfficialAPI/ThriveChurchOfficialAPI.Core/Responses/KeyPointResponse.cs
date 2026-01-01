namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response for a key point from sermon notes or study guide
    /// </summary>
    public class KeyPointResponse
    {
        /// <summary>
        /// The main point text
        /// </summary>
        public string Point { get; set; }

        /// <summary>
        /// Scripture reference for this point (optional)
        /// </summary>
        public string Scripture { get; set; }

        /// <summary>
        /// Additional detail or explanation (optional)
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// Theological context for study guides (optional)
        /// </summary>
        public string TheologicalContext { get; set; }

        /// <summary>
        /// Whether the scripture was directly quoted in the sermon (optional)
        /// </summary>
        public bool? DirectlyQuoted { get; set; }
    }
}

