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
        /// Gets a message using the specified (dual inclusive) date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SermonMessage>> GetMessageByDateRange(DateTime? startDate, DateTime? endDate)
        {
            var filter = Builders<SermonMessage>.Filter.Empty;

            if (startDate.HasValue && endDate.HasValue)
            {
                filter &= Builders<SermonMessage>.Filter.Gte(s => s.Date, startDate);
                filter &= Builders<SermonMessage>.Filter.Lte(s => s.Date, endDate);
            }

            var cursor = await _messagesCollection.FindAsync(filter);

            return cursor.ToList();
        }

        /// <summary>
        /// Gets a collection of messages using their series Id references
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SermonMessage>> GetMessagesBySeriesId(string seriesId)
        {
            if (!IsValidObjectId(seriesId))
            {
                return null;
            }

            var filter = Builders<SermonMessage>.Filter.Eq(i => i.SeriesId, seriesId);

            // we always want them in the order of most recent first
            var stages = new List<BsonDocument>
            {
                new BsonDocument("$match", ConvertFilterToBsonDocument(filter)),
                new BsonDocument("$sort", new BsonDocument(nameof(SermonMessage.Date), -1))
            };

            PipelineDefinition<SermonMessage, SermonMessage> pipeline = PipelineDefinition<SermonMessage, SermonMessage>.Create(stages);

            var cursor = await _messagesCollection.AggregateAsync(pipeline);

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
        public async Task<SermonMessage> UpdateMessageById(string messageId, SermonMessageRequest message)
        {
            if (!IsValidObjectId(messageId))
            {
                return null;
            }

            // use a filter since we are looking for an Id which is a value in an array with n elements
            var filter = Builders<SermonMessage>.Filter.Eq(x => x.Id, messageId);
            var update = Builders<SermonMessage>.Update.Set(x => x.LastUpdated, DateTime.UtcNow)
                                                       .Set(x => x.AudioDuration, message.AudioDuration)
                                                       .Set(x => x.AudioFileSize, message.AudioFileSize)
                                                       .Set(x => x.AudioUrl, message.AudioUrl)
                                                       .Set(x => x.Date, message.Date)
                                                       .Set(x => x.Speaker, message.Speaker)
                                                       .Set(x => x.Title, message.Title)
                                                       .Set(x => x.Summary, message.Summary)
                                                       .Set(x => x.VideoUrl, message.VideoUrl)
                                                       .Set(x => x.PassageRef, message.PassageRef)
                                                       .Set(x => x.Tags, message.Tags);

            var messageResponse = await _messagesCollection.FindOneAndUpdateAsync(filter, update,
                new FindOneAndUpdateOptions<SermonMessage>
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