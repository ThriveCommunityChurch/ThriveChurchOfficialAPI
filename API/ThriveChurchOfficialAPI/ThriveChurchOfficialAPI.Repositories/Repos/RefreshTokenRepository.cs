using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Serilog;
using System;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Refresh token repository for MongoDB operations
    /// </summary>
    public class RefreshTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository
    {
        private readonly IMongoCollection<RefreshToken> _refreshTokens;
        private const string COLLECTION_NAME = "RefreshTokens";

        /// <summary>
        /// Refresh Token Repository Constructor
        /// </summary>
        /// <param name="configuration">Configuration for database connection</param>
        public RefreshTokenRepository(IConfiguration configuration)
            : base(configuration)
        {
            _refreshTokens = DB.GetCollection<RefreshToken>(COLLECTION_NAME);

            // Create indexes for performance
            CreateIndexes();
        }

        /// <summary>
        /// Create database indexes for refresh token collection
        /// </summary>
        private void CreateIndexes()
        {
            try
            {
                // Create unique index on token value
                var tokenIndexKeys = Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.Token);
                var tokenIndexOptions = new CreateIndexOptions { Unique = true, Name = IndexKeys.RefreshTokensByTokenAsc_Unique };
                var tokenIndexModel = new CreateIndexModel<RefreshToken>(tokenIndexKeys, tokenIndexOptions);

                // Create index on user ID for efficient user token lookups
                var userIdIndexKeys = Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.UserId);
                var userIdIndexOptions = new CreateIndexOptions { Name = IndexKeys.RefreshTokensByUserIdAsc };
                var userIdIndexModel = new CreateIndexModel<RefreshToken>(userIdIndexKeys, userIdIndexOptions);

                // Create TTL index on expiration date for automatic cleanup
                var expiresAtIndexKeys = Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.ExpiresAt);
                var expiresAtIndexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = IndexKeys.RefreshTokensByExpiresAtTTL };
                var expiresAtIndexModel = new CreateIndexModel<RefreshToken>(expiresAtIndexKeys, expiresAtIndexOptions);

                // Create compound index for active tokens (not used, not revoked, not expired)
                var activeTokenIndexKeys = Builders<RefreshToken>.IndexKeys
                    .Ascending(rt => rt.IsUsed)
                    .Ascending(rt => rt.IsRevoked)
                    .Ascending(rt => rt.ExpiresAt);
                var activeTokenIndexOptions = new CreateIndexOptions { Name = IndexKeys.RefreshTokensByActiveAsc };
                var activeTokenIndexModel = new CreateIndexModel<RefreshToken>(activeTokenIndexKeys, activeTokenIndexOptions);

                _refreshTokens.Indexes.CreateMany(new[] { 
                    tokenIndexModel, 
                    userIdIndexModel, 
                    expiresAtIndexModel,
                    activeTokenIndexModel 
                });
            }
            catch (Exception ex)
            {
                // Log the error but don't fail startup
                Log.Warning($"Warning: Could not create refresh token indexes: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to create</param>
        /// <returns>Created refresh token with ID</returns>
        public async Task<SystemResponse<RefreshToken>> CreateRefreshTokenAsync(RefreshToken refreshToken)
        {
            try
            {
                if (refreshToken == null)
                {
                    return new SystemResponse<RefreshToken>(true, "Refresh token is required");
                }

                if (string.IsNullOrEmpty(refreshToken.Token))
                {
                    return new SystemResponse<RefreshToken>(true, "Token value is required");
                }

                if (string.IsNullOrEmpty(refreshToken.UserId))
                {
                    return new SystemResponse<RefreshToken>(true, "User ID is required");
                }

                refreshToken.CreatedAt = DateTime.UtcNow;

                await _refreshTokens.InsertOneAsync(refreshToken);

                return new SystemResponse<RefreshToken>(refreshToken, "Refresh token created successfully");
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return new SystemResponse<RefreshToken>(true, "Refresh token already exists");
            }
            catch (Exception ex)
            {
                return new SystemResponse<RefreshToken>(true, $"Error creating refresh token: {ex.Message}");
            }
        }

        /// <summary>
        /// Find a refresh token by token value
        /// </summary>
        /// <param name="token">Token value to search for</param>
        /// <returns>Refresh token if found and valid, null if not found or invalid</returns>
        public async Task<SystemResponse<RefreshToken>> GetRefreshTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return new SystemResponse<RefreshToken>(true, "Token is required");
                }

                var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);
                var refreshToken = await _refreshTokens.Find(filter).FirstOrDefaultAsync();

                if (refreshToken == null)
                {
                    return new SystemResponse<RefreshToken>(true, "Refresh token not found");
                }

                return new SystemResponse<RefreshToken>(refreshToken, "Refresh token found");
            }
            catch (Exception ex)
            {
                return new SystemResponse<RefreshToken>(true, $"Error retrieving refresh token: {ex.Message}");
            }
        }

        /// <summary>
        /// Mark a refresh token as used
        /// </summary>
        /// <param name="tokenId">Token ID to mark as used</param>
        /// <param name="usedByIp">IP address where token was used</param>
        /// <returns>Updated refresh token</returns>
        public async Task<SystemResponse<RefreshToken>> MarkTokenAsUsedAsync(string tokenId, string usedByIp = null)
        {
            try
            {
                if (string.IsNullOrEmpty(tokenId))
                {
                    return new SystemResponse<RefreshToken>(true, "Token ID is required");
                }

                var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Id, tokenId);
                var update = Builders<RefreshToken>.Update
                    .Set(rt => rt.IsUsed, true)
                    .Set(rt => rt.UsedAt, DateTime.UtcNow)
                    .Set(rt => rt.UsedByIp, usedByIp);

                var result = await _refreshTokens.FindOneAndUpdateAsync(filter, update, 
                    new FindOneAndUpdateOptions<RefreshToken> { ReturnDocument = ReturnDocument.After });

                if (result == null)
                {
                    return new SystemResponse<RefreshToken>(true, "Refresh token not found");
                }

                return new SystemResponse<RefreshToken>(result, "Refresh token marked as used");
            }
            catch (Exception ex)
            {
                return new SystemResponse<RefreshToken>(true, $"Error marking token as used: {ex.Message}");
            }
        }

        /// <summary>
        /// Revoke a refresh token
        /// </summary>
        /// <param name="tokenId">Token ID to revoke</param>
        /// <param name="revokedByIp">IP address where token was revoked</param>
        /// <returns>Updated refresh token</returns>
        public async Task<SystemResponse<RefreshToken>> RevokeTokenAsync(string tokenId, string revokedByIp = null)
        {
            try
            {
                if (string.IsNullOrEmpty(tokenId))
                {
                    return new SystemResponse<RefreshToken>(true, "Token ID is required");
                }

                var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Id, tokenId);
                var update = Builders<RefreshToken>.Update
                    .Set(rt => rt.IsRevoked, true)
                    .Set(rt => rt.RevokedAt, DateTime.UtcNow)
                    .Set(rt => rt.RevokedByIp, revokedByIp);

                var result = await _refreshTokens.FindOneAndUpdateAsync(filter, update,
                    new FindOneAndUpdateOptions<RefreshToken> { ReturnDocument = ReturnDocument.After });

                if (result == null)
                {
                    return new SystemResponse<RefreshToken>(true, "Refresh token not found");
                }

                return new SystemResponse<RefreshToken>(result, "Refresh token revoked");
            }
            catch (Exception ex)
            {
                return new SystemResponse<RefreshToken>(true, $"Error revoking token: {ex.Message}");
            }
        }

        /// <summary>
        /// Revoke all refresh tokens for a user
        /// </summary>
        /// <param name="userId">User ID whose tokens should be revoked</param>
        /// <param name="revokedByIp">IP address where tokens were revoked</param>
        /// <returns>Number of tokens revoked</returns>
        public async Task<SystemResponse<int>> RevokeAllUserTokensAsync(string userId, string revokedByIp = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new SystemResponse<int>(true, "User ID is required");
                }

                var filter = Builders<RefreshToken>.Filter.And(
                    Builders<RefreshToken>.Filter.Eq(rt => rt.UserId, userId),
                    Builders<RefreshToken>.Filter.Eq(rt => rt.IsRevoked, false)
                );

                var update = Builders<RefreshToken>.Update
                    .Set(rt => rt.IsRevoked, true)
                    .Set(rt => rt.RevokedAt, DateTime.UtcNow)
                    .Set(rt => rt.RevokedByIp, revokedByIp);

                var result = await _refreshTokens.UpdateManyAsync(filter, update);

                return new SystemResponse<int>((int)result.ModifiedCount, $"Revoked {result.ModifiedCount} refresh tokens");
            }
            catch (Exception ex)
            {
                return new SystemResponse<int>(true, $"Error revoking user tokens: {ex.Message}");
            }
        }

        /// <summary>
        /// Clean up expired refresh tokens
        /// </summary>
        /// <returns>Number of tokens cleaned up</returns>
        public async Task<SystemResponse<int>> CleanupExpiredTokensAsync()
        {
            try
            {
                var filter = Builders<RefreshToken>.Filter.Lt(rt => rt.ExpiresAt, DateTime.UtcNow);
                var result = await _refreshTokens.DeleteManyAsync(filter);

                return new SystemResponse<int>((int)result.DeletedCount, $"Cleaned up {result.DeletedCount} expired tokens");
            }
            catch (Exception ex)
            {
                return new SystemResponse<int>(true, $"Error cleaning up expired tokens: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all active refresh tokens for a user
        /// </summary>
        /// <param name="userId">User ID to get tokens for</param>
        /// <returns>List of active refresh tokens</returns>
        public async Task<SystemResponse<RefreshToken[]>> GetActiveTokensForUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new SystemResponse<RefreshToken[]>(true, "User ID is required");
                }

                var filter = Builders<RefreshToken>.Filter.And(
                    Builders<RefreshToken>.Filter.Eq(rt => rt.UserId, userId),
                    Builders<RefreshToken>.Filter.Eq(rt => rt.IsUsed, false),
                    Builders<RefreshToken>.Filter.Eq(rt => rt.IsRevoked, false),
                    Builders<RefreshToken>.Filter.Gt(rt => rt.ExpiresAt, DateTime.UtcNow)
                );

                var tokens = await _refreshTokens.Find(filter).ToListAsync();

                return new SystemResponse<RefreshToken[]>(tokens.ToArray(), $"Found {tokens.Count} active tokens");
            }
            catch (Exception ex)
            {
                return new SystemResponse<RefreshToken[]>(true, $"Error retrieving active tokens: {ex.Message}");
            }
        }
    }
}
