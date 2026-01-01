using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response for additional study topics
    /// </summary>
    public class AdditionalStudyResponse
    {
        /// <summary>
        /// The topic for additional study
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Scripture references related to this topic
        /// </summary>
        public List<string> Scriptures { get; set; }

        /// <summary>
        /// Note about this additional study topic
        /// </summary>
        public string Note { get; set; }
    }
}

