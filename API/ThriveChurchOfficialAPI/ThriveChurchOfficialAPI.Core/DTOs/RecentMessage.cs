using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core.DTOs
{
    public class RecentMessage
    {
        public RecentMessage()
        {
            MessageId = null;
            WatchTime = null;
        }

        /// <summary>
        /// Message Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Current watch time in seconds
        /// </summary>
        public int? WatchTime { get; set; }
    }
}
