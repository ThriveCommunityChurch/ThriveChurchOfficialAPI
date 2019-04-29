using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class TokenRepo: ITokenRepo
    {
        private readonly IMongoDatabase db;

        /// <summary>
        /// Initialize a new MongoClient for the connection to mongo
        /// </summary>
        private readonly MongoClient _mongoClient;

        /// <summary>
        /// Public readonly access to the Mongo Client
        /// </summary>
        public MongoClient Client { get => _mongoClient; }

        /// <summary>
        /// Sermons Repo C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public TokenRepo(IConfiguration Configuration)
        {
            // set our keys and connections here in our Base Class
            var MongoConnectionString = Configuration["MongoConnectionString"];
            var TokenCollectionLocation = Configuration["TokenConnectionStringPath"];

            // assuming the configs are valid, create a MongoClient we can use for everything, we only need one.
            // Sets the Client object for a given connection string
            _mongoClient = new MongoClient(MongoConnectionString);

            db = Client.GetDatabase(TokenCollectionLocation);
        }

        /// <summary>
        /// Validate API Keys
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public ValidationResponse ValidateToken(string apiKey)
        {
            IMongoCollection<TokenHandler> collection = db.GetCollection<TokenHandler>("ApiKeys");

            var response = collection.Find(
                   Builders<TokenHandler>.Filter.Eq(s => s.ApiKey, apiKey)).FirstOrDefault();

            if (response == null)
            {
                // do not return the hashed key
                return new ValidationResponse(true, string.Format("ThriveAPIKey does not exist."));
            }

            return new ValidationResponse("Success!");
        }
    }
}
