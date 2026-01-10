using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    public class QuoteBlob
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("context")]
        public string Context { get; set; }
    }
}
