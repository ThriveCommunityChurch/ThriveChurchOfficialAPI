using System;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// User repository interface for authentication operations
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Find a user by username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User if found, null if not found</returns>
        Task<SystemResponse<User>> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Find a user by ID
        /// </summary>
        /// <param name="userId">User ID to search for</param>
        /// <returns>User if found, null if not found</returns>
        Task<SystemResponse<User>> GetUserByIdAsync(string userId);

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user">User to create</param>
        /// <returns>Created user with ID</returns>
        Task<SystemResponse<User>> CreateUserAsync(User user);

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="user">User to update</param>
        /// <returns>Updated user</returns>
        Task<SystemResponse<User>> UpdateUserAsync(User user);

        /// <summary>
        /// Check if a username already exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if exists, false if available</returns>
        Task<SystemResponse<bool>> UsernameExistsAsync(string username);

        /// <summary>
        /// Check if an email already exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if exists, false if available</returns>
        Task<SystemResponse<bool>> EmailExistsAsync(string email);

        /// <summary>
        /// Record a failed login attempt for a user
        /// </summary>
        /// <param name="userId">User ID to record failed attempt for</param>
        /// <returns>Updated user with failed attempt count</returns>
        Task<SystemResponse<User>> RecordFailedLoginAttemptAsync(string userId);

        /// <summary>
        /// Reset failed login attempts for a user (called on successful login)
        /// </summary>
        /// <param name="userId">User ID to reset attempts for</param>
        /// <returns>Updated user with reset attempt count</returns>
        Task<SystemResponse<User>> ResetFailedLoginAttemptsAsync(string userId);

        /// <summary>
        /// Lock out a user account for a specified duration
        /// </summary>
        /// <param name="userId">User ID to lock out</param>
        /// <param name="lockoutDuration">Duration of lockout</param>
        /// <returns>Updated user with lockout information</returns>
        Task<SystemResponse<User>> LockoutUserAsync(string userId, TimeSpan lockoutDuration);

        /// <summary>
        /// Unlock a user account (remove lockout)
        /// </summary>
        /// <param name="userId">User ID to unlock</param>
        /// <returns>Updated user with lockout removed</returns>
        Task<SystemResponse<User>> UnlockUserAsync(string userId);
    }
}
