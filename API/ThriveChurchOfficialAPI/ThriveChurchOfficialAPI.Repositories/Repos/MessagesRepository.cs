using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using System.Linq;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Sermons Repo
    /// </summary>
    public class MessagesRepository : RepositoryBase<SermonMessage>, IMessagesRepository
    {
        private readonly IMongoCollection<SermonMessage> _messagesCollection;

        /// <summary>
        /// Sermons Repo C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public MessagesRepository(IConfiguration Configuration)
            : base(Configuration)
        {
            _messagesCollection = DB.GetCollection<SermonMessage>("Messages");
        }

        /// <summary>
        /// Adds a new SermonSeries to the Sermons Collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonMessage>> CreateNewMessage(SermonMessage request)
        {
            // updated time is now
            var now = DateTime.UtcNow;
            request.LastUpdated = now;
            request.CreateDate = now;

            await _messagesCollection.InsertOneAsync(request);

            return new SystemResponse<SermonMessage>(request, "Success!");
        }

        /// <summary>
        /// Gets a series object for the specified Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonMessage>> GetMessageById(string messageId)
        {
            if (!IsValidObjectId(messageId))
            {
                return new SystemResponse<SermonMessage>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "message", messageId));
            }

            var filter = Builders<SermonMessage>.Filter.Eq(s => s.Id, messageId);

            var cursor = await _messagesCollection.FindAsync(filter);

            SermonMessage response = cursor.FirstOrDefault();
            if (response == default)
            {
                return new SystemResponse<SermonMessage>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "message", messageId));
            }

            return new SystemResponse<SermonMessage>(response, "Success!");
        }

        /// <summary>
        /// Gets a sermon message for its Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<SermonMessage> UpdateMessagePlayCount(string messageId)
        {
            // use a filter since we are looking for an Id which is a value in an array with n elements
            var filter = Builders<SermonMessage>.Filter.ElemMatch(x => x.Id, messageId);
            var update = Builders<SermonMessage>.Update.Inc(x => x.PlayCount, 1);

            var messageResponse = await _messagesCollection.FindOneAndUpdateAsync(filter, update, 
                new FindOneAndUpdateOptions<SermonMessage> 
                { 
                    // return the object after the update
                    ReturnDocument = ReturnDocument.After 
                });

            return messageResponse;
        }
    }
}