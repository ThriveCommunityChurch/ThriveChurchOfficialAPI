using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace ThriveChurchOfficialAPI.Core
{
    public class LiveSermonsUpdateRequest
    {
        public LiveSermonsUpdateRequest()
        {
            Id = null;
        }

        /// <summary>
        /// The Id of the LiveSermon object in Mongo
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'Id'. This property is required.")]
        public string Id { get; set; }
        
        /// <summary>
        /// Validates the requested object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool ValidateRequest(LiveSermonsUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return false;
            }

            if (!ObjectId.TryParse(request.Id, out ObjectId id))
            {
                return false;
            }

            return true;
        }
    }
}
