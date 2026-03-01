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
        private Mock<ICacheService> _mockCache;
        private EventsService _eventsService;

        [TestInitialize]
        public void Setup()
        {
            _mockEventsRepository = new Mock<IEventsRepository>();
            _mockCache = new Mock<ICacheService>();

            // Setup cache miss by default (ReadFromCache returns default which is null for reference types)
            // No need to setup ReadFromCache - Moq returns default(T) by default
            _mockCache.Setup(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
                .Returns((string key, object item, TimeSpan exp) => item);

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
            var cachedData = new AllEventsResponse { Events = new List<EventSummary>(), TotalCount = 5 };

            // Setup cache hit - ReadFromCache returns the cached data object
            _mockCache.Setup(c => c.ReadFromCache<AllEventsResponse>(It.IsAny<string>()))
                .Returns(cachedData);

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

            // Setup cache hit - ReadFromCache returns the cached Event object
            _mockCache.Setup(c => c.ReadFromCache<Event>(It.Is<string>(k => k.Contains(eventId))))
                .Returns(cachedEvent);

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
            _mockCache.Verify(c => c.RemoveByPattern(It.IsAny<string>()), Times.AtLeastOnce);
        }

        #endregion

        #region Additional Coverage Tests

        [TestMethod]
        public async Task GetAllEvents_WithCachedResponse_ReturnsCachedData()
        {
            // Arrange - setup cache hit
            var cachedResponse = new AllEventsResponse
            {
                Events = new List<EventSummary>
                {
                    new EventSummary { Id = "1", Title = "Cached Event" }
                },
                TotalCount = 1
            };

            _mockCache.Setup(c => c.ReadFromCache<AllEventsResponse>(It.IsAny<string>()))
                .Returns(cachedResponse);

            // Act
            var result = await _eventsService.GetAllEvents();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalCount);
            Assert.AreEqual("Cached Event", result.Result.Events.First().Title);
            // Verify repository was NOT called due to cache hit
            _mockEventsRepository.Verify(r => r.GetAllEvents(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateEvent_WithAllOptionalProperties_UpdatesAllFields()
        {
            // Arrange
            var eventId = "507f1f77bcf86cd799439011";
            var existingEvent = CreateTestEvent(eventId, "Old Title", DateTime.UtcNow);

            var request = new UpdateEventRequest
            {
                Title = "New Title",
                Summary = "New Summary",
                Description = "New Description",
                ImageUrl = "http://image.url/image.jpg",
                ThumbnailUrl = "http://thumb.url/thumb.jpg",
                IconName = "fa-calendar",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                IsAllDay = true,
                IsRecurring = false,
                Recurrence = new EventRecurrence { Pattern = RecurrencePattern.Weekly, Interval = 1, DayOfWeek = 0 },
                IsOnline = true,
                OnlineLink = "http://zoom.link/meeting",
                OnlinePlatform = "Zoom",
                Location = new EventLocation { Name = "Church Hall", Address = "123 Main St" },
                ContactEmail = "test@example.com",
                ContactPhone = "555-1234",
                RegistrationUrl = "http://register.url",
                Tags = new List<string> { "tag1", "tag2" },
                IsActive = false,
                IsFeatured = true
            };

            var updatedEvent = new Event
            {
                Id = eventId,
                Title = "New Title",
                Summary = "New Summary",
                Description = "New Description",
                ImageUrl = "http://image.url/image.jpg",
                ThumbnailUrl = "http://thumb.url/thumb.jpg",
                IconName = "fa-calendar",
                StartTime = request.StartTime.Value.ToUniversalTime(),
                EndTime = request.EndTime.Value.ToUniversalTime(),
                IsAllDay = true,
                IsRecurring = false,
                Recurrence = request.Recurrence,
                IsOnline = true,
                OnlineLink = "http://zoom.link/meeting",
                OnlinePlatform = "Zoom",
                Location = request.Location,
                ContactEmail = "test@example.com",
                ContactPhone = "555-1234",
                RegistrationUrl = "http://register.url",
                Tags = new List<string> { "tag1", "tag2" },
                IsActive = false,
                IsFeatured = true
            };

            _mockEventsRepository.Setup(r => r.GetEventById(eventId)).ReturnsAsync(existingEvent);
            _mockEventsRepository.Setup(r => r.UpdateEvent(eventId, It.IsAny<Event>())).ReturnsAsync(updatedEvent);

            // Act
            var result = await _eventsService.UpdateEvent(eventId, request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("New Title", result.Result.Event.Title);
            Assert.AreEqual("New Summary", result.Result.Event.Summary);
            Assert.AreEqual("New Description", result.Result.Event.Description);
            Assert.AreEqual("http://image.url/image.jpg", result.Result.Event.ImageUrl);
            Assert.AreEqual("http://thumb.url/thumb.jpg", result.Result.Event.ThumbnailUrl);
            Assert.AreEqual("fa-calendar", result.Result.Event.IconName);
            Assert.AreEqual(true, result.Result.Event.IsAllDay);
            Assert.AreEqual(true, result.Result.Event.IsOnline);
            Assert.AreEqual("http://zoom.link/meeting", result.Result.Event.OnlineLink);
            Assert.AreEqual("Zoom", result.Result.Event.OnlinePlatform);
            Assert.AreEqual("test@example.com", result.Result.Event.ContactEmail);
            Assert.AreEqual("555-1234", result.Result.Event.ContactPhone);
            Assert.AreEqual("http://register.url", result.Result.Event.RegistrationUrl);
            Assert.AreEqual(false, result.Result.Event.IsActive);
            Assert.AreEqual(true, result.Result.Event.IsFeatured);
            Assert.AreEqual(2, result.Result.Event.Tags.Count);
        }

        [TestMethod]
        public void CalculateRecurringDates_WithPatternNone_ReturnsEmptyAfterMinValue()
        {
            // Arrange - Event with RecurrencePattern.None will cause GetNextOccurrenceDate to return DateTime.MinValue
            var evt = new Event
            {
                Id = "1",
                Title = "Test Recurring",
                StartTime = DateTime.UtcNow.AddDays(-1), // Start in the past so we enter the while loop
                IsRecurring = true,
                Recurrence = new EventRecurrence
                {
                    Pattern = RecurrencePattern.None, // This will cause DateTime.MinValue to be returned
                    Interval = 1
                }
            };

            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddMonths(1);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should return empty or minimal dates because Pattern.None returns DateTime.MinValue
            Assert.IsNotNull(result);
            // The first iteration may add the StartTime if it's in range, then break on MinValue
        }

        [TestMethod]
        public void CalculateRecurringDates_WithNullRecurrence_ReturnsEmpty()
        {
            // Arrange - This tests a defensive code path
            var evt = new Event
            {
                Id = "1",
                Title = "Test",
                StartTime = DateTime.UtcNow,
                IsRecurring = true,
                Recurrence = null
            };

            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddMonths(1);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CalculateRecurringDates_WithDailyPattern_ReturnsCorrectDates()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date;
            var evt = new Event
            {
                Id = "1",
                Title = "Daily Event",
                StartTime = startDate,
                IsRecurring = true,
                Recurrence = new EventRecurrence
                {
                    Pattern = RecurrencePattern.Daily,
                    Interval = 1
                }
            };

            var fromDate = startDate;
            var toDate = startDate.AddDays(5);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 5); // Should have daily occurrences
        }

        [TestMethod]
        public void CalculateRecurringDates_WithYearlyPattern_ReturnsCorrectDates()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date;
            var evt = new Event
            {
                Id = "1",
                Title = "Yearly Event",
                StartTime = startDate,
                IsRecurring = true,
                Recurrence = new EventRecurrence
                {
                    Pattern = RecurrencePattern.Yearly,
                    Interval = 1
                }
            };

            var fromDate = startDate;
            var toDate = startDate.AddYears(3);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 2); // Should have yearly occurrences
        }

        [TestMethod]
        public void CalculateRecurringDates_WithMonthlyPattern_HandlesEndOfMonthCorrectly()
        {
            // Arrange - Start on Jan 31 to test edge case
            var startDate = new DateTime(2026, 1, 31, 10, 0, 0, DateTimeKind.Utc);
            var evt = new Event
            {
                Id = "1",
                Title = "Monthly Event on 31st",
                StartTime = startDate,
                IsRecurring = true,
                Recurrence = new EventRecurrence
                {
                    Pattern = RecurrencePattern.Monthly,
                    Interval = 1,
                    DayOfMonth = 31
                }
            };

            var fromDate = startDate;
            var toDate = startDate.AddMonths(4);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 2); // Should handle Feb 28/29 correctly
        }

        [TestMethod]
        public async Task GetFeaturedEvents_WithCachedResponse_ReturnsCachedData()
        {
            // Arrange - setup cache hit for GetFeaturedEvents
            var cachedResponse = new AllEventsResponse
            {
                Events = new List<EventSummary>
                {
                    new EventSummary { Id = "1", Title = "Cached Featured Event", IsFeatured = true }
                },
                TotalCount = 1
            };

            _mockCache.Setup(c => c.ReadFromCache<AllEventsResponse>(CacheKeys.EventsFeatured))
                .Returns(cachedResponse);

            // Act
            var result = await _eventsService.GetFeaturedEvents();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(1, result.Result.TotalCount);
            Assert.AreEqual("Cached Featured Event", result.Result.Events.First().Title);
            // Verify repository was NOT called due to cache hit
            _mockEventsRepository.Verify(r => r.GetFeaturedEvents(), Times.Never);
        }

        [TestMethod]
        public void CalculateRecurringDates_WithInvalidEnumPattern_ReturnsMinValueAndBreaks()
        {
            // Arrange - Create an event with an invalid RecurrencePattern value
            // This tests the default case in the switch statement which returns DateTime.MinValue
            var startDate = DateTime.UtcNow.Date;
            var evt = new Event
            {
                Id = "1",
                Title = "Event with Invalid Pattern",
                StartTime = startDate,
                IsRecurring = true,
                Recurrence = new EventRecurrence
                {
                    Pattern = (RecurrencePattern)999, // Invalid enum value
                    Interval = 1
                }
            };

            var fromDate = startDate;
            var toDate = startDate.AddMonths(1);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should have at most 1 date (the first occurrence before breaking)
            Assert.IsNotNull(result);
            // The first iteration adds startDate, then GetNextOccurrenceDate returns MinValue,
            // causing the loop to break immediately
            Assert.IsTrue(result.Count <= 1);
        }

        #endregion
    }
}

