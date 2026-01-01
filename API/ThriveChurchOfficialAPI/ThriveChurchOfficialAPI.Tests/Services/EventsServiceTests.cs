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
    public class EventsServiceTests
    {
        private Mock<IEventsRepository> _mockEventsRepository;
        private Mock<IMemoryCache> _mockCache;
        private EventsService _eventsService;

        [TestInitialize]
        public void Setup()
        {
            _mockEventsRepository = new Mock<IEventsRepository>();
            _mockCache = new Mock<IMemoryCache>();

            // Setup cache miss by default
            object cacheValue = null;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);

            // Setup cache entry for Set
            var cacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntry.Object);

            _eventsService = new EventsService(
                _mockEventsRepository.Object,
                _mockCache.Object
            );
        }

        #region Helper Methods

        private static Event CreateTestEvent(string id, string title, DateTime startTime)
        {
            return new Event
            {
                Id = id,
                Title = title,
                Summary = $"Summary for {title}",
                StartTime = startTime,
                EndTime = startTime.AddHours(2),
                IsActive = true,
                IsFeatured = false,
                Tags = new List<string> { "test" }
            };
        }

        private static CreateEventRequest CreateValidCreateEventRequest()
        {
            return new CreateEventRequest
            {
                Title = "Test Event",
                Summary = "Test Summary",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                IsOnline = false,
                IsRecurring = false
            };
        }

        #endregion

        #region GetAllEvents Tests

        [TestMethod]
        public async Task GetAllEvents_NoEvents_ReturnsEmptyList()
        {
            // Arrange
            _mockEventsRepository.Setup(r => r.GetAllEvents(It.IsAny<bool>()))
                .ReturnsAsync(new List<Event>());

            // Act
            var result = await _eventsService.GetAllEvents();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Result.TotalCount);
            Assert.AreEqual(0, result.Result.Events.Count());
        }

        [TestMethod]
        public async Task GetAllEvents_WithEvents_ReturnsEventSummaries()
        {
            // Arrange
            var events = new List<Event>
            {
                CreateTestEvent("1", "Event 1", DateTime.UtcNow.AddDays(1)),
                CreateTestEvent("2", "Event 2", DateTime.UtcNow.AddDays(2))
            };

            _mockEventsRepository.Setup(r => r.GetAllEvents(false)).ReturnsAsync(events);

            // Act
            var result = await _eventsService.GetAllEvents(includeInactive: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(2, result.Result.TotalCount);
            Assert.AreEqual(2, result.Result.Events.Count());
            Assert.AreEqual("Event 1", result.Result.Events.First().Title);
        }

        [TestMethod]
        public async Task GetAllEvents_IncludeInactive_PassesParameterToRepository()
        {
            // Arrange
            _mockEventsRepository.Setup(r => r.GetAllEvents(true)).ReturnsAsync(new List<Event>());

            // Act
            await _eventsService.GetAllEvents(includeInactive: true);

            // Assert
            _mockEventsRepository.Verify(r => r.GetAllEvents(true), Times.Once);
        }

        #endregion

        #region GetUpcomingEvents Tests

        [TestMethod]
        public async Task GetUpcomingEvents_ValidCount_ReturnsEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                CreateTestEvent("1", "Upcoming 1", DateTime.UtcNow.AddDays(1)),
                CreateTestEvent("2", "Upcoming 2", DateTime.UtcNow.AddDays(2))
            };

            _mockEventsRepository.Setup(r => r.GetUpcomingEvents(It.IsAny<DateTime>(), 10))
                .ReturnsAsync(events);

            // Act
            var result = await _eventsService.GetUpcomingEvents(count: 10);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(2, result.Result.TotalCount);
        }

        [TestMethod]
        public async Task GetUpcomingEvents_InvalidCount_Zero_ReturnsError()
        {
            // Act
            var result = await _eventsService.GetUpcomingEvents(count: 0);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Count must be between 1 and 100.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetUpcomingEvents_InvalidCount_Over100_ReturnsError()
        {
            // Act
            var result = await _eventsService.GetUpcomingEvents(count: 101);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Count must be between 1 and 100.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetUpcomingEvents_WithFromDate_PassesDateToRepository()
        {
            // Arrange
            var fromDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            _mockEventsRepository.Setup(r => r.GetUpcomingEvents(fromDate, 10))
                .ReturnsAsync(new List<Event>());

            // Act
            await _eventsService.GetUpcomingEvents(fromDate: fromDate, count: 10);

            // Assert
            _mockEventsRepository.Verify(r => r.GetUpcomingEvents(fromDate, 10), Times.Once);
        }

        #endregion

        #region GetEventsByDateRange Tests

        [TestMethod]
        public async Task GetEventsByDateRange_ValidRange_ReturnsEvents()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(30);
            var events = new List<Event>
            {
                CreateTestEvent("1", "Event in range", DateTime.UtcNow.AddDays(5))
            };

            _mockEventsRepository.Setup(r => r.GetEventsByDateRange(startDate, endDate))
                .ReturnsAsync(events);

            // Act
            var result = await _eventsService.GetEventsByDateRange(startDate, endDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalCount);
        }

        [TestMethod]
        public async Task GetEventsByDateRange_EndDateBeforeStartDate_ReturnsError()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(10);
            var endDate = DateTime.UtcNow;

            // Act
            var result = await _eventsService.GetEventsByDateRange(startDate, endDate);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetEventsByDateRange_SameDates_ReturnsError()
        {
            // Arrange
            var date = DateTime.UtcNow;

            // Act
            var result = await _eventsService.GetEventsByDateRange(date, date);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        #endregion

        #region GetEventById Tests

        [TestMethod]
        public async Task GetEventById_ValidId_ReturnsEvent()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            var eventEntity = CreateTestEvent(eventId, "Test Event", DateTime.UtcNow);

            _mockEventsRepository.Setup(r => r.GetEventById(eventId)).ReturnsAsync(eventEntity);

            // Act
            var result = await _eventsService.GetEventById(eventId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(eventId, result.Result.Event.Id);
            Assert.AreEqual("Test Event", result.Result.Event.Title);
        }

        [TestMethod]
        public async Task GetEventById_NullId_ReturnsError()
        {
            // Act
            var result = await _eventsService.GetEventById(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetEventById_EmptyId_ReturnsError()
        {
            // Act
            var result = await _eventsService.GetEventById(string.Empty);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task GetEventById_NotFound_ReturnsError()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            _mockEventsRepository.Setup(r => r.GetEventById(eventId)).ReturnsAsync((Event)null);

            // Act
            var result = await _eventsService.GetEventById(eventId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not found") || result.ErrorMessage.Contains("Unable to find"));
        }

        #endregion

        #region GetFeaturedEvents Tests

        [TestMethod]
        public async Task GetFeaturedEvents_ReturnsFeaturedEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                CreateTestEvent("1", "Featured Event", DateTime.UtcNow.AddDays(1))
            };
            events[0].IsFeatured = true;

            _mockEventsRepository.Setup(r => r.GetFeaturedEvents()).ReturnsAsync(events);

            // Act
            var result = await _eventsService.GetFeaturedEvents();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalCount);
        }

        #endregion

        #region CreateEvent Tests

        [TestMethod]
        public async Task CreateEvent_ValidRequest_CreatesEvent()
        {
            // Arrange
            var request = CreateValidCreateEventRequest();
            var createdEvent = new Event
            {
                Id = "507f1f77bcf86cd799439011",
                Title = request.Title,
                Summary = request.Summary,
                StartTime = request.StartTime
            };

            _mockEventsRepository.Setup(r => r.CreateEvent(It.IsAny<Event>())).ReturnsAsync(createdEvent);

            // Act
            var result = await _eventsService.CreateEvent(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(request.Title, result.Result.Event.Title);
        }

        [TestMethod]
        public async Task CreateEvent_NullRequest_ReturnsError()
        {
            // Act
            var result = await _eventsService.CreateEvent(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task CreateEvent_MissingTitle_ReturnsError()
        {
            // Arrange
            var request = CreateValidCreateEventRequest();
            request.Title = null;

            // Act
            var result = await _eventsService.CreateEvent(request);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task CreateEvent_MissingSummary_ReturnsError()
        {
            // Arrange
            var request = CreateValidCreateEventRequest();
            request.Summary = null;

            // Act
            var result = await _eventsService.CreateEvent(request);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task CreateEvent_RepositoryReturnsNull_ReturnsError()
        {
            // Arrange
            var request = CreateValidCreateEventRequest();
            _mockEventsRepository.Setup(r => r.CreateEvent(It.IsAny<Event>())).ReturnsAsync((Event)null);

            // Act
            var result = await _eventsService.CreateEvent(request);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        #endregion

        #region UpdateEvent Tests

        [TestMethod]
        public async Task UpdateEvent_ValidRequest_UpdatesEvent()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            var existingEvent = CreateTestEvent(eventId, "Old Title", DateTime.UtcNow);
            var request = new UpdateEventRequest { Title = "New Title" };
            var updatedEvent = CreateTestEvent(eventId, "New Title", DateTime.UtcNow);

            _mockEventsRepository.Setup(r => r.GetEventById(eventId)).ReturnsAsync(existingEvent);
            _mockEventsRepository.Setup(r => r.UpdateEvent(eventId, It.IsAny<Event>())).ReturnsAsync(updatedEvent);

            // Act
            var result = await _eventsService.UpdateEvent(eventId, request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("New Title", result.Result.Event.Title);
        }

        [TestMethod]
        public async Task UpdateEvent_NullEventId_ReturnsError()
        {
            // Arrange
            var request = new UpdateEventRequest { Title = "New Title" };

            // Act
            var result = await _eventsService.UpdateEvent(null, request);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task UpdateEvent_NullRequest_ReturnsError()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";

            // Act
            var result = await _eventsService.UpdateEvent(eventId, null);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task UpdateEvent_EventNotFound_ReturnsError()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            var request = new UpdateEventRequest { Title = "New Title" };
            _mockEventsRepository.Setup(r => r.GetEventById(eventId)).ReturnsAsync((Event)null);

            // Act
            var result = await _eventsService.UpdateEvent(eventId, request);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not found") || result.ErrorMessage.Contains("Unable to find"));
        }

        [TestMethod]
        public async Task UpdateEvent_RepositoryUpdateFails_ReturnsError()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            var existingEvent = CreateTestEvent(eventId, "Old Title", DateTime.UtcNow);
            var request = new UpdateEventRequest { Title = "New Title" };

            _mockEventsRepository.Setup(r => r.GetEventById(eventId)).ReturnsAsync(existingEvent);
            _mockEventsRepository.Setup(r => r.UpdateEvent(eventId, It.IsAny<Event>())).ReturnsAsync((Event)null);

            // Act
            var result = await _eventsService.UpdateEvent(eventId, request);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        #endregion

        #region DeleteEvent Tests

        [TestMethod]
        public async Task DeleteEvent_ValidId_DeletesEvent()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            _mockEventsRepository.Setup(r => r.DeleteEvent(eventId)).ReturnsAsync(true);

            // Act
            var result = await _eventsService.DeleteEvent(eventId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("Event deleted successfully.", result.Result);
        }

        [TestMethod]
        public async Task DeleteEvent_NullId_ReturnsError()
        {
            // Act
            var result = await _eventsService.DeleteEvent(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task DeleteEvent_EventNotFound_ReturnsError()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            _mockEventsRepository.Setup(r => r.DeleteEvent(eventId)).ReturnsAsync(false);

            // Act
            var result = await _eventsService.DeleteEvent(eventId);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        #endregion

        #region DeactivateEvent Tests

        [TestMethod]
        public async Task DeactivateEvent_ValidId_DeactivatesEvent()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            var existingEvent = CreateTestEvent(eventId, "Test Event", DateTime.UtcNow);
            existingEvent.IsActive = true;
            var deactivatedEvent = CreateTestEvent(eventId, "Test Event", DateTime.UtcNow);
            deactivatedEvent.IsActive = false;

            _mockEventsRepository.SetupSequence(r => r.GetEventById(eventId))
                .ReturnsAsync(existingEvent)
                .ReturnsAsync(deactivatedEvent);
            _mockEventsRepository.Setup(r => r.DeactivateEvent(eventId)).ReturnsAsync(true);

            // Act
            var result = await _eventsService.DeactivateEvent(eventId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsFalse(result.Result.Event.IsActive);
        }

        [TestMethod]
        public async Task DeactivateEvent_NullId_ReturnsError()
        {
            // Act
            var result = await _eventsService.DeactivateEvent(null);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task DeactivateEvent_EmptyId_ReturnsError()
        {
            // Act
            var result = await _eventsService.DeactivateEvent(string.Empty);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public async Task DeactivateEvent_EventNotFound_ReturnsError()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            _mockEventsRepository.Setup(r => r.GetEventById(eventId)).ReturnsAsync((Event)null);

            // Act
            var result = await _eventsService.DeactivateEvent(eventId);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("not found") || result.ErrorMessage.Contains("Unable to find"));
        }

        [TestMethod]
        public async Task DeactivateEvent_RepositoryFails_ReturnsError()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            var existingEvent = CreateTestEvent(eventId, "Test Event", DateTime.UtcNow);

            _mockEventsRepository.Setup(r => r.GetEventById(eventId)).ReturnsAsync(existingEvent);
            _mockEventsRepository.Setup(r => r.DeactivateEvent(eventId)).ReturnsAsync(false);

            // Act
            var result = await _eventsService.DeactivateEvent(eventId);

            // Assert
            Assert.IsTrue(result.HasErrors);
        }

        #endregion

        #region Cache Tests

        [TestMethod]
        public async Task GetAllEvents_CacheHit_ReturnsCachedResult()
        {
            // Arrange
            var cachedResponse = new SystemResponse<AllEventsResponse>(
                new AllEventsResponse { Events = new List<EventSummary>(), TotalCount = 5 },
                "Success!");

            object cacheValue = cachedResponse;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(true);

            // Act
            var result = await _eventsService.GetAllEvents();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Result.TotalCount);
            _mockEventsRepository.Verify(r => r.GetAllEvents(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public async Task GetEventById_CacheHit_ReturnsCachedResult()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            var cachedEvent = CreateTestEvent(eventId, "Cached Event", DateTime.UtcNow);
            var cachedResponse = new SystemResponse<EventResponse>(
                new EventResponse { Event = cachedEvent },
                "Success!");

            object cacheValue = cachedResponse;
            _mockCache.Setup(c => c.TryGetValue(
                It.Is<object>(k => k.ToString().Contains(eventId)), out cacheValue)).Returns(true);

            // Act
            var result = await _eventsService.GetEventById(eventId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Cached Event", result.Result.Event.Title);
            _mockEventsRepository.Verify(r => r.GetEventById(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateEvent_InvalidatesCache()
        {
            // Arrange
            var request = CreateValidCreateEventRequest();
            var createdEvent = new Event
            {
                Id = "507f1f77bcf86cd799439011",
                Title = request.Title,
                Summary = request.Summary,
                StartTime = request.StartTime
            };

            _mockEventsRepository.Setup(r => r.CreateEvent(It.IsAny<Event>())).ReturnsAsync(createdEvent);

            // Act
            await _eventsService.CreateEvent(request);

            // Assert
            _mockCache.Verify(c => c.Remove(It.IsAny<object>()), Times.AtLeastOnce);
        }

        #endregion
    }
}

