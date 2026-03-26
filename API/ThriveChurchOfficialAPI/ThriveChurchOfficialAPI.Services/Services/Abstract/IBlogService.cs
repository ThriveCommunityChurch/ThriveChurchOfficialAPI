using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service interface for Blog operations
    /// </summary>
    public interface IBlogService
    {
        /// <summary>
        /// Gets published blog posts with paging
        /// </summary>
        /// <param name="pageNumber">1-based page number</param>
        /// <param name="pageSize">Number of items per page (max 50)</param>
        /// <returns>Paged blog post response</returns>
        Task<SystemResponse<BlogPostPagedResponse>> GetPublishedBlogPosts(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets a single blog post by its URL slug
        /// </summary>
        /// <param name="slug">URL-friendly slug</param>
        /// <returns>The blog post</returns>
        Task<SystemResponse<BlogPost>> GetBlogPostBySlug(string slug);

        /// <summary>
        /// Gets a single blog post by its ID
        /// </summary>
        /// <param name="blogId">The blog post ID</param>
        /// <returns>The blog post</returns>
        Task<SystemResponse<BlogPost>> GetBlogPostById(string blogId);

        /// <summary>
        /// Searches published blog posts by keyword
        /// </summary>
        /// <param name="query">Search keyword</param>
        /// <param name="pageNumber">1-based page number</param>
        /// <param name="pageSize">Number of items per page (max 50)</param>
        /// <returns>Paged blog post search results</returns>
        Task<SystemResponse<BlogPostPagedResponse>> SearchBlogPosts(string query, int pageNumber = 1, int pageSize = 10);
    }
}

