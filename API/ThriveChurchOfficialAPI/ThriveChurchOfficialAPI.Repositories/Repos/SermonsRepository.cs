using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using System.Linq;
using System.Collections.Generic;
using ThriveChurchOfficialAPI.Core.DTOs;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class SermonsRepository : RepositoryBase, ISermonsRepository
    {
        private readonly string connectionString;

        public SermonsRepository(IConfiguration configuration)
        {
            connectionString = configuration["MongoConnectionString"];
        }

        /// <summary>
        /// Returns all Sermon Series' from MongoDB - including active sermon series'
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<AllSermonsResponse> GetAllSermons()
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");
            var documents = await collection.Find(_ => true).ToListAsync();

            var allSermonsResponse = new AllSermonsResponse
            {
                Sermons = documents.OrderBy(i => i.StartDate)
            };

            return allSermonsResponse;
        }

        /// <summary>
        /// Adds a new SermonSeries to the Sermons Collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SermonSeries> CreateNewSermonSeries(SermonSeries request)
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            // updated time is now
            request.LastUpdated = DateTime.UtcNow;

            await collection.InsertOneAsync(request);

            // respond with the inserted object
            var inserted = await collection.FindAsync(
                    Builders<SermonSeries>.Filter.Eq(l => l.Slug, request.Slug));

            var response = inserted.FirstOrDefault();

            if (response == default(SermonSeries))
            {
                return null;
            }

            return response;
        }

        /// <summary>
        /// Finds and replaces the old object for the replacement
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SermonSeries> UpdateSermonSeries(SermonSeries request)
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            // updated time is now
            request.LastUpdated = DateTime.UtcNow;

            var singleSeries = await collection.FindOneAndReplaceAsync(
                   Builders<SermonSeries>.Filter.Eq(s => s.Id, request.Id), request);

            // this does not return the updated object. 
            // TODO: fix
            return request;
        }

        /// <summary>
        /// Gets a series object for the specified Id
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <returns></returns>
        public async Task<SermonSeries> GetSermonSeriesForId(string SeriesId)
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            var singleSeries = await collection.FindAsync(
                   Builders<SermonSeries>.Filter.Eq(s => s.Id, SeriesId));

            var response = singleSeries.FirstOrDefault();

            if (response == default(SermonSeries))
            {
                return null;
            }

            return response;
        }

        /// <summary>
        /// Used to find a series for a particular unique slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<SermonSeries> GetSermonSeriesForSlug(string slug)
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            var singleSeries = await collection.FindAsync(
                   Builders<SermonSeries>.Filter.Eq(s => s.Slug, slug));

            var response = singleSeries.FirstOrDefault();

            if (response == default(SermonSeries))
            {
                return null;
            }

            return response;
        }

        /// <summary>
        /// Gets a sermon message for its Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<SermonMessage> GetMessageForId(string messageId)
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            // use a filter since we are looking for an Id which is a value in an array with n elements
            var filter = Builders<SermonSeries>.Filter.ElemMatch(x => x.Messages, x => x.MessageId == messageId);

            var seriesResponse = await collection.FindAsync(filter);
            var series = seriesResponse.FirstOrDefault();
            var response = series.Messages.Where(i => i.MessageId == messageId).FirstOrDefault();

            if (response == default(SermonMessage))
            {
                return null;
            }

            return response;
        }

        /// <summary>
        /// Returns all Sermon Series' from MongoDB - including active sermon series'
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<LiveSermons> GetLiveSermons()
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<LiveSermons> collection = db.GetCollection<LiveSermons>("Livestream");
            var documents = await collection.Find(_ => true).FirstOrDefaultAsync();

            return documents;
        }

        /// <summary>
        /// Updates the LiveStreaming object in Mongo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LiveSermons> UpdateLiveSermons(LiveSermons request)
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<LiveSermons> collection = db.GetCollection<LiveSermons>("Livestream");

            var document = await collection.FindOneAndUpdateAsync(
                    Builders<LiveSermons>.Filter
                        .Eq(l => l.Id, request.Id),
                    Builders<LiveSermons>.Update
                    .Set(l => l.LastUpdated, DateTime.UtcNow)
                    .Set(l => l.IsLive, request.IsLive)
                    .Set(l => l.SpecialEventTimes, request.SpecialEventTimes)
                    .Set(l => l.Title, request.Title)
                    .Set(l => l.VideoUrlSlug, request.VideoUrlSlug)
                    .Set(l => l.ExpirationTime, request.ExpirationTime.ToUniversalTime())
                );

            if (document == null || document == default(LiveSermons))
            {
                // something bad happened here
            }

            // get the object again because it's not updated in memory
            var updatedDocument = await collection.FindAsync(
                    Builders<LiveSermons>.Filter.Eq(l => l.Id, request.Id));

            var response = updatedDocument.FirstOrDefault();

            if (response == null || response == default(LiveSermons))
            {
                // something bad happened here
            }

            return response;
        }

        /// <summary>
        /// Update LiveSermons to inactive once the stream has concluded
        /// </summary>
        /// <returns></returns>
        public async Task<LiveSermons> UpdateLiveSermonsInactive()
        {
            var liveSermonsResponse = await GetLiveSermons();

            if (liveSermonsResponse == null || liveSermonsResponse == default(LiveSermons))
            {
                // something bad happened here
                return default(LiveSermons);
            }

            // make the change to reflect that this sermon was just updated
            liveSermonsResponse.IsLive = false;
            liveSermonsResponse.Title = null;
            liveSermonsResponse.VideoUrlSlug = null;
            liveSermonsResponse.SpecialEventTimes = null;

            var updatedLiveSermon = await UpdateLiveSermons(liveSermonsResponse);

            if (updatedLiveSermon == null || updatedLiveSermon == default(LiveSermons))
            {
                // something bad happened here
                return default(LiveSermons);
            }

            return updatedLiveSermon;
        }

        /// <summary>
        /// Returns a collection of recently viewed sermon messages
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RecentMessage>> GetRecentlyWatched(string userId)
        {
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<RecentlyPlayedMessages> collection = db.GetCollection<RecentlyPlayedMessages>("RecentlyPlayed");

            var singleSeries = await collection.FindAsync(
                   Builders<RecentlyPlayedMessages>.Filter.Eq(s => s.UserId, userId));

            var recentPlayedDocument = singleSeries.FirstOrDefault();

            return recentPlayedDocument.RecentMessages;
        }
    }
}