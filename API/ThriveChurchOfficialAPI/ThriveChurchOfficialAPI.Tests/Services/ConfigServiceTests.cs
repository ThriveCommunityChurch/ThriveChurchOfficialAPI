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

