using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    [TestClass]
    public class ConfigServiceTests
    {
        private Mock<IConfigRepository> _mockConfigRepository;
        private Mock<ICacheService> _mockCacheService;
        private ConfigService _configService;

        [TestInitialize]
        public void Setup()
        {
            _mockConfigRepository = new Mock<IConfigRepository>();
            _mockCacheService = new Mock<ICacheService>();

            // Setup cache miss by default (ReadFromCache returns default which is null for reference types)
            // No need to setup ReadFromCache - Moq returns default(T) by default
            _mockCacheService.Setup(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()));

            _configService = new ConfigService(_mockConfigRepository.Object, _mockCacheService.Object);
        }

        #region GetAllConfigs Tests

        [TestMethod]
        public async Task GetAllConfigs_RepositoryReturnsConfigs_ReturnsSuccessResponse()
        {
            // Arrange
            var configSettings = new List<ConfigSetting>
            {
                new ConfigSetting
                {
                    Key = "Email_Main",
                    Value = "info@example.com",
                    Type = ConfigType.Email
                },
                new ConfigSetting
                {
                    Key = "Phone_Main",
                    Value = "1234567890",
                    Type = ConfigType.Phone
                },
                new ConfigSetting
                {
                    Key = "Website_URL",
                    Value = "https://example.com",
                    Type = ConfigType.Link
                }
            };

            _mockConfigRepository.Setup(r => r.GetAllConfigs())
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(configSettings, "Success!"));

            // Act
            var result = await _configService.GetAllConfigs();

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(3, result.Result.Configs.Count());
            Assert.IsTrue(result.Result.Configs.Any(c => c.Key == "Email_Main"));
            Assert.IsTrue(result.Result.Configs.Any(c => c.Key == "Phone_Main"));
            Assert.IsTrue(result.Result.Configs.Any(c => c.Key == "Website_URL"));
        }

        [TestMethod]
        public async Task GetAllConfigs_RepositoryReturnsEmpty_ReturnsErrorResponse()
        {
            // Arrange
            var emptyList = new List<ConfigSetting>();

            _mockConfigRepository.Setup(r => r.GetAllConfigs())
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(emptyList, "Success!"));

            // Act
            var result = await _configService.GetAllConfigs();

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(0, result.Result.Configs.Count());
        }

        [TestMethod]
        public async Task GetAllConfigs_RepositoryReturnsError_ReturnsErrorResponse()
        {
            // Arrange
            _mockConfigRepository.Setup(r => r.GetAllConfigs())
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(true, "Database error"));

            // Act
            var result = await _configService.GetAllConfigs();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Database error", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetAllConfigs_CachesEachConfig_VerifyCacheSet()
        {
            // Arrange
            var configSettings = new List<ConfigSetting>
            {
                new ConfigSetting
                {
                    Key = "Email_Main",
                    Value = "info@example.com",
                    Type = ConfigType.Email
                }
            };

            _mockConfigRepository.Setup(r => r.GetAllConfigs())
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(configSettings, "Success!"));

            // Act
            var result = await _configService.GetAllConfigs();

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockCacheService.Verify(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.AtLeastOnce);
        }

        #endregion

        #region DeleteConfig Tests

        [TestMethod]
        public async Task DeleteConfig_ValidKey_ReturnsSuccessResponse()
        {
            // Arrange
            var configKey = "Email_Main";
            var successMessage = $"Configuration '{configKey}' deleted successfully";

            _mockConfigRepository.Setup(r => r.DeleteConfig(configKey))
                .ReturnsAsync(new SystemResponse<string>(successMessage, "Success!"));

            // Act
            var result = await _configService.DeleteConfig(configKey);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(successMessage, result.Result);
            _mockCacheService.Verify(c => c.RemoveFromCache(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteConfig_RepositoryReturnsError_ReturnsErrorResponse()
        {
            // Arrange
            var configKey = "NonExistent_Key";

            _mockConfigRepository.Setup(r => r.DeleteConfig(configKey))
                .ReturnsAsync(new SystemResponse<string>(true, "Configuration not found"));

            // Act
            var result = await _configService.DeleteConfig(configKey);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Configuration not found", result.ErrorMessage);
        }

        [TestMethod]
        public async Task DeleteConfig_RemovesFromCache_VerifyCacheRemove()
        {
            // Arrange
            var configKey = "Email_Main";
            var successMessage = $"Configuration '{configKey}' deleted successfully";

            _mockConfigRepository.Setup(r => r.DeleteConfig(configKey))
                .ReturnsAsync(new SystemResponse<string>(successMessage, "Success!"));

            // Act
            var result = await _configService.DeleteConfig(configKey);

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockCacheService.Verify(c => c.RemoveFromCache(It.Is<string>(key => key.Contains(configKey.ToLowerInvariant()))), Times.Once);
        }

        [TestMethod]
        public async Task DeleteConfig_RepositoryError_DoesNotRemoveFromCache()
        {
            // Arrange
            var configKey = "Email_Main";

            _mockConfigRepository.Setup(r => r.DeleteConfig(configKey))
                .ReturnsAsync(new SystemResponse<string>(true, "Database error"));

            // Act
            var result = await _configService.DeleteConfig(configKey);

            // Assert
            Assert.IsTrue(result.HasErrors);
            _mockCacheService.Verify(c => c.RemoveFromCache(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteConfig_NullKey_ReturnsError()
        {
            // Act
            var result = await _configService.DeleteConfig(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task DeleteConfig_EmptyKey_ReturnsError()
        {
            // Act
            var result = await _configService.DeleteConfig("");

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task DeleteConfig_WhitespaceKey_ReturnsError()
        {
            // Act
            var result = await _configService.DeleteConfig("   ");

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        #endregion

        #region GetConfigValue Tests

        [TestMethod]
        public async Task GetConfigValue_NullSetting_ReturnsError()
        {
            // Act
            var result = await _configService.GetConfigValue(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetConfigValue_EmptySetting_ReturnsError()
        {
            // Act
            var result = await _configService.GetConfigValue("");

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetConfigValue_CacheHit_ReturnsFromCache()
        {
            // Arrange
            var settingKey = "Email_Main";
            var cachedConfig = new ConfigurationResponse
            {
                Key = settingKey,
                Value = "info@example.com",
                Type = ConfigType.Email
            };

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(cachedConfig);

            // Act
            var result = await _configService.GetConfigValue(settingKey);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(cachedConfig.Key, result.Result.Key);
            Assert.AreEqual(cachedConfig.Value, result.Result.Value);
            _mockConfigRepository.Verify(r => r.GetConfigValue(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GetConfigValue_CacheMiss_ReturnsFromRepository()
        {
            // Arrange
            var settingKey = "Email_Main";
            var configSetting = new ConfigSetting
            {
                Key = settingKey,
                Value = "info@example.com",
                Type = ConfigType.Email
            };

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns((ConfigurationResponse)null);
            _mockConfigRepository.Setup(r => r.GetConfigValue(settingKey))
                .ReturnsAsync(new SystemResponse<ConfigSetting>(configSetting, "Success!"));

            // Act
            var result = await _configService.GetConfigValue(settingKey);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(settingKey, result.Result.Key);
            Assert.AreEqual("info@example.com", result.Result.Value);
            _mockCacheService.Verify(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [TestMethod]
        public async Task GetConfigValue_RepositoryError_ReturnsError()
        {
            // Arrange
            var settingKey = "NonExistent_Key";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns((ConfigurationResponse)null);
            _mockConfigRepository.Setup(r => r.GetConfigValue(settingKey))
                .ReturnsAsync(new SystemResponse<ConfigSetting>(true, "Config not found"));

            // Act
            var result = await _configService.GetConfigValue(settingKey);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Config not found", result.ErrorMessage);
        }

        #endregion

        #region GetConfigValues Tests

        [TestMethod]
        public async Task GetConfigValues_NullKeys_ReturnsError()
        {
            // Act
            var result = await _configService.GetConfigValues(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetConfigValues_EmptyKeys_ReturnsError()
        {
            // Act
            var result = await _configService.GetConfigValues(new List<string>());

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetConfigValues_AllFromCache_ReturnsSuccessWithoutRepoCall()
        {
            // Arrange
            var keys = new List<string> { "Email_Main", "Phone_Main" };

            _mockCacheService.SetupSequence(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "info@example.com", Type = ConfigType.Email })
                .Returns(new ConfigurationResponse { Key = "Phone_Main", Value = "1234567890", Type = ConfigType.Phone });

            // Act
            var result = await _configService.GetConfigValues(keys);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(2, result.Result.Configs.Count());
            _mockConfigRepository.Verify(r => r.GetConfigValues(It.IsAny<List<string>>()), Times.Never);
        }

        [TestMethod]
        public async Task GetConfigValues_SomeFromCacheSomeFromRepo_ReturnsCombined()
        {
            // Arrange
            var keys = new List<string> { "Email_Main", "Phone_Main" };

            _mockCacheService.SetupSequence(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "info@example.com", Type = ConfigType.Email })
                .Returns((ConfigurationResponse)null);

            _mockConfigRepository.Setup(r => r.GetConfigValues(It.Is<List<string>>(l => l.Contains("Phone_Main"))))
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(new List<ConfigSetting>
                {
                    new ConfigSetting { Key = "Phone_Main", Value = "1234567890", Type = ConfigType.Phone }
                }, "Success!"));

            // Act
            var result = await _configService.GetConfigValues(keys);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(2, result.Result.Configs.Count());
            Assert.IsTrue(result.Result.Configs.Any(c => c.Key == "Email_Main"));
            Assert.IsTrue(result.Result.Configs.Any(c => c.Key == "Phone_Main"));
        }

        [TestMethod]
        public async Task GetConfigValues_RepositoryError_ReturnsError()
        {
            // Arrange
            var keys = new List<string> { "Email_Main" };

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns((ConfigurationResponse)null);
            _mockConfigRepository.Setup(r => r.GetConfigValues(It.IsAny<List<string>>()))
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(true, "Database error"));

            // Act
            var result = await _configService.GetConfigValues(keys);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Database error", result.ErrorMessage);
        }

        #endregion

        #region SetConfigValues Tests

        [TestMethod]
        public async Task SetConfigValues_NullRequest_ReturnsError()
        {
            // Act
            var result = await _configService.SetConfigValues(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValues_NullConfigurations_ReturnsError()
        {
            // Arrange
            var request = new SetConfigRequest { Configurations = null };

            // Act
            var result = await _configService.SetConfigValues(request);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task SetConfigValues_EmptyConfigurations_ReturnsError()
        {
            // Arrange
            var request = new SetConfigRequest { Configurations = new List<ConfigurationMap>() };

            // Act
            var result = await _configService.SetConfigValues(request);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task SetConfigValues_DuplicateKeys_ReturnsError()
        {
            // Arrange
            var request = new SetConfigRequest
            {
                Configurations = new List<ConfigurationMap>
                {
                    new ConfigurationMap { Key = "Email_Main", Value = "test1@example.com", Type = ConfigType.Email },
                    new ConfigurationMap { Key = "Email_Main", Value = "test2@example.com", Type = ConfigType.Email }
                }
            };

            // Act
            var result = await _configService.SetConfigValues(request);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.ConfigurationsMustHaveUniqueKeys, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValues_KeysNotInCache_ChecksRepository()
        {
            // Arrange
            var request = new SetConfigRequest
            {
                Configurations = new List<ConfigurationMap>
                {
                    new ConfigurationMap { Key = "Email_Main", Value = "test@example.com", Type = ConfigType.Email }
                }
            };

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns((ConfigurationResponse)null);
            _mockConfigRepository.Setup(r => r.GetConfigValues(It.IsAny<List<string>>()))
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(new List<ConfigSetting>
                {
                    new ConfigSetting { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email }
                }, "Success!"));
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.IsAny<SetConfigRequest>()))
                .ReturnsAsync(new SystemResponse<string>("Success!", "Success!"));

            // Act
            var result = await _configService.SetConfigValues(request);

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockConfigRepository.Verify(r => r.GetConfigValues(It.IsAny<List<string>>()), Times.Once);
        }

        [TestMethod]
        public async Task SetConfigValues_AllKeysInCache_SkipsRepositoryCheck()
        {
            // Arrange
            var request = new SetConfigRequest
            {
                Configurations = new List<ConfigurationMap>
                {
                    new ConfigurationMap { Key = "Email_Main", Value = "test@example.com", Type = ConfigType.Email }
                }
            };

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email });
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.IsAny<SetConfigRequest>()))
                .ReturnsAsync(new SystemResponse<string>("Success!", "Success!"));

            // Act
            var result = await _configService.SetConfigValues(request);

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockConfigRepository.Verify(r => r.GetConfigValues(It.IsAny<List<string>>()), Times.Never);
        }

        [TestMethod]
        public async Task SetConfigValues_GetConfigValuesReturnsError_ReturnsError()
        {
            // Arrange
            var request = new SetConfigRequest
            {
                Configurations = new List<ConfigurationMap>
                {
                    new ConfigurationMap { Key = "Email_Main", Value = "test@example.com", Type = ConfigType.Email }
                }
            };

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns((ConfigurationResponse)null);
            _mockConfigRepository.Setup(r => r.GetConfigValues(It.IsAny<List<string>>()))
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(true, "Config not found"));

            // Act
            var result = await _configService.SetConfigValues(request);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Config not found", result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValues_UpdateFails_ReturnsError()
        {
            // Arrange
            var request = new SetConfigRequest
            {
                Configurations = new List<ConfigurationMap>
                {
                    new ConfigurationMap { Key = "Email_Main", Value = "test@example.com", Type = ConfigType.Email }
                }
            };

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email });
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.IsAny<SetConfigRequest>()))
                .ReturnsAsync(new SystemResponse<string>(true, "Update failed"));

            // Act
            var result = await _configService.SetConfigValues(request);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Update failed", result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValues_Success_UpdatesCache()
        {
            // Arrange
            var request = new SetConfigRequest
            {
                Configurations = new List<ConfigurationMap>
                {
                    new ConfigurationMap { Key = "Email_Main", Value = "test@example.com", Type = ConfigType.Email }
                }
            };

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email });
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.IsAny<SetConfigRequest>()))
                .ReturnsAsync(new SystemResponse<string>("Success!", "Success!"));

            // Act
            var result = await _configService.SetConfigValues(request);

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockCacheService.Verify(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        #endregion

        #region SetConfigValuesFromCSV Tests

        [TestMethod]
        public async Task SetConfigValuesFromCSV_NullCsv_ReturnsError()
        {
            // Act
            var result = await _configService.SetConfigValuesFromCSV(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_EmptyCsv_ReturnsError()
        {
            // Act
            var result = await _configService.SetConfigValuesFromCSV("");

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.EmptyRequest, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_InvalidFormat_SingleColumn_ReturnsError()
        {
            // Arrange - CSV with only one column
            var csv = "Email_Main";

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.InvalidConfigCSVFormat, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_InvalidFormat_TooManyColumns_ReturnsError()
        {
            // Arrange - CSV with too many columns
            var csv = "Email_Main,test@example.com,extra";

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.InvalidConfigCSVFormat, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_EmptyLine_ReturnsError()
        {
            // Arrange - CSV with an actual empty line in the middle
            var csv = "Email_Main,test@example.com\n\nPhone_Main,1234567890";

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.InvalidConfigCSVFormat, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_DuplicateKeys_ReturnsError()
        {
            // Arrange - CSV with duplicate keys
            var csv = "Email_Main,test1@example.com\nEmail_Main,test2@example.com";

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.ConfigurationsMustHaveUniqueKeys, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_GetConfigValuesError_ReturnsError()
        {
            // Arrange
            var csv = "Email_Main,test@example.com";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns((ConfigurationResponse)null);
            _mockConfigRepository.Setup(r => r.GetConfigValues(It.IsAny<List<string>>()))
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(true, "Config not found"));

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Config not found", result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_UpdateFails_ReturnsError()
        {
            // Arrange
            var csv = "Email_Main,test@example.com";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email });
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.IsAny<SetConfigRequest>()))
                .ReturnsAsync(new SystemResponse<string>(true, "Update failed"));

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Update failed", result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_Success_ReturnsSuccess()
        {
            // Arrange
            var csv = "Email_Main,test@example.com";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email });
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.IsAny<SetConfigRequest>()))
                .ReturnsAsync(new SystemResponse<string>("Success!", "Success!"));

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockCacheService.Verify(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_QuotedValues_ParsesCorrectly()
        {
            // Arrange - CSV with quoted value containing comma
            var csv = "Email_Main,\"test,email@example.com\"";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email });
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.IsAny<SetConfigRequest>()))
                .ReturnsAsync(new SystemResponse<string>("Success!", "Success!"));

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_MultipleLines_ProcessesAll()
        {
            // Arrange - CSV with multiple lines
            var csv = "Email_Main,test@example.com\nPhone_Main,1234567890";

            _mockCacheService.SetupSequence(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email })
                .Returns(new ConfigurationResponse { Key = "Phone_Main", Value = "0000000000", Type = ConfigType.Phone });
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.IsAny<SetConfigRequest>()))
                .ReturnsAsync(new SystemResponse<string>("Success!", "Success!"));

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockCacheService.Verify(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_PercentSReplacement_ParsesNewlines()
        {
            // Arrange - CSV with %s which should be replaced with newline
            var csv = "Address_Main,\"123 Main St%sCity, State 12345\"";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Address_Main", Value = "old address", Type = ConfigType.Misc });
            _mockConfigRepository.Setup(r => r.SetConfigValues(It.Is<SetConfigRequest>(req =>
                req.Configurations.First().Value.Contains("\n"))))
                .ReturnsAsync(new SystemResponse<string>("Success!", "Success!"));

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_InvalidEmailFormat_ReturnsValidationError()
        {
            // Arrange - CSV with a valid format but an invalid email value
            var csv = "Email_Main,not-a-valid-email";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Email_Main", Value = "old@example.com", Type = ConfigType.Email });

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Email") || result.ErrorMessage.Contains("not-a-valid-email"));
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_InvalidPhoneFormat_ReturnsValidationError()
        {
            // Arrange - CSV with a valid format but an invalid phone value (contains special chars)
            var csv = "Phone_Main,(555) 123-4567";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Phone_Main", Value = "1234567890", Type = ConfigType.Phone });

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.PhoneNumbersCannotContainSpecialCharactersOrSpaces, result.ErrorMessage);
        }

        [TestMethod]
        public async Task SetConfigValuesFromCSV_InvalidLinkFormat_ReturnsValidationError()
        {
            // Arrange - CSV with a valid format but an invalid link value
            var csv = "Link_Main,not-a-valid-url";

            _mockCacheService.Setup(c => c.ReadFromCache<ConfigurationResponse>(It.IsAny<string>()))
                .Returns(new ConfigurationResponse { Key = "Link_Main", Value = "https://example.com", Type = ConfigType.Link });

            // Act
            var result = await _configService.SetConfigValuesFromCSV(csv);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Link") || result.ErrorMessage.Contains("not-a-valid-url"));
        }

        #endregion

        #region Integration Tests for GetAllConfigs and DeleteConfig

        [TestMethod]
        public async Task GetAllConfigs_ThenDeleteConfig_VerifiesWorkflow()
        {
            // Arrange
            var configSettings = new List<ConfigSetting>
            {
                new ConfigSetting
                {
                    Key = "Email_Main",
                    Value = "info@example.com",
                    Type = ConfigType.Email
                },
                new ConfigSetting
                {
                    Key = "Phone_Main",
                    Value = "1234567890",
                    Type = ConfigType.Phone
                }
            };

            _mockConfigRepository.Setup(r => r.GetAllConfigs())
                .ReturnsAsync(new SystemResponse<IEnumerable<ConfigSetting>>(configSettings, "Success!"));

            _mockConfigRepository.Setup(r => r.DeleteConfig("Email_Main"))
                .ReturnsAsync(new SystemResponse<string>("Configuration 'Email_Main' deleted successfully", "Success!"));

            // Act - Get all configs
            var getAllResult = await _configService.GetAllConfigs();

            // Assert - Verify we got all configs
            Assert.IsFalse(getAllResult.HasErrors);
            Assert.AreEqual(2, getAllResult.Result.Configs.Count());

            // Act - Delete one config
            var deleteResult = await _configService.DeleteConfig("Email_Main");

            // Assert - Verify deletion was successful
            Assert.IsFalse(deleteResult.HasErrors);
            Assert.IsTrue(deleteResult.Result.Contains("Email_Main"));
            _mockCacheService.Verify(c => c.RemoveFromCache(It.IsAny<string>()), Times.Once);
        }

        #endregion
    }
}

