using Newtonsoft.Json;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// DTO for deserializing transcript JSON from Azure Blob Storage.
    /// Maps the camelCase blob schema to C# properties.
    /// This is an internal blob format - use TranscriptResponse for API responses.
    /// </summary>
    public class TranscriptBlob
    {
        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("speaker")]
        public string Speaker { get; set; }

        [JsonProperty("transcript")]
        public string Transcript { get; set; }

        [JsonProperty("wordCount")]
        public int WordCount { get; set; }

        [JsonProperty("uploadedAt")]
        public string UploadedAt { get; set; }
    }
}

