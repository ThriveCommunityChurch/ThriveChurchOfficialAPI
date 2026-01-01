using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// DTO for deserializing key point data from sermon notes/study guide blob.
    /// Maps the camelCase blob schema to C# properties.
    /// </summary>
    public class KeyPointBlob
    {
        [JsonProperty("point")]
        public string Point { get; set; }

        [JsonProperty("scripture")]
        public string Scripture { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("theologicalContext")]
        public string TheologicalContext { get; set; }

        [JsonProperty("directlyQuoted")]
        public bool? DirectlyQuoted { get; set; }
    }
}
