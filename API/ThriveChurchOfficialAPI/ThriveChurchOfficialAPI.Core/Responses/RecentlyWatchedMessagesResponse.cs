using System;
using System.Collections.Generic;
using System.Text;
using ThriveChurchOfficialAPI.Core.DTOs;

namespace ThriveChurchOfficialAPI.Core
{
    public class RecentlyWatchedMessagesResponse
    {
        public RecentlyWatchedMessagesResponse()
        {
            RecentMessages = null;
        }

        /// <summary>
        /// A collection of message ids in irder of most recently played, limited to a length of 3
        /// </summary>
        public IEnumerable<RecentMessage> RecentMessages { get; set; }
    }
}
