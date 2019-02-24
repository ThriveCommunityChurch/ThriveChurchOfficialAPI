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

        #endregion Public Vars Set At Runtime

        /// <summary>
        /// Initialize a new MongoClient for the connection to mongo
        /// </summary>
        public MongoClient Client;

        #region 

        // only allow this to be accessible within its class and by derived class instances
        protected RepositoryBase(IConfiguration Configuration)
        {
            // set our keys and connections here in our Base Class
            MongoConnectionString = Configuration["MongoConnectionString"];
            EsvApiKey = Configuration["EsvApiKey"];

            // in the event our configs are null, throw a Null Exception
            ValidateConfigs();

            // assuming the configs are valid, create a MongoClient we can use for everything, we only need one.
            GetMongoClient();
        }

        /// <summary>
        /// Sets the Client object for a given connection string
        /// </summary>
        public void GetMongoClient()
        {
            Client = new MongoClient(MongoConnectionString);
        }

        private void ValidateConfigs()
        {
            if (string.IsNullOrEmpty(MongoConnectionString))
            {
                throw new ArgumentNullException("IConnection.MongoConnectionString", 
                    string.Format("MongoConnectionString", SystemMessages.ConnectionMissingFromAppSettings));
            }

            if (string.IsNullOrEmpty(EsvApiKey))
            {
                throw new ArgumentNullException("IConnection.EsvApiKey",
                    string.Format("EsvApiKey", SystemMessages.ConnectionMissingFromAppSettings));
            }
        }
    }
}