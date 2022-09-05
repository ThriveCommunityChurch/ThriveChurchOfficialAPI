using System;

namespace ThriveAPIMongoUpgrade.Objects
{
    internal class LegacySermonMessage
    {
        public string AudioUrl { get; set; }

        public double? AudioDuration { get; set; }

        public double? AudioFileSize { get; set; }

        public string VideoUrl { get; set; }

        public string PassageRef { get; set; }

        public string Speaker { get; set; }

        public string Title { get; set; }

        public DateTime? Date { get; set; }

        public string MessageId { get; set; }
    }
}