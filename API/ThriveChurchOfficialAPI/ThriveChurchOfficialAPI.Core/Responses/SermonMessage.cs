using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class SermonMessage : ObjectBase
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
            Summary = null;
            Date = null;
            PlayCount = 0;
            LastUpdated = DateTime.UtcNow;
            Tags = new List<MessageTag>();
        }

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
        /// A brief text summary/description of the sermon message
        /// </summary>
        [DataType(DataType.Text)]
        public string Summary { get; set; }

        /// <summary>
        /// The date that this message was given - we will ignore the time
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Timestamp for the last time this objct was updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// The number of times that this message has been played.
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// The unique identifier of the series that this message is part of
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string SeriesId { get; set; }

        /// <summary>
        /// A collection of tags categorizing this message by topic/theme
        /// </summary>
        public List<MessageTag> Tags { get; set; }

        /// <summary>
        /// Convert a collection of DB objects into the API response class
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static IEnumerable<SermonMessageResponse> ConvertToResponseList(IEnumerable<SermonMessage> messages)
        {
            var messagesList = new List<SermonMessageResponse>();

            if (messages == null || !messages.Any())
            {
                return messagesList;
            }

            foreach (var message in messages)
            {
                messagesList.Add(new SermonMessageResponse
                {
                    AudioDuration = message.AudioDuration,
                    AudioFileSize = message.AudioFileSize,
                    AudioUrl = message.AudioUrl,
                    Date = message.Date,
                    MessageId = message.Id,
                    SeriesId = message.SeriesId,
                    PassageRef = message.PassageRef,
                    PlayCount = message.PlayCount,
                    Speaker = message.Speaker,
                    Title = message.Title,
                    Summary = message.Summary,
                    VideoUrl = message.VideoUrl,
                    Tags = message.Tags
                });
            }

            return messagesList;
        }
    }
}