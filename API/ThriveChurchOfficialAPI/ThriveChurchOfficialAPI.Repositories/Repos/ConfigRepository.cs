using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using System.Linq;
using System.Collections.Generic;
using Serilog;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class ConfigRepository : RepositoryBase<ConfigSetting>, IConfigRepository
    {
        private readonly IMongoCollection<ConfigSetting> _configCollection;

        /// <summary>
        /// Sermons Repo C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public ConfigRepository(IConfiguration Configuration)
            : base(Configuration)
        {
            _configCollection = DB.GetCollection<ConfigSetting>("Configurations");

            InitCollection();
        }

        private void InitCollection()
        {
            var foundDocs = _configCollection.Find(Builders<ConfigSetting>.Filter.Empty).ToList();

            var defaultPhone = "(555) 555-5555";
            var defaultEmail = "example@example.com";
            var defaultUri = "https://google.com/";
            var defaultValue = "";

            var configs = new List<ConfigSetting>();
            var defaultKeys = new Dictionary<ConfigType, List<string>>
            {
                {
                    ConfigType.Email,
                    new List<string>
                    {
                        "Email_Main"
                    }
                },
                {
                    ConfigType.Link,
                    new List<string>
                    {
                        "SmallGroup_URL",
                        "Live_URL",
                        "Serve_URL",
                        "ImNew_URL",
                        "Give_URL",
                        "FB_Social_URL",
                        "TW_Social_URL",
                        "IG_Social_URL",
                        "Website_URL",
                        "Team_URL",
                        "Prayer_URL"
                    }
                },
                {
                    ConfigType.Phone,
                    new List<string>
                    {
                       "Phone_Main"
                    }
                },
                {
                    ConfigType.Misc,
                    new List<string>
                    {
                        "Address_Main",
                        "Location_Name"
                    }
                },
                {
                    ConfigType.Social,
                    new List<string>
                    {
                        "FB_PageId",
                        "IG_uName",
                        "TW_uName"
                    }
                }
            };

            var totCountConfigs = defaultKeys.SelectMany(i => i.Value);

            // we need to make sure we don't overwrite anything in the collection
            if (foundDocs != null && totCountConfigs.Count() == foundDocs.Count)
            {
                return;
            }

            foreach (var dflt in defaultKeys)
            {
                foreach (var key in dflt.Value)
                {
                    var config = new ConfigSetting
                    {
                        CreateDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                        Type = dflt.Key,
                        Key = key
                    };

                    switch (config.Type)
                    {
                        case ConfigType.Email:
                            config.Value = defaultEmail;
                            break;
                        case ConfigType.Link:
                            config.Value = defaultUri;
                            break;
                        case ConfigType.Phone:
                            config.Value = defaultPhone;
                            break;
                        case ConfigType.Misc:
                            config.Value = defaultValue;
                            break;
                        case ConfigType.Social:
                            config.Value = defaultValue;
                            break;
                    }

                    configs.Add(config);
                }
            }

            // we are only doing an Update here not an upsert or an insert.
            var updateList = new List<WriteModel<ConfigSetting>>();

            foreach (var config in configs)
            {
                var filter = Builders<ConfigSetting>.Filter.Eq(i => i.Key, config.Key);

                var update = Builders<ConfigSetting>.Update.Set(i => i.Value, config.Value)
                    .Set(i => i.LastUpdated, DateTime.UtcNow)
                    .Set(i => i.Key, config.Key)
                    .Set(i => i.Type, config.Type);

                var updateModel = new UpdateOneModel<ConfigSetting>(filter, update)
                {
                    IsUpsert = true
                };

                updateList.Add(updateModel);
            }

            Log.Warning($"Inserting Configs: {updateList?.Count ?? 0}");

            GenerateIndexes();

            _ = _configCollection.BulkWriteAsync(updateList);
        }

        private void GenerateIndexes()
        {
            List<BsonDocument> indexes = _configCollection.Indexes.List().ToList();

            foreach (var existingIndex in indexes)
            {
                if (existingIndex.GetElement("name").Value.ToString() == "Configs_Keys_1")
                {
                    return;
                }
            }

            var index = new CreateIndexModel<ConfigSetting>(new IndexKeysDefinitionBuilder<ConfigSetting>().Ascending(j => j.Key),
                new CreateIndexOptions
                {
                    Name = "Configs_Keys_1",
                    Background = false,
                    Unique = true
                }
            );

            try
            {
                _configCollection.Indexes.CreateOne(index);
            }
            catch (Exception e)
            {
                Log.Error("Error ocurred when generating indexes for Configuration collection.");
                Log.Fatal(string.Format(SystemMessages.ExceptionMessage, e.ToString()));
            }
        }

        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async Task<SystemResponse<ConfigSetting>> GetConfigValue(string setting)
        {
            if (string.IsNullOrEmpty(setting))
            {
                return new SystemResponse<ConfigSetting>(true, string.Format(SystemMessages.NullProperty, nameof(setting)));
            }

            var filter = Builders<ConfigSetting>.Filter.Eq(i => i.Key, setting);

            var cursor = await _configCollection.FindAsync(filter);

            var found = cursor.FirstOrDefault();
            if (found == null || found == default(ConfigSetting))
            {
                return new SystemResponse<ConfigSetting>(true, string.Format(SystemMessages.UnableToFindConfigForKey, setting));
            }

            return new SystemResponse<ConfigSetting>(found, "Success!");
        }

        /// <summary>
        /// Get values for a collection of config settings
        /// </summary>
        /// <param name="request"></param>
        /// <param name="createRequest"></param>
        /// <returns></returns>
        public async Task<SystemResponse<IEnumerable<ConfigSetting>>> GetConfigValues(IEnumerable<string> request)
        {
            if (request == null || !request.Any() || request.Any(i => string.IsNullOrEmpty(i)))
            {
                return new SystemResponse<IEnumerable<ConfigSetting>>(true, string.Format(SystemMessages.NullProperty, nameof(request)));
            }

            var filter = Builders<ConfigSetting>.Filter.In(i => i.Key, request);

            var cursor = await _configCollection.FindAsync(filter);

            var found = cursor.ToList();
            if (found == null)
            {
                return new SystemResponse<IEnumerable<ConfigSetting>>(true, SystemMessages.UnableToFindConfigs);
            }

            if (found.Count != request.Count())
            {
                return new SystemResponse<IEnumerable<ConfigSetting>>(true, SystemMessages.ConfigValuesNotFound);
            }

            return new SystemResponse<IEnumerable<ConfigSetting>>(found, "Success!");
        }

        /// <summary>
        /// Set values for config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<string>> SetConfigValues(SetConfigRequest request)
        {
            if (request == null || !request.Configurations.Any())
            {
                return new SystemResponse<string>(true, string.Format(SystemMessages.NullProperty, nameof(SetConfigRequest.Configurations)));
            }

            // we are only doing an Update here not an upsert or an insert.
            var updateList = new List<WriteModel<ConfigSetting>>();

            foreach (var config in request.Configurations)
            {
                var filter = Builders<ConfigSetting>.Filter.Eq(i => i.Key, config.Key);

                var update = Builders<ConfigSetting>.Update.Set(i => i.Value, config.Value)
                    .Set(i => i.LastUpdated, DateTime.UtcNow);

                updateList.Add(new UpdateOneModel<ConfigSetting>(filter, update));
            }

            var updates = await _configCollection.BulkWriteAsync(updateList);

            var updateResponse = $"{request.Configurations.Count()} configurations updated";

            return new SystemResponse<string>(updateResponse, "Success!");
        }
    }
}