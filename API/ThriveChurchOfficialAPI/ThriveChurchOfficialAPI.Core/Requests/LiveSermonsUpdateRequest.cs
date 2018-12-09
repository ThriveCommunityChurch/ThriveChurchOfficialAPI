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
            Id = null;
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
        /// The Id of the LiveSermon object in Mongo
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Validates the requested object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool ValidateRequest(LiveSermonsUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return false;
            }

            return true;
        }
    }
}
