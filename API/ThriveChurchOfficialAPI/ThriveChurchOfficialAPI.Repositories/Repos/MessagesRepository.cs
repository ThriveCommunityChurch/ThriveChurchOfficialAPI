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
    public class MessagesRepository : RepositoryBase, IMessagesRepository
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
        public async Task<SystemResponse<SermonSeries>> CreateNewSermonSeries(SermonSeries request)
        {
            // updated time is now
            request.LastUpdated = DateTime.UtcNow;

            await _sermonsCollection.InsertOneAsync(request);

            // respond with the inserted object
            var inserted = await _sermonsCollection.FindAsync(
                    Builders<SermonSeries>.Filter.Eq(l => l.Slug, request.Slug));

            var response = inserted.FirstOrDefault();
            if (response == default(SermonSeries))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.ErrorOcurredInsertingIntoCollection, "Sermons"));
            }

            return new SystemResponse<SermonSeries>(response, "Success!");
        }

        /// <summary>
        /// Gets a series object for the specified Id
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeries>> GetSermonSeriesForId(string SeriesId)
        {
            if (!IsValidObjectId(SeriesId))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "Sermon Series", SeriesId));
            }

            var singleSeries = await _sermonsCollection.FindAsync(
                   Builders<SermonSeries>.Filter.Eq(s => s.Id, SeriesId));

            var response = singleSeries.FirstOrDefault();

            return new SystemResponse<SermonSeries>(response, "Success!");
        }

        /// <summary>
        /// Used to find a series for a particular unique slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeries>> GetSermonSeriesForSlug(string slug)
        {
            var singleSeries = await _sermonsCollection.FindAsync(
                   Builders<SermonSeries>.Filter.Eq(s => s.Slug, slug));

            var response = singleSeries.FirstOrDefault();
            if (response == default(SermonSeries))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.UnableToFindSermonWithSlug, slug));
            }

            return new SystemResponse<SermonSeries>(response, "Success!");
        }

        /// <summary>
        /// Gets a sermon message for its Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<SermonMessage> GetMessageForId(string messageId)
        {
            // use a filter since we are looking for an Id which is a value in an array with n elements
            var filter = Builders<SermonSeries>.Filter.ElemMatch(x => x.Messages, x => x.MessageId == messageId);

            var seriesResponse = await _sermonsCollection.FindAsync(filter);
            var series = seriesResponse.FirstOrDefault();
            var response = series.Messages.Where(i => i.MessageId == messageId).FirstOrDefault();

            return response;
        }

        /// <summary>
        /// Gets a sermon message for its Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<SermonMessage> UpdateMessagePlayCount(string messageId)
        {
            // use a filter since we are looking for an Id which is a value in an array with n elements
            var filter = Builders<SermonSeries>.Filter.ElemMatch(x => x.Messages, x => x.MessageId == messageId);

            var seriesResponse = await _sermonsCollection.FindAsync(filter);
            var series = seriesResponse.FirstOrDefault();
            var message = series.Messages.Where(i => i.MessageId == messageId).FirstOrDefault();
            message.PlayCount++;

            return response;
        }
    }
}