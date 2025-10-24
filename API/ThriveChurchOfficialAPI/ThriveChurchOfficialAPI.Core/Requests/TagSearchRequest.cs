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
        /// Specifies the type of search to perform
        /// </summary>
        [Required]
        public SearchTarget SearchTarget { get; set; }

        /// <summary>
        /// Collection of tags to filter by (required when SearchTarget is Messages or Series)
        /// </summary>
        public IEnumerable<MessageTag> Tags { get; set; }

        /// <summary>
        /// Search value (used for speaker name when SearchTarget is Speaker)
        /// In the future, will be used for title/name filtering with Messages/Series
        /// </summary>
        public string SearchValue { get; set; }

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

            // Validate based on SearchTarget
            if (request.SearchTarget == SearchTarget.Messages || request.SearchTarget == SearchTarget.Series)
            {
                // Tags are required for tag-based searches
                if (request.Tags == null || !request.Tags.Any())
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(Tags)));
                }

                // Check for Unknown tags (custom message since this is a specific business rule)
                if (request.Tags.Any(t => t == MessageTag.Unknown))
                {
                    return new ValidationResponse(true, "Unknown tag type is not supported for search");
                }

                // SearchValue is optional for now (will be used for title filtering in the future)
            }
            else if (request.SearchTarget == SearchTarget.Speaker)
            {
                // SearchValue is required for speaker searches
                if (string.IsNullOrWhiteSpace(request.SearchValue))
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(SearchValue)));
                }

                // Validate speaker name length
                if (request.SearchValue.Trim().Length < 2 || request.SearchValue.Trim().Length > 100)
                {
                    return new ValidationResponse(true, "Speaker name must be between 2 and 100 characters");
                }
            }

            return new ValidationResponse("Success!");
        }
    }
}

