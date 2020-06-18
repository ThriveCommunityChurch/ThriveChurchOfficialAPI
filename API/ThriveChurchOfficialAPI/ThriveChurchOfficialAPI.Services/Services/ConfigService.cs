﻿using System;
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

            // check the cache first -> if there's a value there grab it
            if (!_cache.TryGetValue(string.Format(CacheKeys.GetConfig, setting), out string value))
            {
                // Key not in cache, so get data.
                var settingResponse = await _configRepository.GetConfigValue(setting);
                if (settingResponse.HasErrors)
                {
                    return new SystemResponse<ConfigurationResponse>(true, settingResponse.ErrorMessage);
                }

                value = settingResponse.Result;

                // Save data in cache.
                _cache.Set(string.Format(CacheKeys.GetConfig, setting), value, PersistentCacheEntryOptions);
            }

            var response = new ConfigurationResponse
            {
                Value = value
            };

            return new SystemResponse<ConfigurationResponse>(response, "Success!");
        }

        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async Task<SystemResponse<ConfigurationCollectionResponse>> GetConfigValues(ConfigKeyRequest request)
        {
            #region Validations

            var validationResponse = ConfigKeyRequest.Validate(request);
            if (validationResponse.HasErrors)
            {
                return new SystemResponse<ConfigurationCollectionResponse>(true, SystemMessages.EmptyRequest);
            }

            #endregion

            var settingResponse = await _configRepository.GetConfigValues(request);
            if (settingResponse.HasErrors)
            {
                return new SystemResponse<ConfigurationCollectionResponse>(true, settingResponse.ErrorMessage);
            }

            var foundValues = settingResponse.Result;
            var finalList = new List<ConfigurationResponseMap>();

            foreach (var setting in foundValues)
            {
                finalList.Add(new ConfigurationResponseMap
                {
                    Key = setting.Key,
                    Value = setting.Value
                });
            }

            var response = new ConfigurationCollectionResponse
            {
                Configs = finalList
            };

            return new SystemResponse<ConfigurationCollectionResponse>(response, "Success!");
        }
    }
}