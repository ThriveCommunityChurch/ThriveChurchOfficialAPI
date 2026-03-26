using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service implementation for Blog operations
    /// </summary>
    public class BlogService : BaseService, IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly ICacheService _cache;

        // Cache TTLs for blog data
        private static readonly TimeSpan BlogListCacheTTL = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan BlogItemCacheTTL = TimeSpan.FromHours(2);
        private static readonly TimeSpan BlogSearchCacheTTL = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Blog Service constructor
        /// </summary>
        /// <param name="blogRepository">Blog repository</param>
        /// <param name="cache">Cache service</param>
        public BlogService(IBlogRepository blogRepository, ICacheService cache)
        {
            _blogRepository = blogRepository;
            _cache = cache;
        }

        /// <inheritdoc />
        public async Task<SystemResponse<BlogPostPagedResponse>> GetPublishedBlogPosts(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1)
            {
                return new SystemResponse<BlogPostPagedResponse>(true,
                    string.Format(SystemMessages.IllogicalPagingNumber, pageNumber));
            }

            if (pageSize < 1 || pageSize > 50)
            {
                pageSize = 10;
            }

            var cacheKey = CacheKeys.Format(CacheKeys.BlogPublished, pageNumber, pageSize);
            var cached = _cache.ReadFromCache<BlogPostPagedResponse>(cacheKey);
            if (cached != null)
            {
                return new SystemResponse<BlogPostPagedResponse>(cached, "Success!");
            }

            var result = await _blogRepository.GetPublishedBlogPosts(pageNumber, pageSize);

            if (result.TotalCount > 0 && pageNumber > result.TotalPages)
            {
                return new SystemResponse<BlogPostPagedResponse>(true,
                    string.Format(SystemMessages.InvalidPagingNumber, pageNumber, result.TotalPages));
            }

            _cache.InsertIntoCache(cacheKey, result, BlogListCacheTTL);

            return new SystemResponse<BlogPostPagedResponse>(result, "Success!");
        }

        /// <inheritdoc />
        public async Task<SystemResponse<BlogPost>> GetBlogPostBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return new SystemResponse<BlogPost>(true,
                    string.Format(SystemMessages.NullProperty, "Slug"));
            }

            var cacheKey = CacheKeys.Format(CacheKeys.BlogBySlug, slug);
            var cached = _cache.ReadFromCache<BlogPost>(cacheKey);
            if (cached != null)
            {
                return new SystemResponse<BlogPost>(cached, "Success!");
            }

            var blogPost = await _blogRepository.GetBlogPostBySlug(slug);
            if (blogPost == null)
            {
                return new SystemResponse<BlogPost>(true,
                    string.Format(SystemMessages.UnableToFind, $"blog post with slug '{slug}'"));
            }

            _cache.InsertIntoCache(cacheKey, blogPost, BlogItemCacheTTL);

            return new SystemResponse<BlogPost>(blogPost, "Success!");
        }

        /// <inheritdoc />
        public async Task<SystemResponse<BlogPost>> GetBlogPostById(string blogId)
        {
            if (string.IsNullOrWhiteSpace(blogId))
            {
                return new SystemResponse<BlogPost>(true,
                    string.Format(SystemMessages.NullProperty, "BlogId"));
            }

            var validId = ObjectId.TryParse(blogId, out ObjectId _);
            if (!validId)
            {
                return new SystemResponse<BlogPost>(true,
                    string.Format(SystemMessages.InvalidPropertyType, "BlogId", "ObjectId"));
            }

            var cacheKey = CacheKeys.Format(CacheKeys.BlogItem, blogId);
            var cached = _cache.ReadFromCache<BlogPost>(cacheKey);
            if (cached != null)
            {
                return new SystemResponse<BlogPost>(cached, "Success!");
            }

            var blogPost = await _blogRepository.GetBlogPostById(blogId);
            if (blogPost == null)
            {
                return new SystemResponse<BlogPost>(true,
                    string.Format(SystemMessages.UnableToFindPropertyForId, "blog post", blogId));
            }

            _cache.InsertIntoCache(cacheKey, blogPost, BlogItemCacheTTL);

            return new SystemResponse<BlogPost>(blogPost, "Success!");
        }

        /// <inheritdoc />
        public async Task<SystemResponse<BlogPostPagedResponse>> SearchBlogPosts(string query, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new SystemResponse<BlogPostPagedResponse>(true,
                    string.Format(SystemMessages.NullProperty, "Query"));
            }

            if (pageNumber < 1)
            {
                return new SystemResponse<BlogPostPagedResponse>(true,
                    string.Format(SystemMessages.IllogicalPagingNumber, pageNumber));
            }

            if (pageSize < 1 || pageSize > 50)
            {
                pageSize = 10;
            }

            var cacheKey = CacheKeys.Format(CacheKeys.BlogSearch, query, pageNumber, pageSize);
            var cached = _cache.ReadFromCache<BlogPostPagedResponse>(cacheKey);
            if (cached != null)
            {
                return new SystemResponse<BlogPostPagedResponse>(cached, "Success!");
            }

            var result = await _blogRepository.SearchBlogPosts(query, pageNumber, pageSize);

            _cache.InsertIntoCache(cacheKey, result, BlogSearchCacheTTL);

            return new SystemResponse<BlogPostPagedResponse>(result, "Success!");
        }
    }
}

