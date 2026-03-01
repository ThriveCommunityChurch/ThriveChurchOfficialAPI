using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    /// <summary>
    /// Tests for TranscriptService - comprehensive coverage including cache scenarios,
    /// mapping methods, and exception handlers using Azure SDK mocking.
    /// </summary>
    [TestClass]
    public class TranscriptServiceTests
    {
        private Mock<ICacheService> _mockCache;
        private Mock<BlobContainerClient> _mockContainerClient;
        private Mock<BlobClient> _mockBlobClient;
        private const string TestMessageId = "507f1f77bcf86cd799439011";

        [TestInitialize]
        public void Setup()
        {
            _mockCache = new Mock<ICacheService>();

            // Azure SDK classes have protected parameterless constructors for mocking
            // Per Azure SDK Design Guidelines, all public methods are virtual
            // Use MockBehavior.Loose (default) for flexibility
            _mockContainerClient = new Mock<BlobContainerClient>(MockBehavior.Loose);
            _mockBlobClient = new Mock<BlobClient>(MockBehavior.Loose);

            // Default setup: container returns the mock blob client
            _mockContainerClient
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullConnectionString_CreatesServiceWithWarning()
        {
            // Act - Using connection string constructor with null
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

            // Assert - Service should be created but will return errors when used
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_WithEmptyConnectionString_CreatesServiceWithWarning()
        {
            // Act
            var service = new TranscriptService(string.Empty, "transcripts", _mockCache.Object);

            // Assert
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_WithNullCache_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
                new TranscriptService(null, "transcripts", null));
        }

        [TestMethod]
        public void Constructor_WithBlobClient_NullCache_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
                new TranscriptService(_mockContainerClient.Object, null));
        }

        [TestMethod]
        public void Constructor_WithBlobClient_ValidParams_CreatesService()
        {
            // Act
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Assert
            Assert.IsNotNull(service);
        }

        #endregion

        #region GetTranscriptAsync - Input Validation Tests

        [TestMethod]
        public async Task GetTranscriptAsync_NullMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

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
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(string.Empty);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Message ID is required.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetTranscriptAsync_WhitespaceMessageId_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

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
            var unconfiguredService = new TranscriptService(null, "transcripts", _mockCache.Object);

            // Act
            var result = await unconfiguredService.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        [TestMethod]
        public async Task GetTranscriptAsync_EmptyConnectionString_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(string.Empty, "transcripts", _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        #endregion

        #region GetTranscriptAsync - Cache Hit Tests

        [TestMethod]
        public async Task GetTranscriptAsync_CacheHit_ReturnsTranscriptFromCache()
        {
            // Arrange
            var cachedBlob = CreateFullTranscriptBlob();
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(TestMessageId, result.Result.MessageId);
            Assert.AreEqual("Test Title", result.Result.Title);
            Assert.AreEqual("Test Speaker", result.Result.Speaker);
            Assert.AreEqual("Test transcript text", result.Result.FullText);
            Assert.AreEqual(100, result.Result.WordCount);

            // Verify cache was checked
            _mockCache.Verify(c => c.ReadFromCache<TranscriptBlob>(It.IsAny<string>()), Times.Once);
            // Verify blob storage was NOT accessed
            _mockBlobClient.Verify(b => b.ExistsAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetTranscriptAsync_CacheHit_MapsNotesCorrectly()
        {
            // Arrange
            var cachedBlob = CreateFullTranscriptBlob();
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            var notes = result.Result.Notes;
            Assert.IsNotNull(notes);
            Assert.AreEqual("Note Title", notes.Title);
            Assert.AreEqual("Note Speaker", notes.Speaker);
            Assert.AreEqual("2024-01-01", notes.Date);
            Assert.AreEqual("John 3:16", notes.MainScripture);
            Assert.AreEqual("Summary text", notes.Summary);
            Assert.IsNotNull(notes.KeyPoints);
            Assert.AreEqual(1, notes.KeyPoints.Count);
            Assert.AreEqual("Key point 1", notes.KeyPoints[0].Point);
            Assert.AreEqual("Romans 8:28", notes.KeyPoints[0].Scripture);
            Assert.IsNotNull(notes.Quotes);
            Assert.AreEqual(1, notes.Quotes.Count);
            Assert.AreEqual("Quote text", notes.Quotes[0].Text);
            Assert.IsNotNull(notes.ApplicationPoints);
            Assert.AreEqual(1, notes.ApplicationPoints.Count);
        }

        [TestMethod]
        public async Task GetTranscriptAsync_CacheHit_MapsStudyGuideCorrectly()
        {
            // Arrange
            var cachedBlob = CreateFullTranscriptBlob();
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            var guide = result.Result.StudyGuide;
            Assert.IsNotNull(guide);
            Assert.AreEqual("Study Guide Title", guide.Title);
            Assert.AreEqual("Study Speaker", guide.Speaker);
            Assert.IsNotNull(guide.ScriptureReferences);
            Assert.AreEqual(1, guide.ScriptureReferences.Count);
            Assert.AreEqual("Matthew 5:1", guide.ScriptureReferences[0].Reference);
            Assert.IsNotNull(guide.DiscussionQuestions);
            Assert.IsNotNull(guide.DiscussionQuestions.Icebreaker);
            Assert.AreEqual(1, guide.DiscussionQuestions.Icebreaker.Count);
            Assert.IsNotNull(guide.Illustrations);
            Assert.AreEqual(1, guide.Illustrations.Count);
            Assert.IsNotNull(guide.AdditionalStudy);
            Assert.AreEqual(1, guide.AdditionalStudy.Count);
            Assert.IsNotNull(guide.Confidence);
            Assert.AreEqual("High", guide.Confidence.ScriptureAccuracy);
        }

        [TestMethod]
        public async Task GetTranscriptAsync_CacheHit_WithNullNotes_ReturnsNullNotes()
        {
            // Arrange
            var cachedBlob = new TranscriptBlob
            {
                MessageId = TestMessageId,
                Title = "Test Title",
                Speaker = "Test Speaker",
                Transcript = "Test transcript",
                WordCount = 50,
                Notes = null,
                StudyGuide = null
            };
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNull(result.Result.Notes);
            Assert.IsNull(result.Result.StudyGuide);
        }

        [TestMethod]
        public async Task GetTranscriptAsync_CacheHit_WithNullKeyPoints_MapsCorrectly()
        {
            // Arrange
            var cachedBlob = new TranscriptBlob
            {
                MessageId = TestMessageId,
                Title = "Test Title",
                Speaker = "Test Speaker",
                Transcript = "Test transcript",
                WordCount = 50,
                Notes = new SermonNotesBlob
                {
                    Title = "Notes",
                    KeyPoints = null,
                    Quotes = null,
                    ApplicationPoints = null
                }
            };
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result.Notes);
            Assert.IsNull(result.Result.Notes.KeyPoints);
            Assert.IsNull(result.Result.Notes.Quotes);
        }

        [TestMethod]
        public async Task GetTranscriptAsync_CacheHit_StudyGuideWithNullCollections_MapsCorrectly()
        {
            // Arrange
            var cachedBlob = new TranscriptBlob
            {
                MessageId = TestMessageId,
                Title = "Test Title",
                Speaker = "Test Speaker",
                Transcript = "Test transcript",
                WordCount = 50,
                StudyGuide = new StudyGuideBlob
                {
                    Title = "Guide",
                    KeyPoints = null,
                    ScriptureReferences = null,
                    DiscussionQuestions = null,
                    Illustrations = null,
                    AdditionalStudy = null,
                    Confidence = null,
                    PrayerPrompts = null,
                    TakeHomeChallenges = null
                }
            };
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            var guide = result.Result.StudyGuide;
            Assert.IsNotNull(guide);
            Assert.IsNull(guide.KeyPoints);
            Assert.IsNull(guide.ScriptureReferences);
            Assert.IsNull(guide.DiscussionQuestions);
            Assert.IsNull(guide.Illustrations);
            Assert.IsNull(guide.AdditionalStudy);
            Assert.IsNull(guide.Confidence);
        }

        #endregion

        #region GetTranscriptAsync - Cache Miss / Blob Storage Tests

        [TestMethod]
        public async Task GetTranscriptAsync_CacheMiss_BlobNotFound_ReturnsNotFoundError()
        {
            // Arrange
            SetupCacheMiss();
            SetupBlobNotExists();
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Transcript not found"));
            Assert.IsTrue(result.ErrorMessage.Contains(TestMessageId));
        }

        [TestMethod]
        public async Task GetTranscriptAsync_CacheMiss_BlobExists_ReturnsAndCachesTranscript()
        {
            // Arrange
            var blob = CreateFullTranscriptBlob();
            SetupCacheMiss();
            SetupBlobExists(blob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors, $"Expected no errors but got: {result.ErrorMessage}");
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("Test Title", result.Result.Title);

            // Verify blob was cached
            _mockCache.Verify(c => c.InsertIntoCache(
                It.IsAny<string>(),
                It.IsAny<TranscriptBlob>(),
                It.IsAny<TimeSpan>()), Times.Once);
        }

        #endregion

        #region GetSermonNotesAsync - Input Validation Tests

        [TestMethod]
        public async Task GetSermonNotesAsync_NullMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

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
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

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
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

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
            // Arrange
            var unconfiguredService = new TranscriptService(null, "transcripts", _mockCache.Object);

            // Act
            var result = await unconfiguredService.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_EmptyConnectionString_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(string.Empty, "transcripts", _mockCache.Object);

            // Act
            var result = await service.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        #endregion

        #region GetSermonNotesAsync - Cache Hit Tests

        [TestMethod]
        public async Task GetSermonNotesAsync_CacheHit_ReturnsNotesFromCache()
        {
            // Arrange
            var cachedBlob = CreateFullTranscriptBlob();
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("Note Title", result.Result.Title);
            Assert.AreEqual("Note Speaker", result.Result.Speaker);
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_CacheHit_NotesNotGenerated_ReturnsError()
        {
            // Arrange
            var cachedBlob = new TranscriptBlob
            {
                MessageId = TestMessageId,
                Title = "Test",
                Notes = null
            };
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not yet generated"));
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_CacheMiss_BlobNotFound_ReturnsNotFoundError()
        {
            // Arrange
            SetupCacheMiss();
            SetupBlobNotExists();
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Transcript not found"));
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_CacheMiss_BlobExists_ReturnsNotes()
        {
            // Arrange
            var blob = CreateFullTranscriptBlob();
            SetupCacheMiss();
            SetupBlobExists(blob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("Note Title", result.Result.Title);
        }

        #endregion

        #region GetStudyGuideAsync - Input Validation Tests

        [TestMethod]
        public async Task GetStudyGuideAsync_NullMessageId_ReturnsError()
        {
            // Arrange
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

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
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

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
            var service = new TranscriptService(null, "transcripts", _mockCache.Object);

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
            // Arrange
            var unconfiguredService = new TranscriptService(null, "transcripts", _mockCache.Object);

            // Act
            var result = await unconfiguredService.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_EmptyConnectionString_ReturnsConfigurationError()
        {
            // Arrange
            var service = new TranscriptService(string.Empty, "transcripts", _mockCache.Object);

            // Act
            var result = await service.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not configured"));
        }

        #endregion

        #region GetStudyGuideAsync - Cache Hit Tests

        [TestMethod]
        public async Task GetStudyGuideAsync_CacheHit_ReturnsStudyGuideFromCache()
        {
            // Arrange
            var cachedBlob = CreateFullTranscriptBlob();
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("Study Guide Title", result.Result.Title);
            Assert.AreEqual("Study Speaker", result.Result.Speaker);
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_CacheHit_StudyGuideNotGenerated_ReturnsError()
        {
            // Arrange
            var cachedBlob = new TranscriptBlob
            {
                MessageId = TestMessageId,
                Title = "Test",
                StudyGuide = null
            };
            SetupCacheHit(cachedBlob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not yet generated"));
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_CacheMiss_BlobNotFound_ReturnsNotFoundError()
        {
            // Arrange
            SetupCacheMiss();
            SetupBlobNotExists();
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Transcript not found"));
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_CacheMiss_BlobExists_ReturnsStudyGuide()
        {
            // Arrange
            var blob = CreateFullTranscriptBlob();
            SetupCacheMiss();
            SetupBlobExists(blob);
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("Study Guide Title", result.Result.Title);
        }

        #endregion

        #region Exception Handling Tests

        [TestMethod]
        public async Task GetTranscriptAsync_AzureRequestFailedException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("Blob storage error"));
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Failed to retrieve transcript"));
        }

        [TestMethod]
        public async Task GetTranscriptAsync_JsonException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            SetupBlobExistsWithInvalidJson();
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Failed to parse transcript data"));
        }

        [TestMethod]
        public async Task GetTranscriptAsync_GeneralException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetTranscriptAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("unexpected error"));
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_AzureRequestFailedException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("Blob storage error"));
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Failed to retrieve sermon notes"));
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_JsonException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            SetupBlobExistsWithInvalidJson();
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Failed to parse sermon notes data"));
        }

        [TestMethod]
        public async Task GetSermonNotesAsync_GeneralException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetSermonNotesAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("unexpected error"));
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_AzureRequestFailedException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("Blob storage error"));
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Failed to retrieve study guide"));
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_JsonException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            SetupBlobExistsWithInvalidJson();
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Failed to parse study guide data"));
        }

        [TestMethod]
        public async Task GetStudyGuideAsync_GeneralException_ReturnsError()
        {
            // Arrange
            SetupCacheMiss();
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));
            var service = new TranscriptService(_mockContainerClient.Object, _mockCache.Object);

            // Act
            var result = await service.GetStudyGuideAsync(TestMessageId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("unexpected error"));
        }

        #endregion

        #region Helper Methods

        private void SetupCacheHit(TranscriptBlob blob)
        {
            _mockCache
                .Setup(c => c.ReadFromCache<TranscriptBlob>(It.IsAny<string>()))
                .Returns(blob);
        }

        private void SetupCacheMiss()
        {
            _mockCache
                .Setup(c => c.ReadFromCache<TranscriptBlob>(It.IsAny<string>()))
                .Returns((TranscriptBlob)null);
        }

        private void SetupBlobNotExists()
        {
            var response = Response.FromValue(false, Mock.Of<Response>());
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
        }

        private void SetupBlobExists(TranscriptBlob blob)
        {
            var existsResponse = Response.FromValue(true, Mock.Of<Response>());
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(existsResponse);

            var json = JsonConvert.SerializeObject(blob);
            var binaryData = BinaryData.FromString(json);
            var downloadResult = BlobsModelFactory.BlobDownloadResult(content: binaryData);

            // Mock both the parameterless version and version with CancellationToken
            // The service calls the parameterless DownloadContentAsync()
            _mockBlobClient
                .Setup(b => b.DownloadContentAsync())
                .ReturnsAsync(Response.FromValue(downloadResult, Mock.Of<Response>()));
            _mockBlobClient
                .Setup(b => b.DownloadContentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(downloadResult, Mock.Of<Response>()));
        }

        private void SetupBlobExistsWithInvalidJson()
        {
            var existsResponse = Response.FromValue(true, Mock.Of<Response>());
            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(existsResponse);

            // Return invalid JSON that will cause deserialization to throw
            var binaryData = BinaryData.FromString("{ invalid json }");
            var downloadResult = BlobsModelFactory.BlobDownloadResult(content: binaryData);

            // Mock both the parameterless version and version with CancellationToken
            _mockBlobClient
                .Setup(b => b.DownloadContentAsync())
                .ReturnsAsync(Response.FromValue(downloadResult, Mock.Of<Response>()));
            _mockBlobClient
                .Setup(b => b.DownloadContentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(downloadResult, Mock.Of<Response>()));
        }

        private TranscriptBlob CreateFullTranscriptBlob()
        {
            return new TranscriptBlob
            {
                MessageId = TestMessageId,
                Title = "Test Title",
                Speaker = "Test Speaker",
                Transcript = "Test transcript text",
                WordCount = 100,
                UploadedAt = "2024-01-01",
                Notes = new SermonNotesBlob
                {
                    Title = "Note Title",
                    Speaker = "Note Speaker",
                    Date = "2024-01-01",
                    MainScripture = "John 3:16",
                    Summary = "Summary text",
                    KeyPoints = new List<KeyPointBlob>
                    {
                        new KeyPointBlob
                        {
                            Point = "Key point 1",
                            Scripture = "Romans 8:28",
                            Detail = "Detail text",
                            TheologicalContext = "Context text",
                            DirectlyQuoted = true
                        }
                    },
                    Quotes = new List<QuoteBlob>
                    {
                        new QuoteBlob { Text = "Quote text", Context = "Quote context" }
                    },
                    ApplicationPoints = new List<string> { "Apply this" },
                    GeneratedAt = "2024-01-01T12:00:00Z",
                    ModelUsed = "gpt-4",
                    WordCount = 50
                },
                StudyGuide = new StudyGuideBlob
                {
                    Title = "Study Guide Title",
                    Speaker = "Study Speaker",
                    Date = "2024-01-01",
                    MainScripture = "John 3:16",
                    Summary = "Study summary",
                    KeyPoints = new List<KeyPointBlob>
                    {
                        new KeyPointBlob { Point = "Study point", Scripture = "Genesis 1:1" }
                    },
                    ScriptureReferences = new List<ScriptureReferenceBlob>
                    {
                        new ScriptureReferenceBlob
                        {
                            Reference = "Matthew 5:1",
                            Context = "Sermon context",
                            DirectlyQuoted = true
                        }
                    },
                    DiscussionQuestions = new DiscussionQuestionsBlob
                    {
                        Icebreaker = new List<string> { "How are you?" },
                        Reflection = new List<string> { "What did you learn?" },
                        Application = new List<string> { "How will you apply this?" },
                        ForLeaders = new List<string> { "Leader question" }
                    },
                    Illustrations = new List<IllustrationBlob>
                    {
                        new IllustrationBlob { Summary = "Illustration", Point = "Main point" }
                    },
                    PrayerPrompts = new List<string> { "Pray for this" },
                    TakeHomeChallenges = new List<string> { "Challenge 1" },
                    Devotional = "Devotional text",
                    AdditionalStudy = new List<AdditionalStudyBlob>
                    {
                        new AdditionalStudyBlob
                        {
                            Topic = "Topic",
                            Scriptures = new List<string> { "Romans 1:1" },
                            Note = "Study note"
                        }
                    },
                    EstimatedStudyTime = "30 minutes",
                    GeneratedAt = "2024-01-01T12:00:00Z",
                    ModelUsed = "gpt-4",
                    Confidence = new ConfidenceBlob
                    {
                        ScriptureAccuracy = "High",
                        ContentCoverage = "Complete"
                    }
                }
            };
        }

        #endregion
    }
}

