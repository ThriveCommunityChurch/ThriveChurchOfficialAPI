using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response for discussion questions in a study guide
    /// </summary>
    public class DiscussionQuestionsResponse
    {
        /// <summary>
        /// Icebreaker questions to start the discussion (1-2 questions)
        /// </summary>
        public List<string> Icebreaker { get; set; }

        /// <summary>
        /// Reflection questions about the sermon content (2-3 questions)
        /// </summary>
        public List<string> Reflection { get; set; }

        /// <summary>
        /// Application questions about putting the message into practice (2-3 questions)
        /// </summary>
        public List<string> Application { get; set; }

        /// <summary>
        /// Optional questions specifically for small group leaders
        /// </summary>
        public List<string> ForLeaders { get; set; }
    }
}

