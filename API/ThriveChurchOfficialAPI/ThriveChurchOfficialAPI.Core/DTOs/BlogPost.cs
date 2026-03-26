using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Blog post entity stored in the Blogs MongoDB collection
    /// </summary>
    public class BlogPost : ObjectBase
    {
        /// <summary>
        /// Blog post title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Markdown formatted blog post content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The type/source of the blog post
        /// </summary>
        public BlogPostType Type { get; set; }

        /// <summary>
        /// Topic category for filtering
        /// </summary>
        [BsonIgnoreIfNull]
        public BlogPostCategory? Category { get; set; }

        /// <summary>
        /// Source URL reference (e.g., /sermons/{seriesId})
        /// </summary>
        [BsonIgnoreIfNull]
        public string SourceUrl { get; set; }

        /// <summary>
        /// ObjectId reference to the source document
        /// </summary>
        [BsonIgnoreIfNull]
        public string SourceId { get; set; }

        /// <summary>
        /// Timestamp for when this blog post was last updated (UTC)
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this blog post is published and visible to the public
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Date when the blog post was published (UTC)
        /// </summary>
        [BsonIgnoreIfNull]
        public DateTime? PublishedDate { get; set; }

        /// <summary>
        /// Brief excerpt for previews and SEO meta descriptions
        /// </summary>
        [BsonIgnoreIfNull]
        public string Summary { get; set; }

        /// <summary>
        /// URL-friendly slug for SEO routing
        /// </summary>
        public string Slug { get; set; }
    }
}

