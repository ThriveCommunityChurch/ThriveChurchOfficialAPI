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
    public class SermonsServiceTests
    {
        private Mock<ISermonsRepository> _mockSermonsRepository;
        private Mock<IMessagesRepository> _mockMessagesRepository;
        private Mock<ICacheService> _mockCache;
        private Mock<IS3Repository> _mockS3Repository;
        private Mock<IPodcastLambdaService> _mockPodcastLambdaService;
        private Mock<IPodcastMessagesRepository> _mockPodcastMessagesRepository;
        private SermonsService _sermonsService;

        [TestInitialize]
        public void Setup()
        {
            _mockSermonsRepository = new Mock<ISermonsRepository>();
            _mockMessagesRepository = new Mock<IMessagesRepository>();
            _mockCache = new Mock<ICacheService>();
            _mockS3Repository = new Mock<IS3Repository>();
            _mockPodcastLambdaService = new Mock<IPodcastLambdaService>();
            _mockPodcastMessagesRepository = new Mock<IPodcastMessagesRepository>();

            // Setup cache miss by default
            _mockCache.Setup(c => c.CanReadFromCache(It.IsAny<string>())).Returns(false);
            _mockCache.Setup(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
                .Returns((string key, object item, TimeSpan exp) => item);

            _sermonsService = new SermonsService(
                _mockSermonsRepository.Object,
                _mockMessagesRepository.Object,
                _mockCache.Object,
                _mockS3Repository.Object,
                _mockPodcastLambdaService.Object,
                _mockPodcastMessagesRepository.Object
            );
        }

        #region ExportAllSermonData Tests

        [TestMethod]
        public async Task ExportAllSermonData_NoSeries_ReturnsEmptyArray()
        {
            // Arrange
            var emptySeries = new List<SermonSeries>();
            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(emptySeries);

            // Act
            var result = await _sermonsService.ExportAllSermonData();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(0, result.Result.TotalSeries);
            Assert.AreEqual(0, result.Result.TotalMessages);
            Assert.IsNotNull(result.Result.Series);
            Assert.AreEqual(0, result.Result.Series.Count());
        }

        [TestMethod]
        public async Task ExportAllSermonData_MultipleSeries_IncludesAllSeries()
        {
            // Arrange
            var series1 = CreateTestSermonSeries("1", "Series 1", "series-1");
            var series2 = CreateTestSermonSeries("2", "Series 2", "series-2");
            var seriesList = new List<SermonSeries> { series1, series2 };

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(seriesList);

            _mockMessagesRepository.Setup(r => r.GetMessagesBySeriesId("1"))
                .ReturnsAsync(new List<SermonMessage>());
            _mockMessagesRepository.Setup(r => r.GetMessagesBySeriesId("2"))
                .ReturnsAsync(new List<SermonMessage>());

            // Act
            var result = await _sermonsService.ExportAllSermonData();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(2, result.Result.TotalSeries);
            Assert.AreEqual(2, result.Result.Series.Count());
        }

        [TestMethod]
        public async Task ExportAllSermonData_WithMessages_IncludesAllMessagesWithAllProperties()
        {
            // Arrange
            var series = CreateTestSermonSeries("1", "Test Series", "test-series");
            var message1 = CreateTestSermonMessage("msg1", "1", "Message 1");
            var message2 = CreateTestSermonMessage("msg2", "1", "Message 2");
            var messages = new List<SermonMessage> { message1, message2 };

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(new List<SermonSeries> { series });

            _mockMessagesRepository.Setup(r => r.GetMessagesBySeriesId("1"))
                .ReturnsAsync(messages);

            // Act
            var result = await _sermonsService.ExportAllSermonData();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalSeries);
            Assert.AreEqual(2, result.Result.TotalMessages);
            
            var exportedSeries = result.Result.Series.First();
            Assert.AreEqual(2, exportedSeries.Messages.Count());
            
            // Verify all message properties are included
            var exportedMessage = exportedSeries.Messages.First();
            Assert.IsNotNull(exportedMessage.MessageId);
            Assert.IsNotNull(exportedMessage.AudioUrl);
            Assert.IsTrue(exportedMessage.AudioDuration > 0);
            Assert.IsNotNull(exportedMessage.Speaker);
            Assert.IsNotNull(exportedMessage.Title);
        }

        [TestMethod]
        public async Task ExportAllSermonData_IncludesCorrectMetadata()
        {
            // Arrange
            var series = CreateTestSermonSeries("1", "Test Series", "test-series");
            var message = CreateTestSermonMessage("msg1", "1", "Message 1");

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(new List<SermonSeries> { series });

            _mockMessagesRepository.Setup(r => r.GetMessagesBySeriesId("1"))
                .ReturnsAsync(new List<SermonMessage> { message });

            var beforeExport = DateTime.UtcNow;

            // Act
            var result = await _sermonsService.ExportAllSermonData();

            var afterExport = DateTime.UtcNow;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalSeries);
            Assert.AreEqual(1, result.Result.TotalMessages);
            Assert.IsTrue(result.Result.ExportDate >= beforeExport && result.Result.ExportDate <= afterExport);
        }

        [TestMethod]
        public async Task ExportAllSermonData_RepositoryReturnsNull_HandlesGracefully()
        {
            // Arrange
            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync((IEnumerable<SermonSeries>)null);

            // Act
            var result = await _sermonsService.ExportAllSermonData();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        #endregion

        #region ImportSermonData - Validation Tests

        [TestMethod]
        public async Task ImportSermonData_NullRequest_ReturnsValidationError()
        {
            // Act
            var result = await _sermonsService.ImportSermonData(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task ImportSermonData_EmptySeriesArray_ReturnsValidationError()
        {
            // Arrange
            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>()
            };

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task ImportSermonData_InvalidSeriesIdFormat_ReturnsValidationError()
        {
            // Arrange
            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = "invalid-id",
                        Name = "Test Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "test-series"
                    }
                }
            };

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task ImportSermonData_InvalidMessageIdFormat_ReturnsValidationError()
        {
            // Arrange
            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = "507f1f77bcf86cd799439011",
                        Name = "Test Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "test-series",
                        Messages = new List<SermonMessageResponse>
                        {
                            new SermonMessageResponse
                            {
                                MessageId = "invalid-message-id",
                                Speaker = "Test Speaker",
                                Title = "Test Message",
                                Date = DateTime.UtcNow,
                                AudioDuration = 1800
                            }
                        }
                    }
                }
            };

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task ImportSermonData_MissingRequiredFields_ReturnsValidationError()
        {
            // Arrange - Series missing Name
            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = "507f1f77bcf86cd799439011",
                        Name = null, // Missing required field
                        StartDate = DateTime.UtcNow,
                        Slug = "test-series"
                    }
                }
            };

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        #endregion

        #region ImportSermonData - Update Logic Tests

        [TestMethod]
        public async Task ImportSermonData_ValidData_UpdatesExistingSeries()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
            var existingSeries = CreateTestSermonSeries(seriesId, "Old Name", "old-slug");

            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = seriesId,
                        Name = "Updated Name",
                        Year = "2024",
                        StartDate = new DateTime(2024, 1, 1),
                        EndDate = new DateTime(2024, 12, 31),
                        Slug = "updated-slug",
                        Thumbnail = "https://example.com/new-thumbnail.jpg",
                        ArtUrl = "https://example.com/new-art.jpg",
                        Summary = "Updated summary",
                        Messages = new List<SermonMessageResponse>()
                    }
                }
            };

            _mockSermonsRepository.Setup(r => r.GetSermonSeriesForId(seriesId))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockSermonsRepository.Setup(r => r.UpdateSermonSeries(It.IsAny<SermonSeries>()))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalSeriesProcessed);
            Assert.AreEqual(1, result.Result.TotalSeriesUpdated);
            Assert.AreEqual(0, result.Result.TotalSeriesSkipped);

            // Verify UpdateSermonSeries was called with updated properties (except Id)
            _mockSermonsRepository.Verify(r => r.UpdateSermonSeries(
                It.Is<SermonSeries>(s =>
                    s.Id == seriesId && // ID should remain unchanged
                    s.Name == "Updated Name" &&
                    s.Slug == "updated-slug" &&
                    s.Summary == "Updated summary"
                )), Times.Once);
        }

        [TestMethod]
        public async Task ImportSermonData_ValidData_UpdatesExistingMessages()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
            var messageId = "507f1f77bcf86cd799439012";
            var existingSeries = CreateTestSermonSeries(seriesId, "Test Series", "test-series");
            var existingMessage = CreateTestSermonMessage(messageId, seriesId, "Old Title");

            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = seriesId,
                        Name = "Test Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "test-series",
                        Messages = new List<SermonMessageResponse>
                        {
                            new SermonMessageResponse
                            {
                                MessageId = messageId,
                                Title = "Updated Title",
                                Speaker = "Updated Speaker",
                                Date = DateTime.UtcNow,
                                AudioUrl = "https://example.com/updated-audio.mp3",
                                AudioDuration = 2400,
                                AudioFileSize = 15000000,
                                VideoUrl = "https://example.com/updated-video.mp4",
                                PassageRef = "Romans 8:28",
                                Summary = "Updated message summary",
                                Tags = new List<MessageTag> { MessageTag.Prayer }
                            }
                        }
                    }
                }
            };

            _mockSermonsRepository.Setup(r => r.GetSermonSeriesForId(seriesId))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockSermonsRepository.Setup(r => r.UpdateSermonSeries(It.IsAny<SermonSeries>()))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockMessagesRepository.Setup(r => r.GetMessageById(messageId))
                .ReturnsAsync(new SystemResponse<SermonMessage>(existingMessage, "Success"));

            _mockMessagesRepository.Setup(r => r.UpdateMessageById(messageId, It.IsAny<SermonMessageRequest>()))
                .ReturnsAsync(existingMessage);

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalMessagesProcessed);
            Assert.AreEqual(1, result.Result.TotalMessagesUpdated);
            Assert.AreEqual(0, result.Result.TotalMessagesSkipped);

            // Verify UpdateMessageById was called with updated properties (except MessageId)
            _mockMessagesRepository.Verify(r => r.UpdateMessageById(
                messageId, // MessageId used for matching only
                It.Is<SermonMessageRequest>(m =>
                    m.Title == "Updated Title" &&
                    m.Speaker == "Updated Speaker" &&
                    m.AudioUrl == "https://example.com/updated-audio.mp3" &&
                    m.AudioDuration == 2400 &&
                    m.PassageRef == "Romans 8:28"
                )), Times.Once);
        }

        [TestMethod]
        public async Task ImportSermonData_NonExistentSeriesId_InsertsNewSeries()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
            var newSeries = CreateTestSermonSeries(seriesId, "Test Series", "test-series");

            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = seriesId,
                        Name = "Test Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "test-series",
                        Messages = new List<SermonMessageResponse>()
                    }
                }
            };

            _mockSermonsRepository.Setup(r => r.GetSermonSeriesForId(seriesId))
                .ReturnsAsync(new SystemResponse<SermonSeries>(true, "Series not found"));

            _mockSermonsRepository.Setup(r => r.CreateNewSermonSeries(It.IsAny<SermonSeries>()))
                .ReturnsAsync(new SystemResponse<SermonSeries>(newSeries, "Success"));

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalSeriesProcessed);
            Assert.AreEqual(1, result.Result.TotalSeriesUpdated); // Upserted (inserted)
            Assert.AreEqual(0, result.Result.TotalSeriesSkipped);
            Assert.AreEqual(0, result.Result.SkippedItems.Count());

            // Verify CreateNewSermonSeries was called
            _mockSermonsRepository.Verify(r => r.CreateNewSermonSeries(It.Is<SermonSeries>(s =>
                s.Id == seriesId &&
                s.Name == "Test Series" &&
                s.Slug == "test-series"
            )), Times.Once);
        }

        [TestMethod]
        public async Task ImportSermonData_NonExistentMessageId_InsertsNewMessage()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
            var messageId = "507f1f77bcf86cd799439012";
            var existingSeries = CreateTestSermonSeries(seriesId, "Test Series", "test-series");
            var newMessage = CreateTestSermonMessage(messageId, seriesId, "Test Message");

            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = seriesId,
                        Name = "Test Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "test-series",
                        Messages = new List<SermonMessageResponse>
                        {
                            new SermonMessageResponse
                            {
                                MessageId = messageId,
                                Title = "Test Message",
                                Speaker = "Test Speaker",
                                Date = DateTime.UtcNow,
                                AudioDuration = 1800
                            }
                        }
                    }
                }
            };

            _mockSermonsRepository.Setup(r => r.GetSermonSeriesForId(seriesId))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockSermonsRepository.Setup(r => r.UpdateSermonSeries(It.IsAny<SermonSeries>()))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockMessagesRepository.Setup(r => r.GetMessageById(messageId))
                .ReturnsAsync(new SystemResponse<SermonMessage>(true, "Message not found"));

            _mockMessagesRepository.Setup(r => r.CreateNewMessages(It.IsAny<IEnumerable<SermonMessage>>()))
                .ReturnsAsync(new SystemResponse<IEnumerable<SermonMessage>>(new[] { newMessage }, "Success"));

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalMessagesProcessed);
            Assert.AreEqual(1, result.Result.TotalMessagesUpdated); // Upserted (inserted)
            Assert.AreEqual(0, result.Result.TotalMessagesSkipped);
            Assert.AreEqual(0, result.Result.SkippedItems.Count());

            // Verify CreateNewMessages was called
            _mockMessagesRepository.Verify(r => r.CreateNewMessages(It.Is<IEnumerable<SermonMessage>>(msgs =>
                msgs.Count() == 1 &&
                msgs.First().Id == messageId &&
                msgs.First().SeriesId == seriesId &&
                msgs.First().Title == "Test Message" &&
                msgs.First().Speaker == "Test Speaker"
            )), Times.Once);
        }

        #endregion

        #region ImportSermonData - Cache Invalidation & Idempotency Tests

        [TestMethod]
        public async Task ImportSermonData_SuccessfulUpdate_ClearsCacheForUpdatedSeries()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
            var existingSeries = CreateTestSermonSeries(seriesId, "Test Series", "test-series");

            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = seriesId,
                        Name = "Updated Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "updated-series",
                        Messages = new List<SermonMessageResponse>()
                    }
                }
            };

            _mockSermonsRepository.Setup(r => r.GetSermonSeriesForId(seriesId))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockSermonsRepository.Setup(r => r.UpdateSermonSeries(It.IsAny<SermonSeries>()))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);

            // Verify cache.RemoveByPattern was called to invalidate sermon caches
            _mockCache.Verify(c => c.RemoveByPattern(It.Is<string>(key =>
                key.Contains("thrive:sermons")
            )), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task ImportSermonData_Idempotent_RunningTwiceProducesSameResult()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
            var existingSeries = CreateTestSermonSeries(seriesId, "Test Series", "test-series");

            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = seriesId,
                        Name = "Updated Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "updated-series",
                        Messages = new List<SermonMessageResponse>()
                    }
                }
            };

            _mockSermonsRepository.Setup(r => r.GetSermonSeriesForId(seriesId))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockSermonsRepository.Setup(r => r.UpdateSermonSeries(It.IsAny<SermonSeries>()))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            // Act - Run import twice
            var result1 = await _sermonsService.ImportSermonData(request);
            var result2 = await _sermonsService.ImportSermonData(request);

            // Assert - Both results should be identical
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsFalse(result1.HasErrors);
            Assert.IsFalse(result2.HasErrors);
            Assert.AreEqual(result1.Result.TotalSeriesProcessed, result2.Result.TotalSeriesProcessed);
            Assert.AreEqual(result1.Result.TotalSeriesUpdated, result2.Result.TotalSeriesUpdated);
            Assert.AreEqual(result1.Result.TotalSeriesSkipped, result2.Result.TotalSeriesSkipped);
        }

        [TestMethod]
        public async Task ImportSermonData_PlayCountNotUpdated_PreservesUsageData()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
            var messageId = "507f1f77bcf86cd799439012";
            var existingSeries = CreateTestSermonSeries(seriesId, "Test Series", "test-series");
            var existingMessage = CreateTestSermonMessage(messageId, seriesId, "Test Message");
            existingMessage.PlayCount = 100; // Existing play count

            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = seriesId,
                        Name = "Test Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "test-series",
                        Messages = new List<SermonMessageResponse>
                        {
                            new SermonMessageResponse
                            {
                                MessageId = messageId,
                                Title = "Updated Title",
                                Speaker = "Test Speaker",
                                Date = DateTime.UtcNow,
                                AudioDuration = 1800,
                                PlayCount = 0 // Import data has 0, but should not update
                            }
                        }
                    }
                }
            };

            _mockSermonsRepository.Setup(r => r.GetSermonSeriesForId(seriesId))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockSermonsRepository.Setup(r => r.UpdateSermonSeries(It.IsAny<SermonSeries>()))
                .ReturnsAsync(new SystemResponse<SermonSeries>(existingSeries, "Success"));

            _mockMessagesRepository.Setup(r => r.GetMessageById(messageId))
                .ReturnsAsync(new SystemResponse<SermonMessage>(existingMessage, "Success"));

            _mockMessagesRepository.Setup(r => r.UpdateMessageById(messageId, It.IsAny<SermonMessageRequest>()))
                .ReturnsAsync(existingMessage);

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);

            // Verify UpdateMessageById was called WITHOUT PlayCount in the request
            // (PlayCount should not be in SermonMessageRequest, preserving the existing value)
            _mockMessagesRepository.Verify(r => r.UpdateMessageById(
                messageId,
                It.Is<SermonMessageRequest>(m => m.Title == "Updated Title")
            ), Times.Once);
        }

        #endregion

        #region GetMessageWaveformData Tests

        [TestMethod]
        public async Task GetMessageWaveformData_ValidMessageId_ReturnsWaveformDataSuccessfully()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            var waveformData = new List<double> { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
            var expectedResponse = new SystemResponse<List<double>>(waveformData, "Success!");

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sermonsService.GetMessageWaveformData(messageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(10, result.Result.Count);
            Assert.AreEqual(0.1, result.Result[0]);
            Assert.AreEqual(1.0, result.Result[9]);
            Assert.AreEqual("Success!", result.SuccessMessage);

            // Verify repository was called with correct messageId
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId), Times.Once);
        }

        [TestMethod]
        public async Task GetMessageWaveformData_MessageNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            var errorMessage = "Unable to find property for id: message 507f1f77bcf86cd799439011";
            var errorResponse = new SystemResponse<List<double>>(true, errorMessage);

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _sermonsService.GetMessageWaveformData(messageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(errorMessage, result.ErrorMessage);
            Assert.IsNull(result.Result);

            // Verify repository was called
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId), Times.Once);
        }

        [TestMethod]
        public async Task GetMessageWaveformData_MessageExistsWithEmptyWaveformData_ReturnsEmptyList()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            var emptyWaveformData = new List<double>();
            var expectedResponse = new SystemResponse<List<double>>(emptyWaveformData, "Success!");

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sermonsService.GetMessageWaveformData(messageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(0, result.Result.Count);

            // Verify repository was called
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId), Times.Once);
        }

        [TestMethod]
        public async Task GetMessageWaveformData_LargeWaveformDataset_ReturnsCompleteData()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            // Create a large waveform dataset (e.g., 1000 data points)
            var largeWaveformData = new List<double>(Enumerable.Range(0, 1000)
                .Select(i => (double)i / 1000.0));
            var expectedResponse = new SystemResponse<List<double>>(largeWaveformData, "Success!");

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sermonsService.GetMessageWaveformData(messageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(1000, result.Result.Count);
            Assert.AreEqual(0.0, result.Result[0]);
            Assert.AreEqual(0.999, result.Result[999], 0.001);

            // Verify repository was called
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId), Times.Once);
        }

        [TestMethod]
        public async Task GetMessageWaveformData_InvalidMessageIdFormat_ReturnsErrorFromRepository()
        {
            // Arrange
            var invalidMessageId = "invalid-id-format";
            var errorMessage = "Unable to find property for id: message invalid-id-format";
            var errorResponse = new SystemResponse<List<double>>(true, errorMessage);

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(invalidMessageId))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _sermonsService.GetMessageWaveformData(invalidMessageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(errorMessage, result.ErrorMessage);

            // Verify repository was called with the invalid ID
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(invalidMessageId), Times.Once);
        }

        [TestMethod]
        public async Task GetMessageWaveformData_RepositoryThrowsException_PropagatesError()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            var exceptionMessage = "Database connection failed";

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => _sermonsService.GetMessageWaveformData(messageId));

            Assert.AreEqual(exceptionMessage, exception.Message);

            // Verify repository was called
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId), Times.Once);
        }

        [TestMethod]
        public async Task GetMessageWaveformData_NormalizedWaveformValues_ReturnsCorrectRange()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            // Waveform data should be normalized values between 0 and 1
            var normalizedWaveformData = new List<double> { 0.0, 0.25, 0.5, 0.75, 1.0 };
            var expectedResponse = new SystemResponse<List<double>>(normalizedWaveformData, "Success!");

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sermonsService.GetMessageWaveformData(messageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(5, result.Result.Count);

            // Verify all values are within expected range
            foreach (var value in result.Result)
            {
                Assert.IsTrue(value >= 0.0 && value <= 1.0,
                    $"Waveform value {value} is outside normalized range [0, 1]");
            }

            // Verify repository was called
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId), Times.Once);
        }

        [TestMethod]
        public async Task GetMessageWaveformData_MultipleCallsWithDifferentIds_CallsRepositoryForEach()
        {
            // Arrange
            var messageId1 = "507f1f77bcf86cd799439011";
            var messageId2 = "507f1f77bcf86cd799439012";
            var waveformData1 = new List<double> { 0.1, 0.2, 0.3 };
            var waveformData2 = new List<double> { 0.4, 0.5, 0.6 };

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId1))
                .ReturnsAsync(new SystemResponse<List<double>>(waveformData1, "Success!"));
            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId2))
                .ReturnsAsync(new SystemResponse<List<double>>(waveformData2, "Success!"));

            // Act
            var result1 = await _sermonsService.GetMessageWaveformData(messageId1);
            var result2 = await _sermonsService.GetMessageWaveformData(messageId2);

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsFalse(result1.HasErrors);
            Assert.IsFalse(result2.HasErrors);
            Assert.AreEqual(3, result1.Result.Count);
            Assert.AreEqual(3, result2.Result.Count);
            Assert.AreEqual(0.1, result1.Result[0]);
            Assert.AreEqual(0.4, result2.Result[0]);

            // Verify repository was called for each ID
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId1), Times.Once);
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId2), Times.Once);
        }

        [TestMethod]
        public async Task GetMessageWaveformData_ResponseFormatIsCorrect_VerifiesSystemResponseStructure()
        {
            // Arrange
            var messageId = "507f1f77bcf86cd799439011";
            var waveformData = new List<double> { 0.5, 0.6, 0.7 };
            var expectedResponse = new SystemResponse<List<double>>(waveformData, "Success!");

            _mockMessagesRepository.Setup(r => r.GetMessageWaveformData(messageId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sermonsService.GetMessageWaveformData(messageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(SystemResponse<List<double>>));
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(List<double>));
            Assert.AreEqual("Success!", result.SuccessMessage);
            Assert.IsNull(result.ErrorMessage);

            // Verify repository was called
            _mockMessagesRepository.Verify(r => r.GetMessageWaveformData(messageId), Times.Once);
        }

        #endregion

        #region GetAllSermons Caching Tests

        [TestMethod]
        public async Task GetAllSermons_CacheMiss_FetchesFromRepositoryAndCachesResult()
        {
            // Arrange
            var series1 = CreateTestSermonSeries("1", "Series 1", "series-1");
            var series2 = CreateTestSermonSeries("2", "Series 2", "series-2");
            var seriesList = new List<SermonSeries> { series1, series2 };

            var message1 = CreateTestSermonMessage("msg1", "1", "Message 1");
            var message2 = CreateTestSermonMessage("msg2", "2", "Message 2");
            var messagesList = new List<SermonMessage> { message1, message2 };

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(seriesList);

            _mockMessagesRepository.Setup(r => r.GetAllMessages())
                .ReturnsAsync(messagesList);

            // Setup cache miss
            _mockCache.Setup(c => c.ReadFromCache<SystemResponse<AllSermonsSummaryResponse>>(It.IsAny<string>()))
                .Returns((SystemResponse<AllSermonsSummaryResponse>)null);

            // Act
            var result = await _sermonsService.GetAllSermons(highResImg: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(2, result.Result.Summaries.Count());

            // Verify repository was called (cache miss)
            _mockSermonsRepository.Verify(r => r.GetAllSermons(It.IsAny<bool>()), Times.Once);
            _mockMessagesRepository.Verify(r => r.GetAllMessages(), Times.Once);

            // Verify cache InsertIntoCache was called with the correct key pattern
            _mockCache.Verify(c => c.InsertIntoCache(It.Is<string>(key =>
                key.Contains("thrive:sermons:summary")), It.IsAny<SystemResponse<AllSermonsSummaryResponse>>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAllSermons_CacheHit_ReturnsCachedResultWithoutCallingRepository()
        {
            // Arrange
            var cachedSummaries = new List<AllSermonSeriesSummary>
            {
                new AllSermonSeriesSummary { Id = "1", Title = "Cached Series 1", MessageCount = 5 },
                new AllSermonSeriesSummary { Id = "2", Title = "Cached Series 2", MessageCount = 3 }
            };
            var cachedResponse = new SystemResponse<AllSermonsSummaryResponse>(
                new AllSermonsSummaryResponse { Summaries = cachedSummaries },
                "Success!");

            // Setup cache hit - must set CanReadFromCache to true
            _mockCache.Setup(c => c.CanReadFromCache(It.IsAny<string>())).Returns(true);
            _mockCache.Setup(c => c.ReadFromCache<SystemResponse<AllSermonsSummaryResponse>>(It.IsAny<string>()))
                .Returns(cachedResponse);

            // Act
            var result = await _sermonsService.GetAllSermons(highResImg: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(2, result.Result.Summaries.Count());
            Assert.AreEqual("Cached Series 1", result.Result.Summaries.First().Title);

            // Verify repository was NOT called (cache hit)
            _mockSermonsRepository.Verify(r => r.GetAllSermons(It.IsAny<bool>()), Times.Never);
            _mockMessagesRepository.Verify(r => r.GetAllMessages(), Times.Never);
        }

        [TestMethod]
        public async Task GetAllSermons_HighResImgTrue_UsesDifferentCacheKey()
        {
            // Arrange
            var series = CreateTestSermonSeries("1", "Series 1", "series-1");
            var seriesList = new List<SermonSeries> { series };
            var messagesList = new List<SermonMessage>();

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(seriesList);

            _mockMessagesRepository.Setup(r => r.GetAllMessages())
                .ReturnsAsync(messagesList);

            // Setup cache miss
            _mockCache.Setup(c => c.ReadFromCache<SystemResponse<AllSermonsSummaryResponse>>(It.IsAny<string>()))
                .Returns((SystemResponse<AllSermonsSummaryResponse>)null);

            // Act
            var result = await _sermonsService.GetAllSermons(highResImg: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);

            // Verify cache key contains "true" for highResImg (lowercase)
            _mockCache.Verify(c => c.InsertIntoCache(It.Is<string>(key =>
                key.Contains("thrive:sermons:summary:true")), It.IsAny<SystemResponse<AllSermonsSummaryResponse>>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAllSermons_HighResImgFalse_UsesThumbnailUrl()
        {
            // Arrange
            var series = new SermonSeries
            {
                Id = "1",
                Name = "Test Series",
                Slug = "test-series",
                StartDate = DateTime.UtcNow,
                ArtUrl = "https://example.com/high-res-art.jpg",
                Thumbnail = "https://example.com/thumbnail.jpg"
            };
            var seriesList = new List<SermonSeries> { series };
            var messagesList = new List<SermonMessage>();

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(seriesList);

            _mockMessagesRepository.Setup(r => r.GetAllMessages())
                .ReturnsAsync(messagesList);

            // Setup cache miss
            _mockCache.Setup(c => c.ReadFromCache<SystemResponse<AllSermonsSummaryResponse>>(It.IsAny<string>()))
                .Returns((SystemResponse<AllSermonsSummaryResponse>)null);

            // Act
            var result = await _sermonsService.GetAllSermons(highResImg: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            var summary = result.Result.Summaries.First();
            Assert.AreEqual("https://example.com/thumbnail.jpg", summary.ArtUrl);
        }

        [TestMethod]
        public async Task GetAllSermons_HighResImgTrue_UsesArtUrl()
        {
            // Arrange
            var series = new SermonSeries
            {
                Id = "1",
                Name = "Test Series",
                Slug = "test-series",
                StartDate = DateTime.UtcNow,
                ArtUrl = "https://example.com/high-res-art.jpg",
                Thumbnail = "https://example.com/thumbnail.jpg"
            };
            var seriesList = new List<SermonSeries> { series };
            var messagesList = new List<SermonMessage>();

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(seriesList);

            _mockMessagesRepository.Setup(r => r.GetAllMessages())
                .ReturnsAsync(messagesList);

            // Setup cache miss
            _mockCache.Setup(c => c.ReadFromCache<SystemResponse<AllSermonsSummaryResponse>>(It.IsAny<string>()))
                .Returns((SystemResponse<AllSermonsSummaryResponse>)null);

            // Act
            var result = await _sermonsService.GetAllSermons(highResImg: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            var summary = result.Result.Summaries.First();
            Assert.AreEqual("https://example.com/high-res-art.jpg", summary.ArtUrl);
        }

        [TestMethod]
        public async Task GetAllSermons_CalculatesMessageCountCorrectly()
        {
            // Arrange
            var series1 = CreateTestSermonSeries("1", "Series 1", "series-1");
            var series2 = CreateTestSermonSeries("2", "Series 2", "series-2");
            var seriesList = new List<SermonSeries> { series1, series2 };

            // 3 messages for series 1, 2 messages for series 2
            var messages = new List<SermonMessage>
            {
                CreateTestSermonMessage("msg1", "1", "Message 1"),
                CreateTestSermonMessage("msg2", "1", "Message 2"),
                CreateTestSermonMessage("msg3", "1", "Message 3"),
                CreateTestSermonMessage("msg4", "2", "Message 4"),
                CreateTestSermonMessage("msg5", "2", "Message 5")
            };

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(seriesList);

            _mockMessagesRepository.Setup(r => r.GetAllMessages())
                .ReturnsAsync(messages);

            // Setup cache miss
            _mockCache.Setup(c => c.ReadFromCache<SystemResponse<AllSermonsSummaryResponse>>(It.IsAny<string>()))
                .Returns((SystemResponse<AllSermonsSummaryResponse>)null);

            // Act
            var result = await _sermonsService.GetAllSermons(highResImg: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            var summaries = result.Result.Summaries.ToList();
            Assert.AreEqual(2, summaries.Count);

            var series1Summary = summaries.First(s => s.Id == "1");
            var series2Summary = summaries.First(s => s.Id == "2");
            Assert.AreEqual(3, series1Summary.MessageCount);
            Assert.AreEqual(2, series2Summary.MessageCount);
        }

        [TestMethod]
        public async Task GetAllSermons_SeriesWithNoMessages_ReturnsZeroMessageCount()
        {
            // Arrange
            var series = CreateTestSermonSeries("1", "Empty Series", "empty-series");
            var seriesList = new List<SermonSeries> { series };
            var emptyMessages = new List<SermonMessage>();

            _mockSermonsRepository.Setup(r => r.GetAllSermons(It.IsAny<bool>()))
                .ReturnsAsync(seriesList);

            _mockMessagesRepository.Setup(r => r.GetAllMessages())
                .ReturnsAsync(emptyMessages);

            // Setup cache miss
            _mockCache.Setup(c => c.ReadFromCache<SystemResponse<AllSermonsSummaryResponse>>(It.IsAny<string>()))
                .Returns((SystemResponse<AllSermonsSummaryResponse>)null);

            // Act
            var result = await _sermonsService.GetAllSermons(highResImg: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            var summary = result.Result.Summaries.First();
            Assert.AreEqual(0, summary.MessageCount);
        }

        #endregion

        #region Helper Methods

        private SermonSeries CreateTestSermonSeries(string id, string name, string slug)
        {
            return new SermonSeries
            {
                Id = id,
                Name = name,
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                Slug = slug,
                Thumbnail = "https://example.com/thumbnail.jpg",
                ArtUrl = "https://example.com/art.jpg",
                LastUpdated = DateTime.UtcNow,
                Summary = "Test summary"
            };
        }

        private SermonMessage CreateTestSermonMessage(string id, string seriesId, string title)
        {
            return new SermonMessage
            {
                Id = id,
                SeriesId = seriesId,
                Title = title,
                Speaker = "Test Speaker",
                Date = DateTime.UtcNow,
                AudioUrl = "https://example.com/audio.mp3",
                AudioDuration = 1800,
                AudioFileSize = 10000000,
                VideoUrl = "https://example.com/video.mp4",
                PassageRef = "John 3:16",
                Summary = "Test message summary",
                PlayCount = 0,
                Tags = new List<MessageTag> { MessageTag.Gospels },
                LastUpdated = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                WaveformData = new List<double>(Enumerable.Repeat(0.50, 120))
            };
        }

        #endregion
    }
}

