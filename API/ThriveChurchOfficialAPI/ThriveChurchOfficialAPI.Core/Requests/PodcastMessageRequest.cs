using System;
using System.ComponentModel.DataAnnotations;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Request to update a podcast message
    /// </summary>
    public class PodcastMessageRequest
    {
        /// <summary>
        /// The title of the podcast episode
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The full description of the podcast episode
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The URL to the audio file in S3
        /// </summary>
        [Required(ErrorMessage = "No value given for property 'AudioUrl'. This property is required.")]
        [Url(ErrorMessage = "'AudioUrl' must be in valid url syntax.")]
        [DataType(DataType.Url)]
        public string AudioUrl { get; set; }

        /// <summary>
        /// The size of the audio file in megabytes
        /// </summary>
        public double AudioFileSize { get; set; }

        /// <summary>
        /// The duration of the audio file in seconds
        /// </summary>
        public double AudioDuration { get; set; }

        /// <summary>
        /// The publish date of the podcast episode
        /// </summary>
        public DateTime PubDate { get; set; }

        /// <summary>
        /// The speaker/host of the podcast episode
        /// </summary>
        [Required(ErrorMessage = "No value given for property 'Speaker'. This property is required.")]
        [DataType(DataType.Text)]
        public string Speaker { get; set; }

        /// <summary>
        /// The timestamp when this record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The formatted title for podcast RSS feeds
        /// Format: "Series Name – Week # | Episode Title"
        /// </summary>
        public string PodcastTitle { get; set; }

        /// <summary>
        /// The URL for the square podcast artwork image for this episode
        /// </summary>
        [Url(ErrorMessage = "'ArtworkUrl' must be in valid url syntax.")]
        [DataType(DataType.Url)]
        public string ArtworkUrl { get; set; }
    }
}
