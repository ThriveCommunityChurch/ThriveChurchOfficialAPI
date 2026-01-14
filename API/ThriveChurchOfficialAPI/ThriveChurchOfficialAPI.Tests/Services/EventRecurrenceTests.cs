using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    /// <summary>
    /// Tests for event recurrence calculation logic
    /// </summary>
    [TestClass]
    public class EventRecurrenceTests
    {
        private EventsService _eventsService;

        [TestInitialize]
        public void Setup()
        {
            var mockEventsRepository = new Mock<IEventsRepository>();
            var mockCache = new Mock<ICacheService>();

            // Setup cache miss by default
            mockCache.Setup(c => c.CanReadFromCache(It.IsAny<string>())).Returns(false);
            mockCache.Setup(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()));

            _eventsService = new EventsService(mockEventsRepository.Object, mockCache.Object);
        }

        #region Helper Methods

        private static Event CreateRecurringEvent(
            RecurrencePattern pattern,
            DateTime startTime,
            int interval = 1,
            DateTime? endDate = null)
        {
            return new Event
            {
                Id = "test-event-id",
                Title = "Recurring Event",
                Summary = "Test recurring event",
                StartTime = startTime,
                IsRecurring = true,
                IsActive = true,
                Recurrence = new EventRecurrence
                {
                    Pattern = pattern,
                    Interval = interval,
                    EndDate = endDate
                }
            };
        }

        #endregion

        #region Null/Invalid Input Tests

        [TestMethod]
        public void CalculateRecurringDates_NullEvent_ReturnsEmptyList()
        {
            // Act
            var result = _eventsService.CalculateRecurringDates(null, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CalculateRecurringDates_NonRecurringEvent_ReturnsEmptyList()
        {
            // Arrange
            var evt = new Event
            {
                Id = "test",
                Title = "Non-recurring",
                StartTime = DateTime.UtcNow,
                IsRecurring = false
            };

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CalculateRecurringDates_NullRecurrence_ReturnsEmptyList()
        {
            // Arrange
            var evt = new Event
            {
                Id = "test",
                Title = "Missing recurrence",
                StartTime = DateTime.UtcNow,
                IsRecurring = true,
                Recurrence = null
            };

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CalculateRecurringDates_PatternNone_ReturnsEmptyList()
        {
            // Arrange
            var evt = CreateRecurringEvent(RecurrencePattern.None, DateTime.UtcNow);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Daily Recurrence Tests

        [TestMethod]
        public void CalculateRecurringDates_Daily_ReturnsCorrectDates()
        {
            // Arrange
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Daily, startTime);
            var fromDate = startTime;
            var toDate = startTime.AddDays(4); // Jan 1-5 (inclusive) = 5 days

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should include Jan 1, 2, 3, 4, 5 (5 days)
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), result[0]);
            Assert.AreEqual(new DateTime(2025, 1, 5, 10, 0, 0, DateTimeKind.Utc), result[4]);
        }

        [TestMethod]
        public void CalculateRecurringDates_DailyWithInterval_ReturnsCorrectDates()
        {
            // Arrange - Every 2 days
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Daily, startTime, interval: 2);
            var fromDate = startTime;
            var toDate = startTime.AddDays(7);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should include Jan 1, 3, 5, 7 (every 2 days)
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), result[0]);
            Assert.AreEqual(new DateTime(2025, 1, 3, 10, 0, 0, DateTimeKind.Utc), result[1]);
            Assert.AreEqual(new DateTime(2025, 1, 5, 10, 0, 0, DateTimeKind.Utc), result[2]);
            Assert.AreEqual(new DateTime(2025, 1, 7, 10, 0, 0, DateTimeKind.Utc), result[3]);
        }

        #endregion

        #region Weekly Recurrence Tests

        [TestMethod]
        public void CalculateRecurringDates_Weekly_ReturnsCorrectDates()
        {
            // Arrange - Every week starting Jan 1 (Wednesday)
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Weekly, startTime);
            var fromDate = startTime;
            var toDate = startTime.AddDays(21); // 3 weeks (inclusive) = 4 occurrences

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should include Jan 1, 8, 15, 22 (4 Wednesdays)
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), result[0]);
            Assert.AreEqual(new DateTime(2025, 1, 8, 10, 0, 0, DateTimeKind.Utc), result[1]);
            Assert.AreEqual(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), result[2]);
            Assert.AreEqual(new DateTime(2025, 1, 22, 10, 0, 0, DateTimeKind.Utc), result[3]);
        }

        [TestMethod]
        public void CalculateRecurringDates_WeeklyWithInterval_ReturnsCorrectDates()
        {
            // Arrange - Every 2 weeks
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Weekly, startTime, interval: 2);
            var fromDate = startTime;
            var toDate = startTime.AddDays(28); // 4 weeks (inclusive) = 3 occurrences at 2-week interval

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should include Jan 1, 15, 29 (every 2 weeks)
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), result[0]);
            Assert.AreEqual(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), result[1]);
            Assert.AreEqual(new DateTime(2025, 1, 29, 10, 0, 0, DateTimeKind.Utc), result[2]);
        }

        #endregion

        #region BiWeekly Recurrence Tests

        [TestMethod]
        public void CalculateRecurringDates_BiWeekly_ReturnsCorrectDates()
        {
            // Arrange - Every 2 weeks
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.BiWeekly, startTime);
            var fromDate = startTime;
            var toDate = startTime.AddDays(42); // 6 weeks (inclusive) = 4 occurrences at 2-week interval

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should include Jan 1, 15, 29, Feb 12 (every 2 weeks)
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), result[0]);
            Assert.AreEqual(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), result[1]);
            Assert.AreEqual(new DateTime(2025, 1, 29, 10, 0, 0, DateTimeKind.Utc), result[2]);
            Assert.AreEqual(new DateTime(2025, 2, 12, 10, 0, 0, DateTimeKind.Utc), result[3]);
        }

        #endregion

        #region Monthly Recurrence Tests

        [TestMethod]
        public void CalculateRecurringDates_Monthly_ReturnsCorrectDates()
        {
            // Arrange - Monthly on the 15th
            var startTime = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Monthly, startTime);
            var fromDate = startTime;
            var toDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should include Jan 15, Feb 15, Mar 15, Apr 15, May 15
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), result[0]);
            Assert.AreEqual(new DateTime(2025, 2, 15, 10, 0, 0, DateTimeKind.Utc), result[1]);
            Assert.AreEqual(new DateTime(2025, 5, 15, 10, 0, 0, DateTimeKind.Utc), result[4]);
        }

        [TestMethod]
        public void CalculateRecurringDates_Monthly_HandlesEndOfMonth()
        {
            // Arrange - Monthly on the 31st (edge case for Feb, Apr, etc.)
            var startTime = new DateTime(2025, 1, 31, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Monthly, startTime);
            var fromDate = startTime;
            var toDate = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should handle Feb (28), Mar (31), Apr (30)
            Assert.IsTrue(result.Count >= 3);
            Assert.AreEqual(new DateTime(2025, 1, 31, 10, 0, 0, DateTimeKind.Utc), result[0]);
            // Feb should be 28 (2025 is not a leap year)
            Assert.AreEqual(new DateTime(2025, 2, 28, 10, 0, 0, DateTimeKind.Utc), result[1]);
            // Mar should be 31
            Assert.AreEqual(new DateTime(2025, 3, 31, 10, 0, 0, DateTimeKind.Utc), result[2]);
            // Apr should be 30
            Assert.AreEqual(new DateTime(2025, 4, 30, 10, 0, 0, DateTimeKind.Utc), result[3]);
        }

        #endregion

        #region Yearly Recurrence Tests

        [TestMethod]
        public void CalculateRecurringDates_Yearly_ReturnsCorrectDates()
        {
            // Arrange - Yearly event (e.g., annual conference)
            var startTime = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Yearly, startTime);
            var fromDate = startTime;
            var toDate = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should include Jun 15, 2025, 2026, 2027, 2028
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc), result[0]);
            Assert.AreEqual(new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc), result[1]);
            Assert.AreEqual(new DateTime(2027, 6, 15, 10, 0, 0, DateTimeKind.Utc), result[2]);
            Assert.AreEqual(new DateTime(2028, 6, 15, 10, 0, 0, DateTimeKind.Utc), result[3]);
        }

        #endregion

        #region End Date Tests

        [TestMethod]
        public void CalculateRecurringDates_WithEndDate_StopsAtEndDate()
        {
            // Arrange - Weekly event that ends after 3 weeks
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Weekly, startTime, endDate: endDate);
            var fromDate = startTime;
            var toDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should only include Jan 1, 8, 15 (stops before 22 due to endDate)
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), result[0]);
            Assert.AreEqual(new DateTime(2025, 1, 8, 10, 0, 0, DateTimeKind.Utc), result[1]);
            Assert.AreEqual(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), result[2]);
        }

        [TestMethod]
        public void CalculateRecurringDates_EndDateBeforeFromDate_ReturnsEmptyList()
        {
            // Arrange - Recurrence ended before the query range
            var startTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Weekly, startTime, endDate: endDate);
            var fromDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var toDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should return empty since recurrence ended before query range
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Date Range Tests

        [TestMethod]
        public void CalculateRecurringDates_FromDateAfterStartTime_StartsFromFirstOccurrenceInRange()
        {
            // Arrange - Event started in 2024 but query is for 2025
            var startTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Monthly, startTime);
            var fromDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var toDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - Should include Mar 1, Apr 1, May 1 (dates in range)
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(d => d >= fromDate && d <= toDate));
        }

        [TestMethod]
        public void CalculateRecurringDates_PreservesTimeOfDay()
        {
            // Arrange - Event at 7:30 PM
            var startTime = new DateTime(2025, 1, 1, 19, 30, 0, DateTimeKind.Utc);
            var evt = CreateRecurringEvent(RecurrencePattern.Weekly, startTime);
            var fromDate = startTime;
            var toDate = startTime.AddDays(21);

            // Act
            var result = _eventsService.CalculateRecurringDates(evt, fromDate, toDate);

            // Assert - All dates should have 7:30 PM time
            Assert.IsTrue(result.All(d => d.Hour == 19 && d.Minute == 30));
        }

        #endregion
    }
}

