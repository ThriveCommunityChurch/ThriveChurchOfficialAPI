using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using System.Linq;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class SermonsRepository : RepositoryBase, ISermonsRepository //MongoConnectionBase
    {
        public readonly string connectionString;

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
            await collection.InsertOneAsync(request);

            // respond with the inserted object
            var inserted = await collection.FindAsync(
                    Builders<SermonSeries>.Filter.Eq(l => l.Slug, request.Slug));

            var response = inserted.FirstOrDefault();

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
    }
}