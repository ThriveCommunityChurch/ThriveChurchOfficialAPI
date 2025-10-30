using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    [TestClass]
    public class ImportSermonDataRequestValidationTests
    {
        #region ValidateRequest Tests

        [TestMethod]
        public void ValidateRequest_NullRequest_ReturnsError()
        {
            // Act
            var result = ImportSermonDataRequest.ValidateRequest(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateRequest_NullSeriesArray_ReturnsError()
        {
            // Arrange
            var request = new ImportSermonDataRequest
            {
                Series = null
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateRequest_EmptySeriesArray_ReturnsError()
        {
            // Arrange
            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>()
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateRequest_InvalidSeriesObjectIdFormat_ReturnsError()
        {
            // Arrange
            var request = new ImportSermonDataRequest
            {
                Series = new List<SermonSeriesResponse>
                {
                    new SermonSeriesResponse
                    {
                        Id = "invalid-id-format",
                        Name = "Test Series",
                        StartDate = DateTime.UtcNow,
                        Slug = "test-series"
                    }
                }
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Invalid") || result.ErrorMessage.Contains("ObjectId"));
        }

        [TestMethod]
        public void ValidateRequest_InvalidMessageObjectIdFormat_ReturnsError()
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
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Invalid") || result.ErrorMessage.Contains("ObjectId"));
        }

        [TestMethod]
        public void ValidateRequest_MissingSeriesName_ReturnsError()
        {
            // Arrange
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
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Name") || result.ErrorMessage.Contains("required"));
        }

        [TestMethod]
        public void ValidateRequest_MissingSeriesSlug_ReturnsError()
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
                        Slug = null // Missing required field
                    }
                }
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Slug") || result.ErrorMessage.Contains("required"));
        }

        [TestMethod]
        public void ValidateRequest_MissingMessageSpeaker_ReturnsError()
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
                                MessageId = "507f1f77bcf86cd799439012",
                                Speaker = null, // Missing required field
                                Title = "Test Message",
                                Date = DateTime.UtcNow,
                                AudioDuration = 1800
                            }
                        }
                    }
                }
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Speaker") || result.ErrorMessage.Contains("required"));
        }

        [TestMethod]
        public void ValidateRequest_MissingMessageTitle_ReturnsError()
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
                                MessageId = "507f1f77bcf86cd799439012",
                                Speaker = "Test Speaker",
                                Title = null, // Missing required field
                                Date = DateTime.UtcNow,
                                AudioDuration = 1800
                            }
                        }
                    }
                }
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("Title") || result.ErrorMessage.Contains("required"));
        }

        [TestMethod]
        public void ValidateRequest_InvalidAudioDuration_ReturnsError()
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
                                MessageId = "507f1f77bcf86cd799439012",
                                Speaker = "Test Speaker",
                                Title = "Test Message",
                                Date = DateTime.UtcNow,
                                AudioDuration = 0 // Invalid - must be > 0
                            }
                        }
                    }
                }
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("AudioDuration") || result.ErrorMessage.Contains("duration"));
        }

        [TestMethod]
        public void ValidateRequest_UnknownTag_ReturnsError()
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
                                MessageId = "507f1f77bcf86cd799439012",
                                Speaker = "Test Speaker",
                                Title = "Test Message",
                                Date = DateTime.UtcNow,
                                AudioDuration = 1800,
                                Tags = new List<MessageTag> { MessageTag.Unknown } // Invalid tag
                            }
                        }
                    }
                }
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("tag") || result.ErrorMessage.Contains("Unknown"));
        }

        [TestMethod]
        public void ValidateRequest_ValidRequest_ReturnsSuccess()
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
                        StartDate = new DateTime(2024, 1, 1),
                        EndDate = new DateTime(2024, 12, 31),
                        Slug = "test-series",
                        Messages = new List<SermonMessageResponse>
                        {
                            new SermonMessageResponse
                            {
                                MessageId = "507f1f77bcf86cd799439012",
                                Speaker = "Test Speaker",
                                Title = "Test Message",
                                Date = new DateTime(2024, 6, 1),
                                AudioDuration = 1800,
                                Tags = new List<MessageTag> { MessageTag.Gospels }
                            }
                        }
                    }
                }
            };

            // Act
            var result = ImportSermonDataRequest.ValidateRequest(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
        }

        #endregion
    }
}

