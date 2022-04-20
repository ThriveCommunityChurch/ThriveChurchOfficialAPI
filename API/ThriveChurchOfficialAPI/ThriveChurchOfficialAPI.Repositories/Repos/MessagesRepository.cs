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
        /// <param name="messages"></param>
        /// <returns></returns>
        public async Task<SystemResponse<IEnumerable<SermonMessage>>> CreateNewMessages(IEnumerable<SermonMessage> messages)
        {
            // updated time is now
            var now = DateTime.UtcNow;

            foreach (var message in messages)
            {
                message.LastUpdated = now;
                message.CreateDate = now;
            }

            await _messagesCollection.InsertManyAsync(messages);

            return new SystemResponse<IEnumerable<SermonMessage>>(messages, "Success!");
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
        /// Gets a collection of messages using their series Id references
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SermonMessage>> GetMessageBySeriesId(string seriesId)
        {
            if (!IsValidObjectId(seriesId))
            {
                return null;
            }

            var filter = Builders<SermonMessage>.Filter.Eq(i => i.SeriesId, seriesId);

            var cursor = await _messagesCollection.FindAsync(filter);

            return cursor.ToList();
        }

        /// <summary>
        /// Gets a sermon message for its Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<SermonMessage> UpdateMessagePlayCount(string messageId)
        {
            if (!IsValidObjectId(messageId))
            {
                return null;
            }

            // use a filter since we are looking for an Id which is a value in an array with n elements
            var filter = Builders<SermonMessage>.Filter.Eq(x => x.Id, messageId);
            var update = Builders<SermonMessage>.Update.Inc(x => x.PlayCount, 1);

            var messageResponse = await _messagesCollection.FindOneAndUpdateAsync(filter, update, 
                new FindOneAndUpdateOptions<SermonMessage> 
                { 
                    // return the object after the update
                    ReturnDocument = ReturnDocument.After 
                });

            return messageResponse;
        }

        /// <summary>
        /// Updates a message using its unique Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<SermonMessage> UpdateMessageById(string messageId, SermonMessage message)
        {
            if (!IsValidObjectId(messageId))
            {
                return null;
            }

            // use a filter since we are looking for an Id which is a value in an array with n elements
            var filter = Builders<SermonMessage>.Filter.Eq(x => x.Id, messageId);

            var messageResponse = await _messagesCollection.FindOneAndReplaceAsync(filter, message,
                new FindOneAndReplaceOptions<SermonMessage>
                {
                    // return the object after the update
                    ReturnDocument = ReturnDocument.After
                });

            return messageResponse;
        }

        /// <summary>
        /// Get all messages
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SermonMessage>> GetAllMessages()
        {
            var filter = Builders<SermonMessage>.Filter.Empty;

            var cursor = await _messagesCollection.FindAsync(filter);

            return cursor.ToList();
        }
    }
}