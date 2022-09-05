using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace ThriveAPIMongoUpgrade.Objects
{
    internal class ObjectBase
    {
        /// <summary>
        /// Object Id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Timestamp for when this object was created (UTC)
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}