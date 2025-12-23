using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Repository for PodcastMessages collection
    /// </summary>
    public class PodcastMessagesRepository : RepositoryBase<PodcastMessage>, IPodcastMessagesRepository
    {
        private readonly IMongoCollection<PodcastMessage> _podcastMessagesCollection;

        /// <summary>
        /// PodcastMessages Repository Constructor
        /// </summary>
        /// <param name="Configuration"></param>
        public PodcastMessagesRepository(IConfiguration Configuration)
            : base(Configuration)
        {
            _podcastMessagesCollection = DB.GetCollection<PodcastMessage>("PodcastEpisodes");
        }

        /// <summary>
        /// Gets all podcast messages from the PodcastMessages collection
        /// </summary>
        /// <returns>Collection of all podcast messages</returns>
        public async Task<List<PodcastMessage>> GetAllPodcastMessages()
        {
            var filter = Builders<PodcastMessage>.Filter.Empty;

            // Sort by pubDate descending (most recent first)
            var sort = Builders<PodcastMessage>.Sort.Descending(m => m.PubDate);

            var cursor = await _podcastMessagesCollection.Find(filter).Sort(sort).ToListAsync();
            return cursor;
        }

        /// <summary>
        /// Gets a podcast message by its unique Id
        /// </summary>
        /// <param name="messageId">The MongoDB ObjectId of the message</param>
        /// <returns>The podcast message if found, error response otherwise</returns>
        public async Task<SystemResponse<PodcastMessage>> GetPodcastMessageById(string messageId)
        {
            if (!IsValidObjectId(messageId))
            {
                return new SystemResponse<PodcastMessage>(true, 
                    string.Format(SystemMessages.UnableToFindPropertyForId, "podcast message", messageId));
            }

            var filter = Builders<PodcastMessage>.Filter.Eq(m => m.Id, messageId);

            var cursor = await _podcastMessagesCollection.FindAsync(filter);

            PodcastMessage response = cursor.FirstOrDefault();
            if (response == default)
            {
                return new SystemResponse<PodcastMessage>(true, 
                    string.Format(SystemMessages.UnableToFindPropertyForId, "podcast message", messageId));
            }

            return new SystemResponse<PodcastMessage>(response, "Success!");
        }

        /// <summary>
        /// Updates an existing podcast message by its Id
        /// </summary>
        /// <param name="messageId">The MongoDB ObjectId of the message to update</param>
        /// <param name="message">The updated podcast message data</param>
        /// <returns>The updated podcast message if successful, null otherwise</returns>
        public async Task<PodcastMessage> UpdatePodcastMessageById(string messageId, PodcastMessageRequest message)
        {
            if (!IsValidObjectId(messageId))
            {
                return null;
            }

            var filter = Builders<PodcastMessage>.Filter.Eq(m => m.Id, messageId);

            var update = Builders<PodcastMessage>.Update
                .Set(m => m.Title, message.Title)
                .Set(m => m.Description, message.Description)
                .Set(m => m.AudioUrl, message.AudioUrl)
                .Set(m => m.AudioFileSize, message.AudioFileSize)
                .Set(m => m.AudioDuration, message.AudioDuration)
                .Set(m => m.PubDate, message.PubDate)
                .Set(m => m.Speaker, message.Speaker)
                .Set(m => m.PodcastTitle, message.PodcastTitle)
                .Set(m => m.ArtworkUrl, message.ArtworkUrl);

            var updatedMessage = await _podcastMessagesCollection.FindOneAndUpdateAsync(
                filter, 
                update,
                new FindOneAndUpdateOptions<PodcastMessage>
                {
                    ReturnDocument = ReturnDocument.After
                });

            return updatedMessage;
        }
    }
}

