using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// User repository for MongoDB operations
    /// </summary>
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        private const string COLLECTION_NAME = "Users";

        /// <summary>
        /// User Repository Constructor
        /// </summary>
        /// <param name="configuration">Configuration for database connection</param>
        public UserRepository(IConfiguration configuration) : base(configuration)
        {
            _users = DB.GetCollection<User>(COLLECTION_NAME);
            
            // Create indexes for performance
            CreateIndexes();
        }

        /// <summary>
        /// Create database indexes for user collection
        /// </summary>
        private void CreateIndexes()
        {
            try
            {
                // Create unique index on username
                var usernameIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Username);
                var usernameIndexOptions = new CreateIndexOptions { Unique = true };
                var usernameIndexModel = new CreateIndexModel<User>(usernameIndexKeys, usernameIndexOptions);

                // Create unique index on email
                var emailIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
                var emailIndexOptions = new CreateIndexOptions { Unique = true };
                var emailIndexModel = new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions);

                _users.Indexes.CreateMany(new[] { usernameIndexModel, emailIndexModel });
            }
            catch (Exception)
            {
                // Indexes might already exist, ignore errors
            }
        }

        /// <summary>
        /// Find a user by username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User if found, null if not found</returns>
        public async Task<SystemResponse<User>> GetUserByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return new SystemResponse<User>(true, "Username is required");
                }

                var filter = Builders<User>.Filter.Eq(u => u.Username, username);

                var user = await _users.Find(filter).FirstOrDefaultAsync();

                return new SystemResponse<User>(user, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                return new SystemResponse<User>(true, $"Error finding user: {ex.Message}");
            }
        }

        /// <summary>
        /// Find a user by ID
        /// </summary>
        /// <param name="userId">User ID to search for</param>
        /// <returns>User if found, null if not found</returns>
        public async Task<SystemResponse<User>> GetUserByIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || !IsValidObjectId(userId))
                {
                    return new SystemResponse<User>(true, "Valid user ID is required");
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var user = await _users.Find(filter).FirstOrDefaultAsync();

                return new SystemResponse<User>(user, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                return new SystemResponse<User>(true, $"Error finding user by ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user">User to create</param>
        /// <returns>Created user with ID</returns>
        public async Task<SystemResponse<User>> CreateUserAsync(User user)
        {
            try
            {
                if (user == null)
                {
                    return new SystemResponse<User>(true, "User is required");
                }

                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;

                await _users.InsertOneAsync(user);

                return new SystemResponse<User>(user, "User created successfully");
            }
            catch (Exception ex)
            {
                return new SystemResponse<User>(true, $"Error creating user: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="user">User to update</param>
        /// <returns>Updated user</returns>
        public async Task<SystemResponse<User>> UpdateUserAsync(User user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Id))
                {
                    return new SystemResponse<User>(true, "User with valid ID is required");
                }

                user.UpdatedAt = DateTime.UtcNow;

                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var result = await _users.ReplaceOneAsync(filter, user);

                if (result.ModifiedCount == 0)
                {
                    return new SystemResponse<User>(true, "User not found or not updated");
                }

                return new SystemResponse<User>(user, "User updated successfully");
            }
            catch (Exception ex)
            {
                return new SystemResponse<User>(true, $"Error updating user: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a username already exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if exists, false if available</returns>
        public async Task<SystemResponse<bool>> UsernameExistsAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return new SystemResponse<bool>(false, "Username check completed");
                }

                var filter = Builders<User>.Filter.Eq(u => u.Username, username);
                var count = await _users.CountDocumentsAsync(filter);

                return new SystemResponse<bool>(count > 0, "Username check completed");
            }
            catch (Exception ex)
            {
                return new SystemResponse<bool>(true, $"Error checking username: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if an email already exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if exists, false if available</returns>
        public async Task<SystemResponse<bool>> EmailExistsAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new SystemResponse<bool>(false, "Email check completed");
                }

                var filter = Builders<User>.Filter.Eq(u => u.Email, email);
                var count = await _users.CountDocumentsAsync(filter);

                return new SystemResponse<bool>(count > 0, "Email check completed");
            }
            catch (Exception ex)
            {
                return new SystemResponse<bool>(true, $"Error checking email: {ex.Message}");
            }
        }

        /// <summary>
        /// Record a failed login attempt for a user
        /// </summary>
        /// <param name="userId">User ID to record failed attempt for</param>
        /// <returns>Updated user with failed attempt count</returns>
        public async Task<SystemResponse<User>> RecordFailedLoginAttemptAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || !IsValidObjectId(userId))
                {
                    return new SystemResponse<User>(true, "Valid user ID is required");
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var update = Builders<User>.Update
                    .Inc(u => u.FailedLoginAttempts, 1)
                    .Set(u => u.LastFailedLoginAttempt, DateTime.UtcNow)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _users.FindOneAndUpdateAsync(filter, update,
                    new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After });

                if (result == null)
                {
                    return new SystemResponse<User>(true, "User not found");
                }

                return new SystemResponse<User>(result, "Failed login attempt recorded");
            }
            catch (Exception ex)
            {
                return new SystemResponse<User>(true, $"Error recording failed login attempt: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset failed login attempts for a user (called on successful login)
        /// </summary>
        /// <param name="userId">User ID to reset attempts for</param>
        /// <returns>Updated user with reset attempt count</returns>
        public async Task<SystemResponse<User>> ResetFailedLoginAttemptsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || !IsValidObjectId(userId))
                {
                    return new SystemResponse<User>(true, "Valid user ID is required");
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var update = Builders<User>.Update
                    .Set(u => u.FailedLoginAttempts, 0)
                    .Unset(u => u.LastFailedLoginAttempt)
                    .Unset(u => u.LockoutEnd)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _users.FindOneAndUpdateAsync(filter, update,
                    new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After });

                if (result == null)
                {
                    return new SystemResponse<User>(true, "User not found");
                }

                return new SystemResponse<User>(result, "Failed login attempts reset");
            }
            catch (Exception ex)
            {
                return new SystemResponse<User>(true, $"Error resetting failed login attempts: {ex.Message}");
            }
        }

        /// <summary>
        /// Lock out a user account for a specified duration
        /// </summary>
        /// <param name="userId">User ID to lock out</param>
        /// <param name="lockoutDuration">Duration of lockout</param>
        /// <returns>Updated user with lockout information</returns>
        public async Task<SystemResponse<User>> LockoutUserAsync(string userId, TimeSpan lockoutDuration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || !IsValidObjectId(userId))
                {
                    return new SystemResponse<User>(true, "Valid user ID is required");
                }

                var lockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var update = Builders<User>.Update
                    .Set(u => u.LockoutEnd, lockoutEnd)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _users.FindOneAndUpdateAsync(filter, update,
                    new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After });

                if (result == null)
                {
                    return new SystemResponse<User>(true, "User not found");
                }

                return new SystemResponse<User>(result, $"User locked out until {lockoutEnd:yyyy-MM-dd HH:mm:ss} UTC");
            }
            catch (Exception ex)
            {
                return new SystemResponse<User>(true, $"Error locking out user: {ex.Message}");
            }
        }

        /// <summary>
        /// Unlock a user account (remove lockout)
        /// </summary>
        /// <param name="userId">User ID to unlock</param>
        /// <returns>Updated user with lockout removed</returns>
        public async Task<SystemResponse<User>> UnlockUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || !IsValidObjectId(userId))
                {
                    return new SystemResponse<User>(true, "Valid user ID is required");
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var update = Builders<User>.Update
                    .Set(u => u.FailedLoginAttempts, 0)
                    .Unset(u => u.LockoutEnd)
                    .Unset(u => u.LastFailedLoginAttempt)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _users.FindOneAndUpdateAsync(filter, update,
                    new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After });

                if (result == null)
                {
                    return new SystemResponse<User>(true, "User not found");
                }

                return new SystemResponse<User>(result, "User account unlocked");
            }
            catch (Exception ex)
            {
                return new SystemResponse<User>(true, $"Error unlocking user: {ex.Message}");
            }
        }
    }
}
