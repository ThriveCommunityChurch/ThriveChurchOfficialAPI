using MongoDB.Bson;
using System;
using System.ComponentModel.DataAnnotations;

namespace ThriveChurchOfficialAPI.Core
{
    public class LiveSermonsUpdateRequest
    {
        public LiveSermonsUpdateRequest()
        {
            // init a new object to contain our default end time of 11:20 EST which will become UTC
            ExpirationTime = new DateTime(1990, 01, 01, 11, 20, 0, 0);
        }

        /// <summary>
        /// Set an expiration time for the sermon series. 
        /// The only significant piece here is the TIME, not the date
        /// </summary>
        public DateTime? ExpirationTime { get; set; }
        
        /// <summary>
        /// Validates the requested object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ValidationResponse ValidateRequest(LiveSermonsUpdateRequest request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            return new ValidationResponse("Success!");
        }
    }
}
