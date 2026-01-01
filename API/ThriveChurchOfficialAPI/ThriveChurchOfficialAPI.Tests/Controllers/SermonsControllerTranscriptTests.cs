using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Controllers;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Controllers
{
    /// <summary>
    /// Tests for SermonsController transcript-related endpoints (transcript, notes, study guide)
    /// Tests HTTP mapping behavior - business logic is tested in service tests
    /// </summary>
    [TestClass]
    public class SermonsControllerTranscriptTests
    {
        private Mock<ISermonsService> _mockSermonsService;
        private Mock<ITranscriptService> _mockTranscriptService;
        private SermonsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockSermonsService = new Mock<ISermonsService>();
            _mockTranscriptService = new Mock<ITranscriptService>();
            _controller = new SermonsController(_mockSermonsService.Object, _mockTranscriptService.Object);

            // Setup controller context
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region GetMessageTranscript Tests

        [TestMethod]
        public async Task GetMessageTranscript_ServiceReturnsSuccess_ReturnsOkWithTranscript()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            var transcriptResponse = CreateTestTranscriptResponse(messageId);
            _mockTranscriptService.Setup(s => s.GetTranscriptAsync(messageId))
                .ReturnsAsync(new SystemResponse<TranscriptResponse>(transcriptResponse, "Success!"));

            // Act
            var result = await _controller.GetMessageTranscript(messageId);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(messageId, result.Value.MessageId);
            Assert.AreEqual("Test Sermon", result.Value.Title);
        }

        [TestMethod]
        public async Task GetMessageTranscript_TranscriptNotFound_Returns404()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            _mockTranscriptService.Setup(s => s.GetTranscriptAsync(messageId))
                .ReturnsAsync(new SystemResponse<TranscriptResponse>(true, "Transcript not found for message ID: " + messageId));

            // Act
            var result = await _controller.GetMessageTranscript(messageId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(404, statusResult.StatusCode);
        }

        [TestMethod]
        public async Task GetMessageTranscript_ServiceError_Returns400()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            _mockTranscriptService.Setup(s => s.GetTranscriptAsync(messageId))
                .ReturnsAsync(new SystemResponse<TranscriptResponse>(true, "Service error occurred"));

            // Act
            var result = await _controller.GetMessageTranscript(messageId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(400, statusResult.StatusCode);
        }

        #endregion

        #region GetSermonNotes Tests

        [TestMethod]
        public async Task GetSermonNotes_ServiceReturnsSuccess_ReturnsOkWithNotes()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            var notesResponse = CreateTestSermonNotesResponse();
            _mockTranscriptService.Setup(s => s.GetSermonNotesAsync(messageId))
                .ReturnsAsync(new SystemResponse<SermonNotesResponse>(notesResponse, "Success!"));

            // Act
            var result = await _controller.GetSermonNotes(messageId);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Test Sermon", result.Value.Title);
            Assert.AreEqual("Pastor John", result.Value.Speaker);
            Assert.AreEqual("John 3:16", result.Value.MainScripture);
        }

        [TestMethod]
        public async Task GetSermonNotes_NotFound_Returns404()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            _mockTranscriptService.Setup(s => s.GetSermonNotesAsync(messageId))
                .ReturnsAsync(new SystemResponse<SermonNotesResponse>(true, "Transcript not found for message ID: " + messageId));

            // Act
            var result = await _controller.GetSermonNotes(messageId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(404, statusResult.StatusCode);
        }

        [TestMethod]
        public async Task GetSermonNotes_NotYetGenerated_Returns404()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            _mockTranscriptService.Setup(s => s.GetSermonNotesAsync(messageId))
                .ReturnsAsync(new SystemResponse<SermonNotesResponse>(true, "Sermon notes not yet generated for message ID: " + messageId));

            // Act
            var result = await _controller.GetSermonNotes(messageId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(404, statusResult.StatusCode);
        }

        [TestMethod]
        public async Task GetSermonNotes_ServiceError_Returns400()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            _mockTranscriptService.Setup(s => s.GetSermonNotesAsync(messageId))
                .ReturnsAsync(new SystemResponse<SermonNotesResponse>(true, "Service error occurred"));

            // Act
            var result = await _controller.GetSermonNotes(messageId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(400, statusResult.StatusCode);
        }

        #endregion

        #region GetStudyGuide Tests

        [TestMethod]
        public async Task GetStudyGuide_ServiceReturnsSuccess_ReturnsOkWithStudyGuide()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            var studyGuideResponse = CreateTestStudyGuideResponse();
            _mockTranscriptService.Setup(s => s.GetStudyGuideAsync(messageId))
                .ReturnsAsync(new SystemResponse<StudyGuideResponse>(studyGuideResponse, "Success!"));

            // Act
            var result = await _controller.GetStudyGuide(messageId);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Test Sermon", result.Value.Title);
            Assert.AreEqual("Pastor John", result.Value.Speaker);
            Assert.AreEqual("45 minutes", result.Value.EstimatedStudyTime);
        }

        [TestMethod]
        public async Task GetStudyGuide_NotFound_Returns404()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            _mockTranscriptService.Setup(s => s.GetStudyGuideAsync(messageId))
                .ReturnsAsync(new SystemResponse<StudyGuideResponse>(true, "Transcript not found for message ID: " + messageId));

            // Act
            var result = await _controller.GetStudyGuide(messageId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(404, statusResult.StatusCode);
        }

        [TestMethod]
        public async Task GetStudyGuide_NotYetGenerated_Returns404()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            _mockTranscriptService.Setup(s => s.GetStudyGuideAsync(messageId))
                .ReturnsAsync(new SystemResponse<StudyGuideResponse>(true, "Study guide not yet generated for message ID: " + messageId));

            // Act
            var result = await _controller.GetStudyGuide(messageId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(404, statusResult.StatusCode);
        }

        [TestMethod]
        public async Task GetStudyGuide_ServiceError_Returns400()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            _mockTranscriptService.Setup(s => s.GetStudyGuideAsync(messageId))
                .ReturnsAsync(new SystemResponse<StudyGuideResponse>(true, "Service error occurred"));

            // Act
            var result = await _controller.GetStudyGuide(messageId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(400, statusResult.StatusCode);
        }

        #endregion

        #region Helper Methods

        private TranscriptResponse CreateTestTranscriptResponse(string messageId)
        {
            return new TranscriptResponse
            {
                MessageId = messageId,
                Title = "Test Sermon",
                Speaker = "Pastor John",
                FullText = "This is the full transcript text...",
                WordCount = 5000
            };
        }

        private SermonNotesResponse CreateTestSermonNotesResponse()
        {
            return new SermonNotesResponse
            {
                Title = "Test Sermon",
                Speaker = "Pastor John",
                Date = "2024-01-01",
                MainScripture = "John 3:16",
                Summary = "A sermon about God's love",
                KeyPoints = new List<KeyPointResponse>
                {
                    new KeyPointResponse { Point = "God loves us", Scripture = "John 3:16" }
                },
                ApplicationPoints = new List<string> { "Love others as God loves you" },
                GeneratedAt = "2024-01-01T12:00:00Z",
                ModelUsed = "gpt-4",
                WordCount = 5000
            };
        }

        private StudyGuideResponse CreateTestStudyGuideResponse()
        {
            return new StudyGuideResponse
            {
                Title = "Test Sermon",
                Speaker = "Pastor John",
                Date = "2024-01-01",
                MainScripture = "John 3:16",
                Summary = "A sermon about God's love",
                KeyPoints = new List<KeyPointResponse>
                {
                    new KeyPointResponse { Point = "God loves us", Scripture = "John 3:16" }
                },
                DiscussionQuestions = new DiscussionQuestionsResponse
                {
                    Icebreaker = new List<string> { "What does love mean to you?" },
                    Reflection = new List<string> { "How has God shown love in your life?" },
                    Application = new List<string> { "How can you show love this week?" }
                },
                PrayerPrompts = new List<string> { "Thank God for His love" },
                TakeHomeChallenges = new List<string> { "Show love to someone this week" },
                EstimatedStudyTime = "45 minutes",
                GeneratedAt = "2024-01-01T12:00:00Z",
                ModelUsed = "gpt-4"
            };
        }

        #endregion
    }
}

