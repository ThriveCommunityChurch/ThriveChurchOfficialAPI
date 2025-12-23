using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Represents a podcast episode message stored in the PodcastMessages collection
    /// </summary>
    public class PodcastMessage
    {
        /// <summary>
        /// MongoDB Object Id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        /// <summary>
        /// The unique identifier for the message
        /// </summary>
        [BsonElement("messageId")]
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; }

        /// <summary>
        /// The title of the podcast episode
        /// </summary>
        [BsonElement("title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// The full description of the podcast episode
        /// </summary>
        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// The URL to the audio file in S3
        /// </summary>
        [BsonElement("audioUrl")]
        [JsonPropertyName("audioUrl")]
        public string AudioUrl { get; set; }

        /// <summary>
        /// The size of the audio file in megabytes
        /// </summary>
        [BsonElement("audioFileSize")]
        [JsonPropertyName("audioFileSize")]
        public double AudioFileSize { get; set; }

        /// <summary>
        /// The duration of the audio file in seconds
        /// </summary>
        [BsonElement("audioDuration")]
        [JsonPropertyName("audioDuration")]
        public double AudioDuration { get; set; }

        /// <summary>
        /// The publish date of the podcast episode
        /// </summary>
        [BsonElement("pubDate")]
        [JsonPropertyName("pubDate")]
        public DateTime PubDate { get; set; }

        /// <summary>
        /// The speaker/host of the podcast episode
        /// </summary>
        [BsonElement("speaker")]
        [JsonPropertyName("speaker")]
        public string Speaker { get; set; }

        /// <summary>
        /// The unique GUID for the RSS feed item
        /// </summary>
        [BsonElement("guid")]
        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        /// <summary>
        /// The timestamp when this record was created
        /// </summary>
        [BsonElement("createdAt")]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The formatted title for podcast RSS feeds
        /// Format: "Series Name – Week # | Episode Title"
        /// </summary>
        [BsonElement("podcastTitle")]
        [JsonPropertyName("podcastTitle")]
        public string PodcastTitle { get; set; }

        /// <summary>
        /// The URL for the square podcast artwork image for this episode
        /// </summary>
        [BsonElement("artworkUrl")]
        [JsonPropertyName("artworkUrl")]
        public string ArtworkUrl { get; set; }
    }
}
