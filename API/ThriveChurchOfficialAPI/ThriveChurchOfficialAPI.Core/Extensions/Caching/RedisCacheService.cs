using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;
using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Distributed cache service implementation using Redis.
    /// Used for production deployments.
    /// Operations are fail-safe: if Redis is unavailable, operations return gracefully
    /// without throwing exceptions (cache misses return default, writes are no-ops).
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly IServer _server;
        private readonly double _standardCacheTimeout;
        private readonly double _persistentCacheTimeout;
        private readonly JsonSerializerSettings _serializerSettings;
        private bool _connectionWarningLogged = false;

        /// <summary>
        /// C'tor
        /// </summary>
        public RedisCacheService(IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            
            // Get the server for pattern-based operations
            var endpoints = redis.GetEndPoints();
            if (endpoints.Length > 0)
            {
                _server = redis.GetServer(endpoints[0]);
            }

            // Parse timeout values from configuration
            if (!double.TryParse(configuration["CacheTimeout"], out _standardCacheTimeout) || _standardCacheTimeout == 0)
            {
                _standardCacheTimeout = 60.0; // 1 minute default
            }

            if (!double.TryParse(configuration["PersistentCacheTimeout"], out _persistentCacheTimeout) || _persistentCacheTimeout == 0)
            {
                _persistentCacheTimeout = 86400.0; // 24 hours default
            }

            // Configure Newtonsoft to match API serialization settings
            _serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            Log.Information("RedisCacheService initialized. Standard timeout: {Standard}s, Persistent timeout: {Persistent}s",
                _standardCacheTimeout, _persistentCacheTimeout);
        }

        /// <inheritdoc />
        public bool IsDistributed => true;

        private bool IsConnected
        {
            get
            {
                var connected = _redis?.IsConnected ?? false;
                if (!connected && !_connectionWarningLogged)
                {
                    Log.Warning("Redis connection is not available. Cache operations will be skipped.");
                    _connectionWarningLogged = true;
                }
                else if (connected && _connectionWarningLogged)
                {
                    Log.Information("Redis connection restored.");
                    _connectionWarningLogged = false;
                }
                return connected;
            }
        }

        /// <inheritdoc />
        public T ReadFromCache<T>(string cacheKey)
        {
            if (!IsConnected) return default;

            try
            {
                var value = _database.StringGet(cacheKey);
                if (value.IsNullOrEmpty)
                {
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(value, _serializerSettings);
            }
            catch (RedisConnectionException ex)
            {
                Log.Warning("Redis connection error in ReadFromCache for key {CacheKey}: {Message}", cacheKey, ex.Message);
                return default;
            }
            catch (JsonException ex)
            {
                Log.Warning("JSON deserialization error in ReadFromCache for key {CacheKey}: {Message}", cacheKey, ex.Message);
                return default;
            }
        }

        /// <inheritdoc />
        public T InsertIntoCache<T>(string cacheKey, T item)
        {
            return InsertIntoCache(cacheKey, item, TimeSpan.FromSeconds(_standardCacheTimeout));
        }

        /// <inheritdoc />
        public T InsertIntoCache<T>(string cacheKey, T item, TimeSpan expiration)
        {
            if (!IsConnected) return item;

            try
            {
                var serialized = JsonConvert.SerializeObject(item, _serializerSettings);
                _database.StringSet(cacheKey, serialized, expiration);
                return item;
            }
            catch (RedisConnectionException ex)
            {
                Log.Warning("Redis connection error in InsertIntoCache for key {CacheKey}: {Message}", cacheKey, ex.Message);
                return item;
            }
        }

        /// <inheritdoc />
        public void RemoveFromCache(string cacheKey)
        {
            if (!IsConnected) return;

            try
            {
                _database.KeyDelete(cacheKey);
            }
            catch (RedisConnectionException ex)
            {
                Log.Warning("Redis connection error in RemoveFromCache for key {CacheKey}: {Message}", cacheKey, ex.Message);
            }
        }

        /// <inheritdoc />
        public void RemoveByPattern(string pattern)
        {
            if (!IsConnected || _server == null) return;

            try
            {
                // Use SCAN to find matching keys (safer than KEYS for production)
                var keys = _server.Keys(pattern: pattern);
                foreach (var key in keys)
                {
                    _database.KeyDelete(key);
                }
                Log.Debug("Removed cache entries matching pattern: {Pattern}", pattern);
            }
            catch (RedisConnectionException ex)
            {
                Log.Warning("Redis connection error in RemoveByPattern for pattern {Pattern}: {Message}", pattern, ex.Message);
            }
        }
    }
}

