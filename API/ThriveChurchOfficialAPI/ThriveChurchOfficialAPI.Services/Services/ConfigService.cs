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
using System.IO;
using System.Text.RegularExpressions;

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
            if (!_cache.TryGetValue(string.Format(CacheKeys.GetConfig, setting), out ConfigurationResponse value))
            {
                // Key not in cache, so get data.
                var settingResponse = await _configRepository.GetConfigValue(setting);
                if (settingResponse.HasErrors)
                {
                    return new SystemResponse<ConfigurationResponse>(true, settingResponse.ErrorMessage);
                }

                var result = settingResponse.Result;

                var response = new ConfigurationResponse
                {
                    Key = result.Key,
                    Value = result.Value,
                    Type = result.Type
                };

                // Save data in cache.
                _cache.Set(string.Format(CacheKeys.GetConfig, setting), result, PersistentCacheEntryOptions);
            }

            return new SystemResponse<ConfigurationResponse>(value, "Success!");
        }

        /// <summary>
        /// Set values for config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<string>> SetConfigValues(SetConfigRequest request)
        {
            #region Validations

            var validationResponse = SetConfigRequest.Validate(request);
            if (validationResponse.HasErrors)
            {
                return new SystemResponse<string>(true, validationResponse.ErrorMessage);
            }

            var keysToUpdate = request.Configurations.Select(i => i.Key).ToList();
            var uniqueKeys = new HashSet<string>(keysToUpdate);

            if (keysToUpdate.Count() != uniqueKeys.Count)
            {
                return new SystemResponse<string>(true, SystemMessages.ConfigurationsMustHaveUniqueKeys);
            }

            var keysNotFound = new List<string>();

            foreach (var settingKey in uniqueKeys)
            {
                // check the cache first -> if there's a value there grab it
                if (!_cache.TryGetValue(string.Format(CacheKeys.GetConfig, settingKey), out ConfigurationResponse value))
                {
                    keysNotFound.Add(settingKey);
                    continue;
                }
            }

            var settingResponse = await _configRepository.GetConfigValues(keysNotFound);
            if (settingResponse.HasErrors)
            {
                return new SystemResponse<string>(true, settingResponse.ErrorMessage);
            }

            #endregion

            var foundValues = settingResponse.Result;

            var updateResponse = await _configRepository.SetConfigValues(request);
            if (updateResponse.HasErrors)
            {
                return new SystemResponse<string>(true, updateResponse.ErrorMessage);
            }

            foreach (var setting in request.Configurations)
            {
                var config = new ConfigurationResponse
                {
                    Value = setting.Value,
                    Type = setting.Type,
                    Key = setting.Key
                };

                // Save data in cache.
                _cache.Set(string.Format(CacheKeys.GetConfig, setting.Key), config, PersistentCacheEntryOptions);
            }

            return new SystemResponse<string>($"Updated {keysToUpdate.Count}", "Success!");
        }

        /// <summary>
        /// Set values for config settings from a CSV
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public async Task<SystemResponse<string>> SetConfigValuesFromCSV(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return new SystemResponse<string>(true, SystemMessages.EmptyRequest);
            }

            var requestedUpdates = new Dictionary<string, string>();

            // Okay so I need to enforce the format that the Keys are aleays in the first column and the values are always in the 2nd
            using (StringReader reader = new StringReader(csv))
            {
                string readText = reader.ReadLine();

                if (string.IsNullOrEmpty(readText))
                {
                    return new SystemResponse<string>(true, SystemMessages.InvalidConfigCSVFormat);
                }

                // We need to only split on the strings that are not within an escaped set of string quotes
                Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                string[] csvValues = CSVParser.Split(readText);

                if (csvValues == null || !csvValues.Any() || csvValues.Count() != 2)
                {
                    return new SystemResponse<string>(true, SystemMessages.InvalidConfigCSVFormat);
                }

                var key = csvValues[0];
                var value = csvValues[1];

                if (requestedUpdates.ContainsKey(key))
                {
                    return new SystemResponse<string>(true, SystemMessages.ConfigurationsMustHaveUniqueKeys);
                }
                else
                {
                    requestedUpdates[key] = value;
                }
            }

            var getConfigRequest = new ConfigKeyRequest
            {
                Keys = requestedUpdates.Keys
            };

            var getApplicableKeys = await GetConfigValues(getConfigRequest);
            if (getApplicableKeys.HasErrors)
            {
                return new SystemResponse<string>(true, getApplicableKeys.ErrorMessage);
            }

            var settings = getApplicableKeys.Result.Configs.ToDictionary(Key => Key.Key, Value => Value);
            var updateRequest = new SetConfigRequest();
            var updates = new List<ConfigurationMap>();

            foreach (var setting in requestedUpdates)
            {
                var foundSetting = settings[setting.Key];

                updates.Add(new ConfigurationMap
                {
                    Key = setting.Key,
                    Type = foundSetting.Type,
                    Value = setting.Value
                });
            }

            var validationResponse = SetConfigRequest.Validate(updateRequest);
            if (validationResponse.HasErrors)
            {
                return new SystemResponse<string>(true, validationResponse.ErrorMessage);
            }

            var updateResponse = await _configRepository.SetConfigValues(updateRequest);
            if (updateResponse.HasErrors)
            {
                return new SystemResponse<string>(true, updateResponse.ErrorMessage);
            }

            return new SystemResponse<string>("Success!", "Success!");
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

            var keysNotFount = new List<string>();
            var finalList = new List<ConfigurationResponse>();

            foreach (var settingKey in request.Keys)
            {
                // check the cache first -> if there's a value there grab it
                if (!_cache.TryGetValue(string.Format(CacheKeys.GetConfig, settingKey), out ConfigurationResponse value))
                {
                    keysNotFount.Add(settingKey);
                    continue;
                }

                finalList.Add(value);
            }

            var settingResponse = await _configRepository.GetConfigValues(request);
            if (settingResponse.HasErrors)
            {
                return new SystemResponse<ConfigurationCollectionResponse>(true, settingResponse.ErrorMessage);
            }

            var foundValues = settingResponse.Result;
            if (foundValues.Count() != request.Keys.Count())
            {
                return new SystemResponse<ConfigurationCollectionResponse>(true, SystemMessages.ConfigValuesNotFound);
            }

            foreach (var setting in foundValues)
            {
                finalList.Add(new ConfigurationResponse
                {
                    Key = setting.Key,
                    Value = setting.Value,
                    Type = setting.Type
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