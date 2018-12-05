using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

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
            // connect to mongo and get 'r done
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");
            var documents = await collection.Find(_ => true).ToListAsync();

            var allSermonsResponse = new AllSermonsResponse()
            {
                Sermons = documents
            };

            return allSermonsResponse;
        }

        /// <summary>
        /// Returns all Sermon Series' from MongoDB - including active sermon series'
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<LiveSermons> GetLiveSermons()
        {
            // connect to mongo and get 'r done
            var client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("SermonSeries");
            IMongoCollection<LiveSermons> collection = db.GetCollection<LiveSermons>("Livestream");
            var documents = await collection.Find(_ => true).FirstAsync();

            return documents;
        }

        /// <summary>
        /// Updates the LiveStreaming object in Mongo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LiveSermons> UpdateLiveSermons(LiveSermons request)
        {
            // connect to mongo and get 'r done
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
                    .Set(l => l.ExpirationTime, request.ExpirationTime)
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
    }
}