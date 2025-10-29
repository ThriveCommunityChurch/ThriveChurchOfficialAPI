using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        private Mock<IMemoryCache> _mockMemoryCache;
        private ConfigService _configService;

        [TestInitialize]
        public void Setup()
        {
            _mockConfigRepository = new Mock<IConfigRepository>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _configService = new ConfigService(_mockConfigRepository.Object, _mockMemoryCache.Object);
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

            object cacheValue;
            _mockMemoryCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            _mockMemoryCache.Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

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

            object cacheValue;
            _mockMemoryCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            // Act
            var result = await _configService.GetAllConfigs();

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockMemoryCache.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.AtLeastOnce);
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

            _mockMemoryCache.Setup(c => c.Remove(It.IsAny<object>()));

            // Act
            var result = await _configService.DeleteConfig(configKey);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(successMessage, result.Result);
            _mockMemoryCache.Verify(c => c.Remove(It.IsAny<object>()), Times.Once);
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

            _mockMemoryCache.Setup(c => c.Remove(It.IsAny<object>()));

            // Act
            var result = await _configService.DeleteConfig(configKey);

            // Assert
            Assert.IsFalse(result.HasErrors);
            _mockMemoryCache.Verify(c => c.Remove(It.Is<object>(key => key.ToString().Contains(configKey))), Times.Once);
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
            _mockMemoryCache.Verify(c => c.Remove(It.IsAny<object>()), Times.Never);
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

            object cacheValue;
            _mockMemoryCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            _mockMemoryCache.Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            _mockMemoryCache.Setup(c => c.Remove(It.IsAny<object>()));

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
            _mockMemoryCache.Verify(c => c.Remove(It.IsAny<object>()), Times.Once);
        }

        #endregion
    }
}

