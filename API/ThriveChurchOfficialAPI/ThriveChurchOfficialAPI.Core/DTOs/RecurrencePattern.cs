namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Defines the recurrence pattern for recurring events
    /// </summary>
    public enum RecurrencePattern
    {
        /// <summary>
        /// Event does not recur
        /// </summary>
        None = 0,

        /// <summary>
        /// Event recurs daily
        /// </summary>
        Daily = 1,

        /// <summary>
        /// Event recurs weekly on a specific day
        /// </summary>
        Weekly = 2,

        /// <summary>
        /// Event recurs every two weeks
        /// </summary>
        BiWeekly = 3,

        /// <summary>
        /// Event recurs monthly on a specific day
        /// </summary>
        Monthly = 4,

        /// <summary>
        /// Event recurs yearly on a specific date
        /// </summary>
        Yearly = 5
    }
}

