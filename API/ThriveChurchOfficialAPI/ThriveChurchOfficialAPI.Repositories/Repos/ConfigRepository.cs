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
    public class ConfigRepository: RepositoryBase, IConfigRepository
    {
        private readonly IMongoCollection<ConfigSettings> _configCollection;

        /// <summary>
        /// Sermons Repo C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public ConfigRepository(IConfiguration Configuration)
            : base(Configuration)
        {
            _configCollection = DB.GetCollection<ConfigSettings>("Configurations");
        }

        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async Task<SystemResponse<ConfigSettings>> GetConfigValue(string setting)
        {
            if (string.IsNullOrEmpty(setting))
            {
                return new SystemResponse<ConfigSettings>(true, string.Format(SystemMessages.NullProperty, nameof(setting)));
            }

            var filter = Builders<ConfigSettings>.Filter.Eq(i => i.Key, setting);

            var cursor = await _configCollection.FindAsync(filter);

            var found = cursor.FirstOrDefault();
            if (found == null || found == default(ConfigSettings))
            {
                return new SystemResponse<ConfigSettings>(true, string.Format(SystemMessages.UnableToFindConfigForKey, nameof(setting)));
            }

            return new SystemResponse<ConfigSettings>(found, "Success!");
        }

        /// <summary>
        /// Get values for a collection of config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<IEnumerable<ConfigSettings>>> GetConfigValues(IEnumerable<string> request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get values for a collection of config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<IEnumerable<ConfigSettings>>> GetConfigValues(ConfigKeyRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set values for config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<IEnumerable<string>>> SetConfigValues(SetConfigRequest request)
        {
            throw new NotImplementedException();
        }
    }
}