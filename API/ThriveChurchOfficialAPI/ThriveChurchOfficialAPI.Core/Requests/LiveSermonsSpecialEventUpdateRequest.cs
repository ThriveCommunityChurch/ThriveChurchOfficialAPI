using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class LiveSermonsSpecialEventUpdateRequest
    {
        public LiveSermonsSpecialEventUpdateRequest()
        {
            Title = null;
            Slug = null;
            SpecialEventTimes = null;
        }

        /// <summary>
        /// The requested title 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The requested Video url Slug
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// The start and end times of a special event that is to be broadcast. 
        /// The End Date here is the expiration time.
        /// </summary>
        public DateRange SpecialEventTimes { get; set; }

        /// <summary>
        /// Validates the requested object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool ValidateRequest(LiveSermonsSpecialEventUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                return false;
            }

            if (request.SpecialEventTimes == null)
            {
                return false;
            }

            return true;
        }
    }
}
