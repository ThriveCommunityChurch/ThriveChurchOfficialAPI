using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Abstraction for caching operations. Allows switching between
    /// in-memory cache (development) and Redis (production).
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Reads object from cache using key
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="cacheKey">The cache key</param>
        /// <returns>The cached object, or default if not found</returns>
        T ReadFromCache<T>(string cacheKey);

        /// <summary>
        /// Set object in the cache using key with standard cache length (configurable, default 60s)
        /// </summary>
        /// <typeparam name="T">The type of object to cache</typeparam>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="item">The item to cache</param>
        /// <returns>The cached item</returns>
        T InsertIntoCache<T>(string cacheKey, T item);

        /// <summary>
        /// Set object in the cache using key with a custom expiration time
        /// </summary>
        /// <typeparam name="T">The type of object to cache</typeparam>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="item">The item to cache</param>
        /// <param name="expiration">Custom expiration timespan</param>
        /// <returns>The cached item</returns>
        T InsertIntoCache<T>(string cacheKey, T item, TimeSpan expiration);

        /// <summary>
        /// Removes an object from the cache by key
        /// </summary>
        /// <param name="cacheKey">The cache key to remove</param>
        void RemoveFromCache(string cacheKey);

        /// <summary>
        /// Removes all cache entries matching a pattern (e.g., "thrive:sermons:*")
        /// Only supported by Redis; memory cache implementation will be a no-op.
        /// </summary>
        /// <param name="pattern">The key pattern to match</param>
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Indicates whether this cache service uses distributed caching (e.g., Redis)
        /// </summary>
        bool IsDistributed { get; }
    }
}

