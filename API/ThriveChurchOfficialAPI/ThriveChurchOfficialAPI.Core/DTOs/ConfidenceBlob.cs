using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    public class ConfidenceBlob
    {
        [JsonProperty("scriptureAccuracy")]
        public string ScriptureAccuracy { get; set; }

        [JsonProperty("contentCoverage")]
        public string ContentCoverage { get; set; }
    }
}
