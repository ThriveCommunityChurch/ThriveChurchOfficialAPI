using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class SermonMessage
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public SermonMessage()
        {
            AudioUrl = null;
            AudioDuration = null;
            VideoUrl = null;
            PassageRef = null;
            Speaker = null;
            Title = null;
            Date = null;
            PlayCount = 0;
        }

        /// <summary>
        /// The unique ID of the message
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// The full Url for the .mp3 for the sermon recording.
        /// If null then this may not have been recorded
        /// </summary>
        [Url(ErrorMessage = "'AudioUrl' must be in valid url syntax.")]
        [DataType(DataType.Url)]
        public string AudioUrl { get; set; }

        /// <summary>
        /// A numeric value representing the number of seconds of the message audio file
        /// </summary>
        public double? AudioDuration { get; set; }

        /// <summary>
        /// The size of the audio file in megabytes
        /// </summary>
        public double? AudioFileSize { get; set; }

        /// <summary>
        /// The full Url for the youtube video for the sermon recording.
        /// If null then this may not have been recorded
        /// </summary>
        [Url(ErrorMessage = "'VideoUrl' must be in valid url syntax.")]
        [DataType(DataType.Url)]
        public string VideoUrl { get; set; }

        /// <summary>
        /// The passage being referenced in this message
        /// </summary>
        public string PassageRef { get; set; }

        /// <summary>
        /// The individual giving this message
        /// </summary>
        [Required(ErrorMessage = "No value given for property 'Speaker'. This property is required.")]
        [DataType(DataType.Text)]
        public string Speaker { get; set; }

        /// <summary>
        /// The title of the message. If null follow this pattern
        /// {Series Name} - Week {#}
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'Title'. This property is required.")]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        /// <summary>
        /// The date that this message was given - we will ignore the time
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// The number of times that this message has been played.
        /// </summary>
        public int PlayCount { get; set; }
    }
}