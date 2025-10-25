using System;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonMessageResponse
    {
        /// <summary>
        /// The full Url for the .mp3 for the sermon recording.
        /// If null then this may not have been recorded
        /// </summary>
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
        public string VideoUrl { get; set; }

        /// <summary>
        /// The passage being referenced in this message
        /// </summary>
        public string PassageRef { get; set; }

        /// <summary>
        /// The individual giving this message
        /// </summary>
        public string Speaker { get; set; }

        /// <summary>
        /// The title of the message. If null follow this pattern
        /// {Series Name} - Week {#}
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A brief text summary/description of the sermon message
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The date that this message was given - we will ignore the time
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// The number of times that this message has been played.
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// Unique identifier of the message
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Unique identifier of the series that this message is part of
        /// </summary>
        public string SeriesId { get; set; }

        /// <summary>
        /// A collection of tags categorizing this message by topic/theme
        /// </summary>
        public IEnumerable<MessageTag> Tags { get; set; }
    }
}