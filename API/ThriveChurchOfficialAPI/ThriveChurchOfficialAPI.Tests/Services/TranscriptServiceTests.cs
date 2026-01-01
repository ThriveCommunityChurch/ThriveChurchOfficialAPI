using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    /// <summary>
    /// Tests for TranscriptService - focuses on input validation and configuration checks.
    /// Azure Blob Storage integration is tested via integration tests, not unit tests,
    /// since the Azure SDK classes are not easily mockable.
    /// </summary>
    [TestClass]
    public class TranscriptServiceTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullConnectionString_CreatesServiceWithWarning()
        {
            // Act - Using connection string constructor with null
            var service = new TranscriptService(null, "transcripts");

            // Assert - Service should be created but will return errors when used
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_WithEmptyConnectionString_CreatesServiceWithWarning()
        {
            // Act
            var service = new TranscriptService(string.Empty, "transcripts");

            // Assert
            Assert.IsNotNull(service);
        }

        #endregion

        #region GetTranscriptAsync - Input Validation Tests

        [TestMethod]
        public async Task GetTranscriptAsync_NullMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetTranscriptAsync(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Message ID is required.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetTranscriptAsync_EmptyMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetTranscriptAsync(string.Empty);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Message ID is required.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetTranscriptAsync_WhitespaceMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetTranscriptAsync("   ");

            // Assert - whitespace passes IsNullOrEmpty check, so hits configuration error
            Assert.IsTrue(result.HasErrors);
        }

        #endregion

        #region GetTranscriptAsync - Unconfigured Service Tests

        [TestMethod]
        public async Task GetTranscriptAsync_ServiceNotConfigured_ReturnsConfigurationError()
        {
            // Arrange - Create service with null connection string
            var unconfiguredService = new TranscriptService(null, "transcripts");
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await unconfiguredService.GetTranscriptAsync(messageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        [TestMethod]
        public async Task GetTranscriptAsync_EmptyConnectionString_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(string.Empty, "transcripts");
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await service.GetTranscriptAsync(messageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        #endregion

        #region GetSermonNotesAsync - Input Validation Tests

        [TestMethod]
        public async Task GetSermonNotesAsync_NullMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetSermonNotesAsync(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Message ID is required.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_EmptyMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetSermonNotesAsync(string.Empty);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Message ID is required.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_WhitespaceMessageId_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetSermonNotesAsync("   ");

            // Assert - whitespace passes IsNullOrEmpty check, so hits configuration error
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        #endregion

        #region GetSermonNotesAsync - Unconfigured Service Tests

        [TestMethod]
        public async Task GetSermonNotesAsync_ServiceNotConfigured_ReturnsConfigurationError()
        {
            // Arrange - Create service with null connection string
            var unconfiguredService = new TranscriptService(null, "transcripts");
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await unconfiguredService.GetSermonNotesAsync(messageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_EmptyConnectionString_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(string.Empty, "transcripts");
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await service.GetSermonNotesAsync(messageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        #endregion

        #region GetStudyGuideAsync - Input Validation Tests

        [TestMethod]
        public async Task GetStudyGuideAsync_NullMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetStudyGuideAsync(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Message ID is required.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_EmptyMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetStudyGuideAsync(string.Empty);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Message ID is required.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_WhitespaceMessageId_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts");

            // Act
            var result = await service.GetStudyGuideAsync("   ");

            // Assert - whitespace passes IsNullOrEmpty check, so hits configuration error
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        #endregion

        #region GetStudyGuideAsync - Unconfigured Service Tests

        [TestMethod]
        public async Task GetStudyGuideAsync_ServiceNotConfigured_ReturnsConfigurationError()
        {
            // Arrange - Create service with null connection string
            var unconfiguredService = new TranscriptService(null, "transcripts");
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await unconfiguredService.GetStudyGuideAsync(messageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_EmptyConnectionString_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(string.Empty, "transcripts");
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await service.GetStudyGuideAsync(messageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        #endregion
    }
}

