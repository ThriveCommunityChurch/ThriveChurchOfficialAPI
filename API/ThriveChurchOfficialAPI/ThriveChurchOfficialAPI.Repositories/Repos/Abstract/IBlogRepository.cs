using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Interface for Blog repository operations
    /// </summary>
    public interface IBlogRepository
    {
        /// <summary>
        /// Get published blog posts with paging, ordered by PublishedDate descending
        /// </summary>
        /// <param name="pageNumber">1-based page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paged blog post response</returns>
        Task<BlogPostPagedResponse> GetPublishedBlogPosts(int pageNumber, int pageSize);

        /// <summary>
        /// Get a single blog post by its URL slug
        /// </summary>
        /// <param name="slug">URL-friendly slug</param>
        /// <returns>The blog post if found, null otherwise</returns>
        Task<BlogPost> GetBlogPostBySlug(string slug);

        /// <summary>
        /// Get a single blog post by its ID
        /// </summary>
        /// <param name="blogId">The blog post ID</param>
        /// <returns>The blog post if found, null otherwise</returns>
        Task<BlogPost> GetBlogPostById(string blogId);

        /// <summary>
        /// Search published blog posts by keyword in Title, Summary, or Content
        /// </summary>
        /// <param name="query">Search keyword</param>
        /// <param name="pageNumber">1-based page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paged blog post response</returns>
        Task<BlogPostPagedResponse> SearchBlogPosts(string query, int pageNumber, int pageSize);
    }
}

