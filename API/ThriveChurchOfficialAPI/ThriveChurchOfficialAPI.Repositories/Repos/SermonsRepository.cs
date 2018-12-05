using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class SermonsRepository: RepositoryBase, ISermonsRepository //MongoConnectionBase
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
    }
}
