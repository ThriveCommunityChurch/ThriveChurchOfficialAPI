using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Repository for Blogs collection operations
    /// </summary>
    public class BlogRepository : RepositoryBase<BlogPost>, IBlogRepository
    {
        private readonly IMongoCollection<BlogPost> _blogsCollection;

        /// <summary>
        /// Blog Repository C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public BlogRepository(IConfiguration Configuration)
            : base(Configuration)
        {
            // Register class map for proper ObjectId to string deserialization on SourceId
            if (!BsonClassMap.IsClassMapRegistered(typeof(BlogPost)))
            {
                BsonClassMap.RegisterClassMap<BlogPost>(cm =>
                {
                    cm.AutoMap();
                    cm.GetMemberMap(c => c.SourceId).SetSerializer(
                        new MongoDB.Bson.Serialization.Serializers.StringSerializer(BsonType.ObjectId));
                });
            }

            _blogsCollection = DB.GetCollection<BlogPost>("Blogs");

            // Create indexes for better query performance
            CreateIndexesAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create indexes for the Blogs collection
        /// </summary>
        private async Task CreateIndexesAsync()
        {
            var indexModels = new List<CreateIndexModel<BlogPost>>
            {
                // Unique index on Slug for SEO lookups
                new CreateIndexModel<BlogPost>(
                    Builders<BlogPost>.IndexKeys.Ascending(b => b.Slug),
                    new CreateIndexOptions { Name = IndexKeys.BlogsBySlugAsc_Unique, Unique = true }),

                // Index on IsPublished for filtering
                new CreateIndexModel<BlogPost>(
                    Builders<BlogPost>.IndexKeys.Ascending(b => b.IsPublished),
                    new CreateIndexOptions { Name = IndexKeys.BlogsByIsPublishedAsc }),

                // Index on PublishedDate for sorting
                new CreateIndexModel<BlogPost>(
                    Builders<BlogPost>.IndexKeys.Descending(b => b.PublishedDate),
                    new CreateIndexOptions { Name = IndexKeys.BlogsByPublishedDateDesc }),

                // Compound index for published posts sorted by date (most common query)
                new CreateIndexModel<BlogPost>(
                    Builders<BlogPost>.IndexKeys
                        .Ascending(b => b.IsPublished)
                        .Descending(b => b.PublishedDate),
                    new CreateIndexOptions { Name = IndexKeys.BlogsByIsPublishedAndPublishedDate })
            };

            await _blogsCollection.Indexes.CreateManyAsync(indexModels);
        }

        /// <inheritdoc />
        public async Task<BlogPostPagedResponse> GetPublishedBlogPosts(int pageNumber, int pageSize)
        {
            var filter = Builders<BlogPost>.Filter.Eq(b => b.IsPublished, true);
            var sort = Builders<BlogPost>.Sort.Descending(b => b.PublishedDate);

            var totalCount = await _blogsCollection.CountDocumentsAsync(filter);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await _blogsCollection
                .Find(filter)
                .Sort(sort)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new BlogPostPagedResponse
            {
                Items = items,
                TotalCount = (int)totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }

        /// <inheritdoc />
        public async Task<BlogPost> GetBlogPostBySlug(string slug)
        {
            var filter = Builders<BlogPost>.Filter.Eq(b => b.Slug, slug);
            return await _blogsCollection.Find(filter).FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<BlogPost> GetBlogPostById(string blogId)
        {
            var filter = Builders<BlogPost>.Filter.Eq(b => b.Id, blogId);
            return await _blogsCollection.Find(filter).FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<BlogPostPagedResponse> SearchBlogPosts(string query, int pageNumber, int pageSize)
        {
            var escapedQuery = Regex.Escape(query);
            var regex = new BsonRegularExpression(escapedQuery, "i");

            var filter = Builders<BlogPost>.Filter.And(
                Builders<BlogPost>.Filter.Eq(b => b.IsPublished, true),
                Builders<BlogPost>.Filter.Or(
                    Builders<BlogPost>.Filter.Regex(b => b.Title, regex),
                    Builders<BlogPost>.Filter.Regex(b => b.Summary, regex),
                    Builders<BlogPost>.Filter.Regex(b => b.Content, regex)
                )
            );

            var sort = Builders<BlogPost>.Sort.Descending(b => b.PublishedDate);
            var totalCount = await _blogsCollection.CountDocumentsAsync(filter);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await _blogsCollection
                .Find(filter)
                .Sort(sort)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new BlogPostPagedResponse
            {
                Items = items,
                TotalCount = (int)totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }
    }
}

