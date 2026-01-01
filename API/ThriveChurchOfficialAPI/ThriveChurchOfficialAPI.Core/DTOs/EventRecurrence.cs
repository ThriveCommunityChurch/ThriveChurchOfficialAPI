using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Defines recurrence settings for recurring events
    /// </summary>
    public class EventRecurrence
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public EventRecurrence()
        {
            Pattern = RecurrencePattern.None;
            DayOfWeek = null;
            DayOfMonth = null;
            MonthOfYear = null;
            Interval = 1;
            EndDate = null;
        }

        /// <summary>
        /// The recurrence pattern (Daily, Weekly, Monthly, etc.)
        /// </summary>
        public RecurrencePattern Pattern { get; set; }

        /// <summary>
        /// The day of the week for weekly recurrence (0 = Sunday, 6 = Saturday)
        /// </summary>
        public int? DayOfWeek { get; set; }

        /// <summary>
        /// The day of the month for monthly recurrence (1-31)
        /// </summary>
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// The month of the year for yearly recurrence (1-12)
        /// </summary>
        public int? MonthOfYear { get; set; }

        /// <summary>
        /// The interval between recurrences (e.g., every 2 weeks)
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// The date when the recurrence ends (optional)
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}

