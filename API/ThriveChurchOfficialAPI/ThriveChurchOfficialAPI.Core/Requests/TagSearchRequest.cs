using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Request object for searching messages or series by tags
    /// </summary>
    public class TagSearchRequest
    {
        public TagSearchRequest()
        {
            Tags = new List<MessageTag>();
        }
        
        /// <summary>
        /// Specifies whether to search Messages or Series
        /// </summary>
        [Required]
        public SearchTarget SearchTarget { get; set; }
        
        /// <summary>
        /// Collection of tags to filter by (messages/series must match at least one tag)
        /// </summary>
        [Required]
        public IEnumerable<MessageTag> Tags { get; set; }
        
        /// <summary>
        /// Sort direction for results (by date)
        /// </summary>
        [Required]
        public SortDirection SortDirection { get; set; }
        
        /// <summary>
        /// Validates the request object
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>ValidationResponse indicating success or failure</returns>
        public static ValidationResponse ValidateRequest(TagSearchRequest request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }
            
            if (request.Tags == null || !request.Tags.Any())
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(TagSearchRequest.Tags)));
            }
            
            // Check for Unknown tags (custom message since this is a specific business rule)
            if (request.Tags.Any(t => t == MessageTag.Unknown))
            {
                return new ValidationResponse(true, "Unknown tag type is not supported for search");
            }
            
            return new ValidationResponse("Success!");
        }
    }
}

