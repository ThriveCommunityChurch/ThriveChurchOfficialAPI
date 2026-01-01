using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    public class IllustrationBlob
    {
        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("point")]
        public string Point { get; set; }
    }
}
