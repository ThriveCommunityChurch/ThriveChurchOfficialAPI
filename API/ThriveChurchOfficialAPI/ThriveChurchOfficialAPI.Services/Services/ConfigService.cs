using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using System.Linq;
using System.Collections.Generic;
using ThriveChurchOfficialAPI.Core.System;

namespace ThriveChurchOfficialAPI.Services
{
    public class ConfigService : BaseService, IConfigService
    {
        private readonly IConfigRepository _configRepository;
        private readonly IMemoryCache _cache;

        public ConfigService(IConfigRepository configRepo,
            IMemoryCache cache)
        {
            // init the repo with the connection string via DI
            _configRepository = configRepo;
            _cache = cache;
        }

        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async Task<SystemResponse<ConfigurationResponse>> GetConfigValue(string setting)
        {
            #region Validations

            if (string.IsNullOrEmpty(setting))
            {
                return new SystemResponse<ConfigurationResponse>(true, SystemMessages.EmptyRequest);
            }

            #endregion

            var settingResponse = await _configRepository.GetConfigValue(setting);
            if (settingResponse.HasErrors)
            {
                return new SystemResponse<ConfigurationResponse>(true, settingResponse.ErrorMessage);
            }

            var response = new ConfigurationResponse
            {
                Value = settingResponse.Result
            };

            return new SystemResponse<ConfigurationResponse>(response, "Success!");
        }
    }
}