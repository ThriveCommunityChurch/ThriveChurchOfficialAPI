using System.Collections.Generic;
using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonNotesBlob
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

        [JsonProperty("quotes")]
        public List<QuoteBlob> Quotes { get; set; }

        [JsonProperty("applicationPoints")]
        public List<string> ApplicationPoints { get; set; }

        [JsonProperty("generatedAt")]
        public string GeneratedAt { get; set; }

        [JsonProperty("modelUsed")]
        public string ModelUsed { get; set; }

        [JsonProperty("wordCount")]
        public int WordCount { get; set; }
    }
}
