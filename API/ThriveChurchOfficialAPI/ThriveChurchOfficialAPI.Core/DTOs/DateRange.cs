using System;

namespace ThriveChurchOfficialAPI.Core
{
    public class DateRange
    {
        public DateRange()
        {
            Start = null;
            End = null;
        }

        /// <summary>
        /// The start of the range
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        /// The end of the range
        /// </summary>
        public DateTime? End { get; set; }
    }
}