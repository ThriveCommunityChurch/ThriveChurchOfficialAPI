using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    public class ScriptureReferenceBlob
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("context")]
        public string Context { get; set; }

        [JsonProperty("directlyQuoted")]
        public bool DirectlyQuoted { get; set; }
    }
}
