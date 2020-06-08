using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class LiveSermonsSchedulingRequest
    {
        /// <summary>
        /// Schedule in which a live stream is activated (CRON format)
        /// </summary>
        public string StartSchedule { get; set; }

        /// <summary>
        /// Automated schedule in which a live stream is ended (CRON format)
        /// </summary>
        public string EndSchedule { get; set; }
    }
}