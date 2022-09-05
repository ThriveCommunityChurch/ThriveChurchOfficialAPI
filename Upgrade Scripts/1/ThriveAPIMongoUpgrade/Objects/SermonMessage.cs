using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace ThriveAPIMongoUpgrade.Objects
{
    internal class SermonMessage : ObjectBase
    {
        public string AudioUrl { get; set; }

        public double? AudioDuration { get; set; }

        public double? AudioFileSize { get; set; }

        public string VideoUrl { get; set; }

        public string PassageRef { get; set; }

        public string Speaker { get; set; }

        public string Title { get; set; }

        public DateTime? Date { get; set; }

        public DateTime LastUpdated { get; set; }

        public int PlayCount { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SeriesId { get; set; }
    }
}