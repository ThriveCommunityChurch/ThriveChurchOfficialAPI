using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;


namespace ThriveChurchOfficialAPI.Repositories
{
    public abstract class RepositoryBase
    {
        #region Read-Only Configs

        /// <summary>
        /// Readonly for MongoConnectionString
        /// </summary>
        public string MongoConnectionString { get; }

        /// <summary>
        /// Readonly for EsvApiKey
        /// </summary>
        public string EsvApiKey { get; }

        /// <summary>
        /// Use this flag to override a check for the EsvApiKey in your appsettings.json,
        /// This setting should also however be located in your settings
        /// </summary>
        public string OverrideEsvApiKey { get; }

        #endregion

        #region Public Vars Set At Runtime

        /// <summary>
        /// Initialize a new MongoClient for the connection to mongo
        /// </summary>
        private readonly MongoClient _mongoClient;

        /// <summary>
        /// Public access to the Mongo Client
        /// </summary>
        public MongoClient Client { get => _mongoClient; }

        #endregion

        // only allow this to be accessible within its class and by derived class instances
        protected RepositoryBase(IConfiguration Configuration)
        {
            // set our keys and connections here in our Base Class
            MongoConnectionString = Configuration["MongoConnectionString"];
            EsvApiKey = Configuration["EsvApiKey"];
            OverrideEsvApiKey = Configuration["OverrideEsvApiKey"];

            // in the event our configs are null, throw a Null Exception
            ValidateConfigs();

            // assuming the configs are valid, create a MongoClient we can use for everything, we only need one.
            // Sets the Client object for a given connection string
            _mongoClient = new MongoClient(MongoConnectionString);
        }

        private void ValidateConfigs()
        {
            if (string.IsNullOrEmpty(MongoConnectionString))
            {
                throw new ArgumentNullException("IConfiguration.MongoConnectionString", 
                    string.Format(SystemMessages.ConnectionMissingFromAppSettings, "MongoConnectionString"));
            }

            var successful = bool.TryParse(OverrideEsvApiKey, out bool EsvKeyOverride);

            // if the setting is false, and there is no key -> throw an exception
            if (!EsvKeyOverride)
            {
                if (string.IsNullOrEmpty(EsvApiKey))
                {
                    throw new ArgumentNullException("IConfiguration.EsvApiKey",
                        string.Format(SystemMessages.ConnectionMissingFromAppSettings, "EsvApiKey"));
                }
            }

            if (!successful)
            {
                throw new ArgumentNullException("IConfiguration.OverrideEsvApiKey",
                        string.Format(SystemMessages.OverrideMissingFromAppSettings));
            }
        }


        #region Generic REST Methods

        public async Task<HttpResponseMessage> GetAsync(string url, string authToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "HttpClient");

            // we may not ever have an auth token in the request
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add("Authorization", authToken);
            }

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);

            return response;
        }

        #endregion
    }
}