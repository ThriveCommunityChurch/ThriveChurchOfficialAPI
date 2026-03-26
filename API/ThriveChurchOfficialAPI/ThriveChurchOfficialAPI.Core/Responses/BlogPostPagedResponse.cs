using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Paged response for blog post queries
    /// </summary>
    public class BlogPostPagedResponse
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public BlogPostPagedResponse()
        {
            Items = new List<BlogPost>();
        }

        /// <summary>
        /// Collection of blog posts for this page
        /// </summary>
        public List<BlogPost> Items { get; set; }

        /// <summary>
        /// Total number of matching blog posts across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages available
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Whether there is a next page available
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Whether there is a previous page available
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
}

