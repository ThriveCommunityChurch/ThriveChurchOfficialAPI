using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Refresh token repository interface for token management operations
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Create a new refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to create</param>
        /// <returns>Created refresh token with ID</returns>
        Task<SystemResponse<RefreshToken>> CreateRefreshTokenAsync(RefreshToken refreshToken);

        /// <summary>
        /// Find a refresh token by token value
        /// </summary>
        /// <param name="token">Token value to search for</param>
        /// <returns>Refresh token if found and valid, null if not found or invalid</returns>
        Task<SystemResponse<RefreshToken>> GetRefreshTokenAsync(string token);

        /// <summary>
        /// Mark a refresh token as used
        /// </summary>
        /// <param name="tokenId">Token ID to mark as used</param>
        /// <param name="usedByIp">IP address where token was used</param>
        /// <returns>Updated refresh token</returns>
        Task<SystemResponse<RefreshToken>> MarkTokenAsUsedAsync(string tokenId, string usedByIp = null);

        /// <summary>
        /// Revoke a refresh token
        /// </summary>
        /// <param name="tokenId">Token ID to revoke</param>
        /// <param name="revokedByIp">IP address where token was revoked</param>
        /// <returns>Updated refresh token</returns>
        Task<SystemResponse<RefreshToken>> RevokeTokenAsync(string tokenId, string revokedByIp = null);

        /// <summary>
        /// Revoke all refresh tokens for a user
        /// </summary>
        /// <param name="userId">User ID whose tokens should be revoked</param>
        /// <param name="revokedByIp">IP address where tokens were revoked</param>
        /// <returns>Number of tokens revoked</returns>
        Task<SystemResponse<int>> RevokeAllUserTokensAsync(string userId, string revokedByIp = null);

        /// <summary>
        /// Clean up expired refresh tokens
        /// </summary>
        /// <returns>Number of tokens cleaned up</returns>
        Task<SystemResponse<int>> CleanupExpiredTokensAsync();

        /// <summary>
        /// Get all active refresh tokens for a user
        /// </summary>
        /// <param name="userId">User ID to get tokens for</param>
        /// <returns>List of active refresh tokens</returns>
        Task<SystemResponse<RefreshToken[]>> GetActiveTokensForUserAsync(string userId);
    }
}
