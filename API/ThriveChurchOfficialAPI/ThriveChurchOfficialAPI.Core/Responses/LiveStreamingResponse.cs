using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class LiveStreamingResponse
    {
        public LiveStreamingResponse()
        {
            IsLive = false;
            Title = null;
            SpecialEventTimes = null;
            IsSpecialEvent = false;
        }

        /// <summary>
        /// Flag determining whether or not a stream is currently active
        /// </summary>
        public bool IsLive { get; set; }

        /// <summary>
        /// Full path to the video that is currently streaming
        /// </summary>
        public string VideoUrl { get; set; }

        /// <summary>
        /// The title of the video / message
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Set a time when a livestream notification will disappear,
        /// after this time when a user loads the page the notification will disappear.
        /// We will only use the time here.
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// Used to determine if this stream is for one that might not be during the normal Sunday times
        /// </summary>
        public bool IsSpecialEvent { get; set; }

        /// <summary>
        /// The start and end times of the special event that is to be broadcast
        /// if null => then this is not a special event
        /// </summary>
        public DateRange SpecialEventTimes { get; set; }
    }
}