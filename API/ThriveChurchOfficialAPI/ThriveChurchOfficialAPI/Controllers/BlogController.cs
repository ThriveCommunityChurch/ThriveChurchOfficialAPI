using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    /// <summary>
    /// Blog Controller - Manages blog post endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        /// <summary>
        /// Blog Controller Constructor
        /// </summary>
        /// <param name="blogService">Blog service instance</param>
        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        /// <summary>
        /// Get published blog posts with paging
        /// </summary>
        /// <remarks>
        /// Returns published blog posts ordered by PublishedDate descending.
        /// Default page size is 10, max is 50.
        /// </remarks>
        /// <param name="pageNumber">1-based page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 50)</param>
        /// <returns>Paged blog post response</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("published")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<BlogPostPagedResponse>> GetPublishedBlogPosts(int pageNumber = 1, int pageSize = 10)
        {
            var response = await _blogService.GetPublishedBlogPosts(pageNumber, pageSize);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get a blog post by its URL slug
        /// </summary>
        /// <remarks>
        /// Retrieves a single published blog post by its SEO-friendly URL slug.
        /// </remarks>
        /// <param name="slug">The URL-friendly slug</param>
        /// <returns>The blog post</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<BlogPost>> GetBlogPostBySlug(string slug)
        {
            var response = await _blogService.GetBlogPostBySlug(slug);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get a blog post by its ID
        /// </summary>
        /// <param name="blogId">The blog post ID</param>
        /// <returns>The blog post</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("{blogId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<BlogPost>> GetBlogPostById(string blogId)
        {
            var response = await _blogService.GetBlogPostById(blogId);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Search published blog posts
        /// </summary>
        /// <remarks>
        /// Searches published blog posts by keyword in Title, Summary, or Content.
        /// Results are ordered by PublishedDate descending.
        /// </remarks>
        /// <param name="query">Search keyword</param>
        /// <param name="pageNumber">1-based page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 50)</param>
        /// <returns>Paged blog post search results</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<BlogPostPagedResponse>> SearchBlogPosts(string query, int pageNumber = 1, int pageSize = 10)
        {
            var response = await _blogService.SearchBlogPosts(query, pageNumber, pageSize);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }
    }
}

