using System.Collections.Generic;
using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    public class DiscussionQuestionsBlob
    {
        [JsonProperty("icebreaker")]
        public List<string> Icebreaker { get; set; }

        [JsonProperty("reflection")]
        public List<string> Reflection { get; set; }

        [JsonProperty("application")]
        public List<string> Application { get; set; }

        [JsonProperty("forLeaders")]
        public List<string> ForLeaders { get; set; }
    }
}
