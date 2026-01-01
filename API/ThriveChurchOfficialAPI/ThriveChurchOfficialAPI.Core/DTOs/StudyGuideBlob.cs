using System.Collections.Generic;
using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    public class StudyGuideBlob
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("speaker")]
        public string Speaker { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("mainScripture")]
        public string MainScripture { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("keyPoints")]
        public List<KeyPointBlob> KeyPoints { get; set; }

        [JsonProperty("scriptureReferences")]
        public List<ScriptureReferenceBlob> ScriptureReferences { get; set; }

        [JsonProperty("discussionQuestions")]
        public DiscussionQuestionsBlob DiscussionQuestions { get; set; }

        [JsonProperty("illustrations")]
        public List<IllustrationBlob> Illustrations { get; set; }

        [JsonProperty("prayerPrompts")]
        public List<string> PrayerPrompts { get; set; }

        [JsonProperty("takeHomeChallenges")]
        public List<string> TakeHomeChallenges { get; set; }

        [JsonProperty("additionalStudy")]
        public List<AdditionalStudyBlob> AdditionalStudy { get; set; }

        [JsonProperty("estimatedStudyTime")]
        public string EstimatedStudyTime { get; set; }

        [JsonProperty("generatedAt")]
        public string GeneratedAt { get; set; }

        [JsonProperty("modelUsed")]
        public string ModelUsed { get; set; }

        [JsonProperty("confidence")]
        public ConfidenceBlob Confidence { get; set; }
    }
}
