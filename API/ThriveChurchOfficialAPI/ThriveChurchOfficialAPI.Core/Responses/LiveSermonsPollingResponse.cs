using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class LiveSermonsPollingResponse
    {
        public LiveSermonsPollingResponse()
        {
            IsLive = false;
        }
        
        /// <summary>
        /// Expiry time for the LiveSermon object, in UTC
        /// </summary>
        public DateTime StreamExpirationTime { get; set; }

        /// <summary>
        /// Is the stream currently active at this moment
        /// </summary>
        public bool IsLive { get; set; }
    }
}
