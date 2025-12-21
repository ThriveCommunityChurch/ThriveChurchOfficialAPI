using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

using SortDirection = ThriveChurchOfficialAPI.Core.SortDirection;

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
                                                       .Set(x => x.Tags, message.Tags)
                                                       .Set(x => x.WaveformData, message.WaveformData)
                                                       .Set(x => x.PodcastImageUrl, message.PodcastImageUrl)
                                                       .Set(x => x.PodcastTitle, message.PodcastTitle);

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

        /// <summary>
        /// Search for messages that contain at least one of the specified tags
        /// </summary>
        /// <param name="tags">Tags to search for</param>
        /// <param name="sortDirection">Sort direction by date</param>
        /// <returns>Collection of matching messages as SermonMessageResponse</returns>
        public async Task<IEnumerable<SermonMessageResponse>> SearchMessagesByTags(IEnumerable<MessageTag> tags, SortDirection sortDirection)
        {
            // Convert tags to their integer values for MongoDB query
            var tagValues = tags.Select(t => (int)t).ToList();

            // Determine sort direction
            var sortOrder = sortDirection == SortDirection.Ascending ? 1 : -1;

            // Build aggregation pipeline - filter first for best performance
            var stages = new List<BsonDocument>
            {
                // Stage 1: Filter messages that have at least one matching tag
                new BsonDocument("$match", new BsonDocument
                {
                    { "Tags", new BsonDocument("$in", new BsonArray(tagValues)) }
                }),

                // Stage 2: Lookup the series for each matching message to get SeriesId
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Series" },
                    { "localField", "SeriesId" },
                    { "foreignField", "_id" },
                    { "as", "series" }
                }),

                // Stage 3: Unwind the series array (each message has one series)
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$series" },
                    { "preserveNullAndEmptyArrays", true }
                }),

                // Stage 4: Sort by Date
                new BsonDocument("$sort", new BsonDocument("Date", sortOrder)),

                // Stage 5: Project into SermonMessageResponse format
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "MessageId", new BsonDocument("$toString", "$_id" )},
                    { "SeriesId", new BsonDocument("$toString", "$series._id" )},
                    { "AudioUrl", 1 },
                    { "AudioDuration", 1 },
                    { "AudioFileSize", 1 },
                    { "VideoUrl", 1 },
                    { "PassageRef", 1 },
                    { "Speaker", 1 },
                    { "Title", 1 },
                    { "Summary", 1 },
                    { "Date", 1 },
                    { "PlayCount", 1 },
                    { "Tags", 1 }
                })
            };

            // Execute pipeline on Messages collection
            PipelineDefinition<SermonMessage, SermonMessageResponse> pipeline =
                PipelineDefinition<SermonMessage, SermonMessageResponse>.Create(stages);

            var cursor = await _messagesCollection.AggregateAsync(pipeline);

            return cursor.ToList();
        }

        /// <summary>
        /// Gets all unique speaker names from sermon messages
        /// </summary>
        /// <returns>Collection of unique speaker names</returns>
        public async Task<IEnumerable<string>> GetUniqueSpeakers()
        {
            // Use aggregation pipeline to get distinct speakers
            var stages = new List<BsonDocument>
            {
                // Stage 1: Group by Speaker to get unique values
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$Speaker" }
                }),

                // Stage 2: Sort alphabetically
                new BsonDocument("$sort", new BsonDocument("_id", 1)),

                // Stage 3: Project to return just the speaker name
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "speaker", "$_id" }
                })
            };

            PipelineDefinition<SermonMessage, BsonDocument> pipeline =
                PipelineDefinition<SermonMessage, BsonDocument>.Create(stages);

            var cursor = await _messagesCollection.AggregateAsync(pipeline);
            var results = await cursor.ToListAsync();

            // Extract speaker names from BsonDocuments
            return results.Select(doc => doc["speaker"].AsString).ToList();
        }

        /// <summary>
        /// Search for messages by speaker name
        /// </summary>
        /// <param name="speaker">Speaker name to search for (case-insensitive)</param>
        /// <param name="sortDirection">Sort direction by date</param>
        /// <returns>Collection of matching messages</returns>
        public async Task<IEnumerable<SermonMessage>> SearchMessagesBySpeaker(string speaker, SortDirection sortDirection)
        {
            // Case-insensitive exact match using regex
            var filter = Builders<SermonMessage>.Filter.Regex(
                m => m.Speaker,
                new BsonRegularExpression($"^{Regex.Escape(speaker.Trim())}$", "i"));

            IFindFluent<SermonMessage, SermonMessage> query = _messagesCollection.Find(filter);

            if (sortDirection == SortDirection.Ascending)
            {
                query = query.SortBy(m => m.Date);
            }
            else
            {
                query = query.SortByDescending(m => m.Date);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get the waveform data for a message
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns>Waveform Data</returns>
        public async Task<SystemResponse<List<double>>> GetMessageWaveformData(string messageId)
        {
            if (!IsValidObjectId(messageId))
            {
                return new SystemResponse<List<double>>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "message", messageId));
            }

            var filter = Builders<SermonMessage>.Filter.Eq(i => i.Id, messageId);

            var cursor = await _messagesCollection.FindAsync(filter);

            var document = cursor.FirstOrDefault();
            if (document == default)
            {
                return new SystemResponse<List<double>>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "message", messageId));
            }

            return new SystemResponse<List<double>>(document.WaveformData, "Success!");
        }
    }
}