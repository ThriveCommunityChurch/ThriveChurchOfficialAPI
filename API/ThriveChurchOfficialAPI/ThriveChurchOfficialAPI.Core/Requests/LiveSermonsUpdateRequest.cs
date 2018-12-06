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
        
        /// <summary>
        /// Validates the requested object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool ValidateRequest(LiveSermonsUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                return false;
            }

            return true;
        }
    }
}
