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
        /// Time that the stream should be done
        /// </summary>
        public DateTime StreamExpirationTime { get; set; }

        /// <summary>
        /// Is the stream currently active at this moment
        /// </summary>
        public bool IsLive { get; set; }
    }
}
