using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class LiveSermonsUpdateRequest
    {
        public LiveSermonsUpdateRequest()
        {
            Title = null;
            Slug = null;
        }

        /// <summary>
        /// The requested title 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The requested Video url Slug
        /// </summary>
        public string Slug { get; set; }

        public static bool ValidateRequest(LiveSermonsUpdateRequest request)
        {
            return true;
        }
    }
}
