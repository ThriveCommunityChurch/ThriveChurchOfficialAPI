using System.Collections.Generic;
using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    public class AdditionalStudyBlob
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("scriptures")]
        public List<string> Scriptures { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }
    }
}
