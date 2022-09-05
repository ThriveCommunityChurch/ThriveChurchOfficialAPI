using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace ThriveAPIMongoUpgrade.Objects
{
    internal class SermonSeries: ObjectBase
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Slug { get; set; }

        public string Thumbnail { get; set; }

        public string ArtUrl { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}