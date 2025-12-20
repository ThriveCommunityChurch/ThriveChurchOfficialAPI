using Amazon.Lambda;
using Amazon.Lambda.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    [TestClass]
    public class PodcastLambdaServiceTests
    {
        private Mock<IAmazonLambda> _mockLambdaClient;
        private PodcastLambdaService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLambdaClient = new Mock<IAmazonLambda>();
            _service = new PodcastLambdaService(_mockLambdaClient.Object);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithMockClient_CreatesService()
        {
            // Act
            var service = new PodcastLambdaService(_mockLambdaClient.Object);

            // Assert
            Assert.IsNotNull(service);
        }

        #endregion

        #region RebuildFeedAsync Tests

        [TestMethod]
        public async Task RebuildFeedAsync_Success_ReturnsTrue()
        {
            // Arrange
            _mockLambdaClient.Setup(c => c.InvokeAsync(It.IsAny<InvokeRequest>(), default))
                .ReturnsAsync(new InvokeResponse { StatusCode = 202 });

            // Act
            var result = await _service.RebuildFeedAsync();

            // Assert
            Assert.IsTrue(result);
            _mockLambdaClient.Verify(c => c.InvokeAsync(
                It.Is<InvokeRequest>(r =>
                    r.FunctionName == "podcast-rss-generator-prod" &&
                    r.InvocationType == InvocationType.Event &&
                    r.Payload.Contains("rebuild")),
                default), Times.Once);
        }

        [TestMethod]
        public async Task RebuildFeedAsync_LambdaReturnsNon202_ReturnsFalse()
        {
            // Arrange
            _mockLambdaClient.Setup(c => c.InvokeAsync(It.IsAny<InvokeRequest>(), default))
                .ReturnsAsync(new InvokeResponse { StatusCode = 500 });

            // Act
            var result = await _service.RebuildFeedAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task RebuildFeedAsync_LambdaThrowsException_ReturnsFalse()
        {
            // Arrange
            _mockLambdaClient.Setup(c => c.InvokeAsync(It.IsAny<InvokeRequest>(), default))
                .ThrowsAsync(new AmazonLambdaException("Test exception"));

            // Act
            var result = await _service.RebuildFeedAsync();

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region UpsertEpisodeAsync Tests

        [TestMethod]
        public async Task UpsertEpisodeAsync_NullMessageId_ReturnsFalse()
        {
            // Act
            var result = await _service.UpsertEpisodeAsync(null);

            // Assert
            Assert.IsFalse(result);
            _mockLambdaClient.Verify(c => c.InvokeAsync(It.IsAny<InvokeRequest>(), default), Times.Never);
        }

        [TestMethod]
        public async Task UpsertEpisodeAsync_EmptyMessageId_ReturnsFalse()
        {
            // Act
            var result = await _service.UpsertEpisodeAsync(string.Empty);

            // Assert
            Assert.IsFalse(result);
            _mockLambdaClient.Verify(c => c.InvokeAsync(It.IsAny<InvokeRequest>(), default), Times.Never);
        }

        [TestMethod]
        public async Task UpsertEpisodeAsync_ValidMessageId_InvokesTranscriptionLambda()
        {
            // Arrange
            _mockLambdaClient.Setup(c => c.InvokeAsync(It.IsAny<InvokeRequest>(), default))
                .ReturnsAsync(new InvokeResponse { StatusCode = 202 });
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await _service.UpsertEpisodeAsync(messageId);

            // Assert
            Assert.IsTrue(result);
            _mockLambdaClient.Verify(c => c.InvokeAsync(
                It.Is<InvokeRequest>(r =>
                    r.FunctionName == "transcription-processor-prod" &&
                    r.InvocationType == InvocationType.Event &&
                    r.Payload.Contains(messageId)),
                default), Times.Once);
        }

        [TestMethod]
        public async Task UpsertEpisodeAsync_LambdaReturnsNon202_ReturnsFalse()
        {
            // Arrange
            _mockLambdaClient.Setup(c => c.InvokeAsync(It.IsAny<InvokeRequest>(), default))
                .ReturnsAsync(new InvokeResponse { StatusCode = 500 });
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await _service.UpsertEpisodeAsync(messageId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UpsertEpisodeAsync_LambdaThrowsException_ReturnsFalse()
        {
            // Arrange
            _mockLambdaClient.Setup(c => c.InvokeAsync(It.IsAny<InvokeRequest>(), default))
                .ThrowsAsync(new AmazonLambdaException("Test exception"));
            var messageId = "507f1f77bcf86cd799439011";

            // Act
            var result = await _service.UpsertEpisodeAsync(messageId);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}

