using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Core.Constants;
using ThriveChurchOfficialAPI.Core.Utilities;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Authentication service for user login and credential validation
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<AuthenticationService> _logger;

        /// <summary>
        /// Authentication Service Constructor
        /// </summary>
        /// <param name="userRepository">User repository for database operations</param>
        /// <param name="jwtService">JWT service for token generation</param>
        /// <param name="refreshTokenRepository">Refresh token repository for token management</param>
        /// <param name="logger">Logger for internal error logging</param>
        public AuthenticationService(IUserRepository userRepository, 
            IJwtService jwtService, 
            IRefreshTokenRepository refreshTokenRepository, 
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate a user with username/email and password
        /// </summary>
        /// <param name="request">Login request with credentials</param>
        /// <returns>Login response with JWT token if successful</returns>
        public async Task<SystemResponse<LoginResponse>> LoginAsync(HttpContext webRequest, LoginRequest request)
        {
            try
            {
                // Validate input
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    // Log internal details but return generic message
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.InvalidRequest);
                }

                // Find user by username or email
                var userResponse = await _userRepository.GetUserByUsernameAsync(request.Username);
                if (userResponse.HasErrors)
                {
                    // Log internal details but return generic message
                    _logger.LogError("Database error during login for username: {Username} - {Error}", request.Username, userResponse.ErrorMessage);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.LoginFailed);
                }

                var user = userResponse.Result;
                if (user == null)
                {
                    // Log internal details but return generic message
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.LoginFailed);
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    // Return generic error message to prevent user enumeration
                    _logger.LogError("Login attempt for inactive user: {UserId}", user.Id);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.LoginFailed);
                }

                // Check if user is locked out
                if (user.IsLockedOut)
                {
                    // Return generic error message to prevent user enumeration
                    _logger.LogError("Login attempt for locked out user: {UserId}, lockout ends: {LockoutEnd} UTC", user.Id, user.LockoutEnd);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.LoginFailed);
                }

                // Validate password
                if (!ValidatePassword(user, request.Password))
                {
                    // Log internal details and record failed attempt
                    _logger.LogError("Invalid password attempt for user: {UserId}", user.Id);
                    await RecordFailedLoginAttemptAsync(user.Id);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.LoginFailed);
                }

                // Reset failed login attempts on successful login
                await _userRepository.ResetFailedLoginAttemptsAsync(user.Id);

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);
                var refreshTokenValue = _jwtService.GenerateRefreshToken();
                var expiresAt = _jwtService.GetTokenExpiration();

                // Create refresh token entity and store in database
                var refreshToken = new RefreshToken
                {
                    Token = refreshTokenValue,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days for refresh token
                    CreatedByIp = GetClientIpAddress(webRequest)
                };

                var refreshTokenResult = await _refreshTokenRepository.CreateRefreshTokenAsync(refreshToken);
                if (refreshTokenResult.HasErrors)
                {
                    _logger.LogError($"Error creating refresh token: {refreshTokenResult.ErrorMessage}");
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.LoginFailed);
                }

                var loginResponse = new LoginResponse
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    RefreshToken = refreshTokenValue
                };

                return new SystemResponse<LoginResponse>(loginResponse, AuthenticationMessages.LoginSuccessful);
            }
            catch (Exception ex)
            {
                // Log internal details but return generic message
                _logger.LogError(ex, "Unexpected error during login");
                return new SystemResponse<LoginResponse>(true, AuthenticationMessages.LoginFailed);
            }
        }

        /// <summary>
        /// Refresh a JWT token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New login response with fresh JWT token</returns>
        public async Task<SystemResponse<LoginResponse>> RefreshTokenAsync(HttpContext webRequest, RefreshTokenRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.InvalidRequest);
                }

                // Get refresh token from database
                var refreshTokenResponse = await _refreshTokenRepository.GetRefreshTokenAsync(request.RefreshToken);
                if (refreshTokenResponse.HasErrors)
                {
                    // Log internal details but return generic message
                    _logger.LogError("Refresh token lookup failed: {Error}", refreshTokenResponse.ErrorMessage);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                }

                var refreshToken = refreshTokenResponse.Result;

                // Validate refresh token
                if (!refreshToken.IsValid)
                {
                    if (refreshToken.IsExpired)
                    {
                        _logger.LogError("Expired refresh token used: {TokenId}", refreshToken.Id);
                        return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                    }
                    if (refreshToken.IsUsed)
                    {
                        _logger.LogError("Already used refresh token attempted: {TokenId}", refreshToken.Id);
                        return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                    }
                    if (refreshToken.IsRevoked)
                    {
                        _logger.LogError("Revoked refresh token attempted: {TokenId}", refreshToken.Id);
                        return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                    }
                    _logger.LogError("Invalid refresh token attempted: {TokenId}", refreshToken.Id);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                }

                // Get user associated with refresh token
                var userResponse = await _userRepository.GetUserByIdAsync(refreshToken.UserId);
                if (userResponse.HasErrors || userResponse.Result == null)
                {
                    _logger.LogError("User lookup failed for refresh token: {TokenId}, UserId: {UserId}", refreshToken.Id, refreshToken.UserId);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                }

                var user = userResponse.Result;

                // Check if user is still active
                if (!user.IsActive)
                {
                    _logger.LogError("Refresh token used for inactive user: {UserId}", user.Id);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                }

                // Check if user is locked out
                if (user.IsLockedOut)
                {
                    _logger.LogError("Refresh token used for locked out user: {UserId}, lockout ends: {LockoutEnd}", user.Id, user.LockoutEnd);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                }

                // Mark the old refresh token as used
                await _refreshTokenRepository.MarkTokenAsUsedAsync(refreshToken.Id);

                // Generate new JWT token and refresh token
                var newJwtToken = _jwtService.GenerateToken(user);
                var newRefreshTokenValue = _jwtService.GenerateRefreshToken();
                var expiresAt = _jwtService.GetTokenExpiration();

                // Create new refresh token entity and store in database
                var newRefreshToken = new RefreshToken
                {
                    Token = newRefreshTokenValue,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days for refresh token
                    CreatedByIp = GetClientIpAddress(webRequest)
                };

                var newRefreshTokenResult = await _refreshTokenRepository.CreateRefreshTokenAsync(newRefreshToken);
                if (newRefreshTokenResult.HasErrors)
                {
                    _logger.LogError("Error creating new refresh token: {Error}", newRefreshTokenResult.ErrorMessage);
                    return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
                }

                var loginResponse = new LoginResponse
                {
                    Token = newJwtToken,
                    ExpiresAt = expiresAt,
                    RefreshToken = newRefreshTokenValue
                };

                return new SystemResponse<LoginResponse>(loginResponse, AuthenticationMessages.TokenRefreshSuccessful);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return new SystemResponse<LoginResponse>(true, AuthenticationMessages.RefreshTokenFailed);
            }
        }

        /// <summary>
        /// Validate user credentials against stored password hash
        /// </summary>
        /// <param name="user">User from database</param>
        /// <param name="password">Plain text password to verify</param>
        /// <returns>True if password is valid, false otherwise</returns>
        public bool ValidatePassword(User user, string password)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash) || string.IsNullOrWhiteSpace(password))
                {
                    return false;
                }

                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Hash a password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password</returns>
        /// <exception cref="ArgumentException">Thrown when password doesn't meet complexity requirements</exception>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            // Validate password complexity
            var validationError = PasswordValidator.ValidatePasswordWithMessage(password);
            if (validationError != null)
            {
                throw new ArgumentException(validationError, nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// Validate password complexity requirements
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Validation result with error message if invalid</returns>
        public SystemResponse<string> ValidatePasswordComplexity(string password)
        {
            var validationError = PasswordValidator.ValidatePasswordWithMessage(password);
            if (validationError != null)
            {
                return new SystemResponse<string>(true, validationError);
            }

            return new SystemResponse<string>("Password meets complexity requirements", "Password is valid");
        }

        /// <summary>
        /// Record a failed login attempt and lock account if threshold is reached
        /// </summary>
        /// <param name="userId">User ID to record failed attempt for</param>
        /// <returns>Task</returns>
        private async Task RecordFailedLoginAttemptAsync(string userId)
        {
            try
            {
                // Record the failed attempt
                var result = await _userRepository.RecordFailedLoginAttemptAsync(userId);

                if (!result.HasErrors && result.Result != null)
                {
                    var user = result.Result;

                    // Check if we've reached the lockout threshold (5 failed attempts)
                    if (user.FailedLoginAttempts >= 5)
                    {
                        // Lock the account for 30 minutes
                        var lockoutDuration = TimeSpan.FromMinutes(30);
                        await _userRepository.LockoutUserAsync(userId, lockoutDuration);
                        _logger.LogWarning("User account locked due to 5 failed login attempts: {UserId}", userId);
                    }
                }
                else
                {
                    _logger.LogError("Failed to record login attempt for user: {UserId} - {Error}", userId, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the login process
                _logger.LogError(ex, "Error recording failed login attempt for user: {UserId}", userId);
            }
        }

        /// <summary>
        /// Extract client IP address from HTTP context
        /// </summary>
        /// <param name="httpContext">HTTP context containing connection information</param>
        /// <returns>Client IP address as string, or null if not available</returns>
        private string GetClientIpAddress(HttpContext httpContext)
        {
            try
            {
                if (httpContext?.Connection?.RemoteIpAddress != null)
                {
                    return httpContext.Connection.RemoteIpAddress.ToString();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract client IP address from HTTP context");
                return null;
            }
        }

        /// <summary>
        /// Unlock a user account (admin function)
        /// </summary>
        /// <param name="userId">User ID to unlock</param>
        /// <returns>Success response</returns>
        public async Task<SystemResponse<string>> UnlockUserAccountAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogError("Unlock attempt with missing user ID");
                    return new SystemResponse<string>(true, AuthenticationMessages.InvalidRequest);
                }

                var result = await _userRepository.UnlockUserAsync(userId);
                if (result.HasErrors)
                {
                    _logger.LogError("Failed to unlock user {UserId}: {Error}", userId, result.ErrorMessage);
                    return new SystemResponse<string>(true, AuthenticationMessages.UnlockFailed);
                }

                return new SystemResponse<string>(AuthenticationMessages.AccountUnlocked, "User account has been unlocked");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error unlocking user account: {UserId}", userId);
                return new SystemResponse<string>(true, AuthenticationMessages.UnlockFailed);
            }
        }
    }
}
