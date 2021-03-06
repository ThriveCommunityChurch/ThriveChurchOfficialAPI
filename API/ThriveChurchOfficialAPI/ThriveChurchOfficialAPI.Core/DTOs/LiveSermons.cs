using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class LiveSermons
    {
        public LiveSermons()
        {
            IsLive = false;
            SpecialEventTimes = null;
        }

        /// <summary>
        /// ObjectId notation from Mongo
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Flag determining whether or not a stream is currently active
        /// </summary>
        public bool IsLive { get; set; }

        /// <summary>
        /// A UTC timestamp for when the livestream will be live again
        /// </summary>
        public DateTime? NextLive { get; set; }

        /// <summary>
        /// Set a time when a livestream notification will disappear,
        /// after this time when a user loads the page the notification will disappear.
        /// We will only use the time here.
        /// NOTE: Stored as UTC.
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// Timestamp for the last time this objct was updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// The start and end times of a special event that is to be broadcast
        /// if null => then this is not a special event
        /// </summary>
        public DateRange SpecialEventTimes { get; set; }
    }
}