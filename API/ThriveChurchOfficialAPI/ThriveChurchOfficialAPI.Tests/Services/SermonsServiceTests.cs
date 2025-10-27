using Microsoft.Extensions.Caching.Memory;
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
        private Mock<IMemoryCache> _mockCache;
        private Mock<IS3Repository> _mockS3Repository;
        private SermonsService _sermonsService;

        [TestInitialize]
        public void Setup()
        {
            _mockSermonsRepository = new Mock<ISermonsRepository>();
            _mockMessagesRepository = new Mock<IMessagesRepository>();
            _mockCache = new Mock<IMemoryCache>();
            _mockS3Repository = new Mock<IS3Repository>();

            _sermonsService = new SermonsService(
                _mockSermonsRepository.Object,
                _mockMessagesRepository.Object,
                _mockCache.Object,
                _mockS3Repository.Object
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

            // Setup cache remove
            object cacheValue;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);
            _mockCache.Setup(c => c.Remove(It.IsAny<object>()));

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

            // Setup cache remove
            object cacheValue;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);
            _mockCache.Setup(c => c.Remove(It.IsAny<object>()));

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
        public async Task ImportSermonData_NonExistentSeriesId_SkipsSeriesAndAddsToSkippedItems()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
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

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalSeriesProcessed);
            Assert.AreEqual(0, result.Result.TotalSeriesUpdated);
            Assert.AreEqual(1, result.Result.TotalSeriesSkipped);
            Assert.AreEqual(1, result.Result.SkippedItems.Count());

            var skippedItem = result.Result.SkippedItems.First();
            Assert.AreEqual(seriesId, skippedItem.Id);
            Assert.AreEqual("Series", skippedItem.Type);
        }

        [TestMethod]
        public async Task ImportSermonData_NonExistentMessageId_SkipsMessageAndAddsToSkippedItems()
        {
            // Arrange
            var seriesId = "507f1f77bcf86cd799439011";
            var messageId = "507f1f77bcf86cd799439012";
            var existingSeries = CreateTestSermonSeries(seriesId, "Test Series", "test-series");

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

            // Setup cache remove
            object cacheValue;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);
            _mockCache.Setup(c => c.Remove(It.IsAny<object>()));

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalMessagesProcessed);
            Assert.AreEqual(0, result.Result.TotalMessagesUpdated);
            Assert.AreEqual(1, result.Result.TotalMessagesSkipped);
            Assert.AreEqual(1, result.Result.SkippedItems.Count());

            var skippedItem = result.Result.SkippedItems.First();
            Assert.AreEqual(messageId, skippedItem.Id);
            Assert.AreEqual("Message", skippedItem.Type);
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

            // Setup cache remove
            object cacheValue;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);
            _mockCache.Setup(c => c.Remove(It.IsAny<object>()));

            // Act
            var result = await _sermonsService.ImportSermonData(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);

            // Verify cache.Remove was called for the updated series
            _mockCache.Verify(c => c.Remove(It.Is<object>(key =>
                key.ToString().Contains(seriesId)
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

            // Setup cache remove
            object cacheValue;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);
            _mockCache.Setup(c => c.Remove(It.IsAny<object>()));

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

            // Setup cache remove
            object cacheValue;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);
            _mockCache.Setup(c => c.Remove(It.IsAny<object>()));

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
                CreateDate = DateTime.UtcNow
            };
        }

        #endregion
    }
}

