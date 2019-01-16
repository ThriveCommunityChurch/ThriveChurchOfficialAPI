using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core.DTOs
{
    public class RecentlyPlayedMessages
    {
        public RecentlyPlayedMessages()
        {
            UserId = null;
            RecentMessages = null;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// String representation for a User's Guid 
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// A collection of message ids in irder of most recently played, limited to a length of 3
        /// </summary>
        public IEnumerable<RecentMessage> RecentMessages { get; set; }
    }
}
