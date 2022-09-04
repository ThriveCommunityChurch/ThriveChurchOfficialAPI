using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveAPIMongoUpgrade.Objects
{
    internal class LegacySermonSeries
    {
        /// <summary>
        /// ObjectId notation from Mongo
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Year { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Slug { get; set; }

        public string Thumbnail { get; set; }

        public string ArtUrl { get; set; }

        public DateTime LastUpdated { get; set; }

        public IEnumerable<LegacySermonMessage> Messages { get; set; }
    }
}