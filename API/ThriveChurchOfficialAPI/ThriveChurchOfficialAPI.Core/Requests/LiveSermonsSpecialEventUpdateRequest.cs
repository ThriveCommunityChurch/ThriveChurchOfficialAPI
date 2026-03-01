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
        public ValidationResponse ValidateRequest()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(Title)));
            }

            if (string.IsNullOrWhiteSpace(Slug))
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(Slug)));
            }

            if (SpecialEventTimes == null)
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(SpecialEventTimes)));
            }

            return new ValidationResponse("Success!");
        }
    }
}
