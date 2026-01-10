using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response containing study guide data for small group discussion
    /// </summary>
    public class StudyGuideResponse
    {
        /// <summary>
        /// The title of the sermon message
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The speaker's name
        /// </summary>
        public string Speaker { get; set; }

        /// <summary>
        /// The date of the sermon
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// The main scripture passage for the sermon
        /// </summary>
        public string MainScripture { get; set; }

        /// <summary>
        /// A 4-6 sentence overview for someone who missed Sunday
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Key points with theological context
        /// </summary>
        public List<KeyPointResponse> KeyPoints { get; set; }

        /// <summary>
        /// Scripture references used in the sermon
        /// </summary>
        public List<ScriptureReferenceResponse> ScriptureReferences { get; set; }

        /// <summary>
        /// Discussion questions organized by type
        /// </summary>
        public DiscussionQuestionsResponse DiscussionQuestions { get; set; }

        /// <summary>
        /// Illustrations and stories from the sermon
        /// </summary>
        public List<IllustrationResponse> Illustrations { get; set; }

        /// <summary>
        /// Prayer prompts based on sermon themes
        /// </summary>
        public List<string> PrayerPrompts { get; set; }

        /// <summary>
        /// Concrete challenges for the week
        /// </summary>
        public List<string> TakeHomeChallenges { get; set; }

        /// <summary>
        /// A short devotional reflection for personal or group use
        /// </summary>
        public string Devotional { get; set; }

        /// <summary>
        /// Additional topics for further study
        /// </summary>
        public List<AdditionalStudyResponse> AdditionalStudy { get; set; }

        /// <summary>
        /// Estimated time needed for the study (e.g., "30-45 minutes")
        /// </summary>
        public string EstimatedStudyTime { get; set; }

        /// <summary>
        /// ISO timestamp when the study guide was generated
        /// </summary>
        public string GeneratedAt { get; set; }

        /// <summary>
        /// The AI model used to generate the study guide
        /// </summary>
        public string ModelUsed { get; set; }

        /// <summary>
        /// Confidence assessment of the generated content
        /// </summary>
        public ConfidenceResponse Confidence { get; set; }
    }
}

