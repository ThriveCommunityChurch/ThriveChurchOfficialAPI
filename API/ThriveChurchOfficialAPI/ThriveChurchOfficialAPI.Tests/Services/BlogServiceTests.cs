using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    [TestClass]
    public class BlogServiceTests
    {
        private Mock<IBlogRepository> _mockBlogRepository;
        private Mock<ICacheService> _mockCache;
        private BlogService _blogService;

        [TestInitialize]
        public void Setup()
        {
            _mockBlogRepository = new Mock<IBlogRepository>();
            _mockCache = new Mock<ICacheService>();

            _mockCache.Setup(c => c.InsertIntoCache(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
                .Returns((string key, object item, TimeSpan exp) => item);

            _blogService = new BlogService(
                _mockBlogRepository.Object,
                _mockCache.Object
            );
        }

        private static BlogPost CreateTestBlogPost(string id, string slug, string title)
        {
            return new BlogPost
            {
                Id = id,
                Slug = slug,
                Title = title,
                Content = $"Content for {title}",
                Summary = $"Summary for {title}",
                Type = BlogPostType.SermonSeries,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-1),
                LastUpdated = DateTime.UtcNow
            };
        }

        private static BlogPostPagedResponse CreateTestPagedResponse(int totalCount, int pageNumber, int pageSize)
        {
            var items = new List<BlogPost>();
            for (int i = 0; i < Math.Min(pageSize, totalCount); i++)
            {
                items.Add(CreateTestBlogPost($"id{i}", $"slug-{i}", $"Blog Post {i}"));
            }
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            return new BlogPostPagedResponse
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }

        #region GetPublishedBlogPosts

        [TestMethod]
        public async Task GetPublishedBlogPosts_ValidPaging_ReturnsPagedResults()
        {
            var pagedResponse = CreateTestPagedResponse(25, 1, 10);
            _mockBlogRepository.Setup(r => r.GetPublishedBlogPosts(1, 10)).ReturnsAsync(pagedResponse);
            var result = await _blogService.GetPublishedBlogPosts(1, 10);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(25, result.Result.TotalCount);
            Assert.AreEqual(10, result.Result.Items.Count);
        }

        [TestMethod]
        public async Task GetPublishedBlogPosts_PageNumberLessThan1_ReturnsError()
        {
            var result = await _blogService.GetPublishedBlogPosts(0, 10);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.IllogicalPagingNumber, 0), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetPublishedBlogPosts_NegativePageNumber_ReturnsError()
        {
            var result = await _blogService.GetPublishedBlogPosts(-1, 10);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.IllogicalPagingNumber, -1), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetPublishedBlogPosts_PageSizeTooLarge_DefaultsTo10()
        {
            var pagedResponse = CreateTestPagedResponse(5, 1, 10);
            _mockBlogRepository.Setup(r => r.GetPublishedBlogPosts(1, 10)).ReturnsAsync(pagedResponse);
            var result = await _blogService.GetPublishedBlogPosts(1, 100);
            Assert.IsFalse(result.HasErrors);
            _mockBlogRepository.Verify(r => r.GetPublishedBlogPosts(1, 10), Times.Once);
        }

        [TestMethod]
        public async Task GetPublishedBlogPosts_PageSizeLessThan1_DefaultsTo10()
        {
            var pagedResponse = CreateTestPagedResponse(5, 1, 10);
            _mockBlogRepository.Setup(r => r.GetPublishedBlogPosts(1, 10)).ReturnsAsync(pagedResponse);
            var result = await _blogService.GetPublishedBlogPosts(1, 0);
            Assert.IsFalse(result.HasErrors);
            _mockBlogRepository.Verify(r => r.GetPublishedBlogPosts(1, 10), Times.Once);
        }

        [TestMethod]
        public async Task GetPublishedBlogPosts_PageExceedsTotalPages_ReturnsError()
        {
            var pagedResponse = CreateTestPagedResponse(25, 5, 10);
            pagedResponse.TotalPages = 3;
            _mockBlogRepository.Setup(r => r.GetPublishedBlogPosts(5, 10)).ReturnsAsync(pagedResponse);
            var result = await _blogService.GetPublishedBlogPosts(5, 10);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.InvalidPagingNumber, 5, 3), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetPublishedBlogPosts_CacheHit_ReturnsCachedResult()
        {
            var cachedResponse = CreateTestPagedResponse(10, 1, 10);
            _mockCache.Setup(c => c.ReadFromCache<BlogPostPagedResponse>(It.IsAny<string>()))
                .Returns(cachedResponse);
            var result = await _blogService.GetPublishedBlogPosts(1, 10);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(10, result.Result.TotalCount);
            _mockBlogRepository.Verify(r => r.GetPublishedBlogPosts(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region GetBlogPostBySlug

        [TestMethod]
        public async Task GetBlogPostBySlug_ValidSlug_ReturnsBlogPost()
        {
            var blogPost = CreateTestBlogPost("id1", "test-slug", "Test Blog");
            _mockBlogRepository.Setup(r => r.GetBlogPostBySlug("test-slug")).ReturnsAsync(blogPost);
            var result = await _blogService.GetBlogPostBySlug("test-slug");
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("test-slug", result.Result.Slug);
            Assert.AreEqual("Test Blog", result.Result.Title);
        }

        [TestMethod]
        public async Task GetBlogPostBySlug_NullSlug_ReturnsError()
        {
            var result = await _blogService.GetBlogPostBySlug(null);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "Slug"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetBlogPostBySlug_EmptySlug_ReturnsError()
        {
            var result = await _blogService.GetBlogPostBySlug(string.Empty);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "Slug"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetBlogPostBySlug_WhitespaceSlug_ReturnsError()
        {
            var result = await _blogService.GetBlogPostBySlug("   ");
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "Slug"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetBlogPostBySlug_NotFound_ReturnsError()
        {
            _mockBlogRepository.Setup(r => r.GetBlogPostBySlug("nonexistent")).ReturnsAsync((BlogPost)null);
            var result = await _blogService.GetBlogPostBySlug("nonexistent");
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.UnableToFind, "blog post with slug 'nonexistent'"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetBlogPostBySlug_CacheHit_ReturnsCachedResult()
        {
            var cachedPost = CreateTestBlogPost("id1", "cached-slug", "Cached Blog");
            _mockCache.Setup(c => c.ReadFromCache<BlogPost>(It.IsAny<string>())).Returns(cachedPost);
            var result = await _blogService.GetBlogPostBySlug("cached-slug");
            Assert.IsNotNull(result);
            Assert.AreEqual("Cached Blog", result.Result.Title);
            _mockBlogRepository.Verify(r => r.GetBlogPostBySlug(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region GetBlogPostById

        [TestMethod]
        public async Task GetBlogPostById_ValidId_ReturnsBlogPost()
        {
            var blogId = "507f1f77bcf86cd799439011";
            var blogPost = CreateTestBlogPost(blogId, "test-slug", "Test Blog");
            _mockBlogRepository.Setup(r => r.GetBlogPostById(blogId)).ReturnsAsync(blogPost);
            var result = await _blogService.GetBlogPostById(blogId);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(blogId, result.Result.Id);
        }

        [TestMethod]
        public async Task GetBlogPostById_NullId_ReturnsError()
        {
            var result = await _blogService.GetBlogPostById(null);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "BlogId"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetBlogPostById_EmptyId_ReturnsError()
        {
            var result = await _blogService.GetBlogPostById(string.Empty);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "BlogId"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetBlogPostById_NotFound_ReturnsError()
        {
            var blogId = "507f1f77bcf86cd799439011";
            _mockBlogRepository.Setup(r => r.GetBlogPostById(blogId)).ReturnsAsync((BlogPost)null);
            var result = await _blogService.GetBlogPostById(blogId);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.UnableToFindPropertyForId, "blog post", blogId), result.ErrorMessage);
        }

        #endregion

        #region SearchBlogPosts

        [TestMethod]
        public async Task SearchBlogPosts_ValidQuery_ReturnsResults()
        {
            var pagedResponse = CreateTestPagedResponse(3, 1, 10);
            _mockBlogRepository.Setup(r => r.SearchBlogPosts("grace", 1, 10)).ReturnsAsync(pagedResponse);
            var result = await _blogService.SearchBlogPosts("grace", 1, 10);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(3, result.Result.TotalCount);
        }

        [TestMethod]
        public async Task SearchBlogPosts_NullQuery_ReturnsError()
        {
            var result = await _blogService.SearchBlogPosts(null, 1, 10);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "Query"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task SearchBlogPosts_EmptyQuery_ReturnsError()
        {
            var result = await _blogService.SearchBlogPosts(string.Empty, 1, 10);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "Query"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task SearchBlogPosts_InvalidPageNumber_ReturnsError()
        {
            var result = await _blogService.SearchBlogPosts("grace", 0, 10);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.IllogicalPagingNumber, 0), result.ErrorMessage);
        }

        [TestMethod]
        public async Task SearchBlogPosts_PageSizeTooLarge_DefaultsTo10()
        {
            var pagedResponse = CreateTestPagedResponse(3, 1, 10);
            _mockBlogRepository.Setup(r => r.SearchBlogPosts("grace", 1, 10)).ReturnsAsync(pagedResponse);
            var result = await _blogService.SearchBlogPosts("grace", 1, 100);
            Assert.IsFalse(result.HasErrors);
            _mockBlogRepository.Verify(r => r.SearchBlogPosts("grace", 1, 10), Times.Once);
        }

        [TestMethod]
        public async Task SearchBlogPosts_CacheHit_ReturnsCachedResult()
        {
            var cachedResponse = CreateTestPagedResponse(5, 1, 10);
            _mockCache.Setup(c => c.ReadFromCache<BlogPostPagedResponse>(It.IsAny<string>()))
                .Returns(cachedResponse);
            var result = await _blogService.SearchBlogPosts("grace", 1, 10);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            _mockBlogRepository.Verify(r => r.SearchBlogPosts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        #endregion
    }
}
