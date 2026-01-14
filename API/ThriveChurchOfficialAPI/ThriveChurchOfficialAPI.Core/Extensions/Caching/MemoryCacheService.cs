using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// In-memory cache service implementation using IMemoryCache.
    /// Used for local development or as a fallback when Redis is unavailable.
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly double _standardCacheTimeout;
        private readonly double _persistentCacheTimeout;

        /// <summary>
        /// C'tor
        /// </summary>
        public MemoryCacheService(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;

            // Parse timeout values from configuration, with sensible defaults
            if (!double.TryParse(configuration["CacheTimeout"], out _standardCacheTimeout) || _standardCacheTimeout == 0)
            {
                _standardCacheTimeout = 60.0; // 1 minute default
            }

            if (!double.TryParse(configuration["PersistentCacheTimeout"], out _persistentCacheTimeout) || _persistentCacheTimeout == 0)
            {
                _persistentCacheTimeout = 86400.0; // 24 hours default
            }

            Log.Debug("MemoryCacheService initialized. Standard timeout: {Standard}s, Persistent timeout: {Persistent}s",
                _standardCacheTimeout, _persistentCacheTimeout);
        }

        /// <inheritdoc />
        public bool IsDistributed => false;

        /// <inheritdoc />
        public bool CanReadFromCache(string cacheKey)
        {
            return _cache.TryGetValue(cacheKey, out object _);
        }

        /// <inheritdoc />
        public T ReadFromCache<T>(string cacheKey)
        {
            _cache.TryGetValue(cacheKey, out T response);
            return response;
        }

        /// <inheritdoc />
        public T InsertIntoCache<T>(string cacheKey, T item)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_standardCacheTimeout));

            return _cache.Set(cacheKey, item, options);
        }

        /// <inheritdoc />
        public T InsertIntoCache<T>(string cacheKey, T item, TimeSpan expiration)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration);

            return _cache.Set(cacheKey, item, options);
        }

        /// <inheritdoc />
        public void RemoveFromCache(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        /// <inheritdoc />
        public void RemoveByPattern(string pattern)
        {
            // IMemoryCache doesn't support pattern-based removal
            // This is a no-op for memory cache - callers should remove specific keys
            Log.Debug("RemoveByPattern called on MemoryCacheService - pattern matching not supported for in-memory cache. Pattern: {Pattern}", pattern);
        }
    }
}

