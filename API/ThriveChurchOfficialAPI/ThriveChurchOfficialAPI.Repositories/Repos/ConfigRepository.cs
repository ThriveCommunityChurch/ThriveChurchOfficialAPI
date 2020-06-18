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
        public async Task<SystemResponse<string>> GetConfigValue(string setting)
        {
            return null;
        }
    }
}