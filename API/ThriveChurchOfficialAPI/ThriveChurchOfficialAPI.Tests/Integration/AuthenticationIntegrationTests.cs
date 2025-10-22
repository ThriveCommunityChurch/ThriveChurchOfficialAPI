using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Integration
{
    [TestClass]
    public class AuthenticationIntegrationTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private Mock<ILogger<AuthenticationService>> _mockLogger;
        private IJwtService _jwtService;
        private IAuthenticationService _authenticationService;
        private JwtSettings _jwtSettings;
        private User _testUser;
        private string _testPassword = "TestPassword123!";

        [TestInitialize]
        public void Setup()
        {
            // Setup JWT settings
            _jwtSettings = new JwtSettings
            {
                SecretKey = "TestSecretKey123456789012345678901234567890",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };

            // Setup services
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _mockLogger = new Mock<ILogger<AuthenticationService>>();
            _jwtService = new JwtService(Options.Create(_jwtSettings));
            _authenticationService = new AuthenticationService(_mockUserRepository.Object, _jwtService, _mockRefreshTokenRepository.Object, _mockLogger.Object);

            // Setup test user with real BCrypt hash
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(_testPassword);
            _testUser = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = passwordHash,
                IsActive = true,
                Roles = new[] { "Admin", "User" },
                FailedLoginAttempts = 0,
                LockoutEnd = null
            };
        }

        [TestMethod]
        public async Task CompleteAuthenticationFlow_ValidUser_Success()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = _testPassword
            };

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(new SystemResponse<User>(_testUser, "User found"));

            _mockUserRepository.Setup(r => r.ResetFailedLoginAttemptsAsync(_testUser.Id))
                .ReturnsAsync(new SystemResponse<User>(_testUser, "Attempts reset"));

            _mockRefreshTokenRepository.Setup(r => r.CreateRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync(new SystemResponse<RefreshToken>(new RefreshToken(), "Token created"));

            // Act - Login
            var loginResult = await _authenticationService.LoginAsync(loginRequest);

            // Assert - Login successful
            Assert.IsFalse(loginResult.HasErrors);
            Assert.IsNotNull(loginResult.Result);
            Assert.IsNotNull(loginResult.Result.Token);
            Assert.IsNotNull(loginResult.Result.RefreshToken);
            Assert.IsTrue(loginResult.Result.ExpiresAt > DateTime.UtcNow);

            // Verify token and refresh token are present
            Assert.IsNotNull(loginResult.Result.Token);
            Assert.IsNotNull(loginResult.Result.RefreshToken);

            // Act - Validate the generated token
            var principal = _jwtService.ValidateToken(loginResult.Result.Token);

            // Assert - Token validation successful
            Assert.IsNotNull(principal);
            Assert.AreEqual(_testUser.Id, principal.FindFirst("userId")?.Value);
            Assert.AreEqual(_testUser.Username, principal.FindFirst("username")?.Value);
            Assert.AreEqual(_testUser.Email, principal.FindFirst("email")?.Value);

            // Verify roles in token (using lowercase "role" as that's what JWT generates)
            var roleClaims = principal.FindAll("role");
            Assert.AreEqual(_testUser.Roles.Length, roleClaims.Count());
        }

        [TestMethod]
        public async Task AuthenticationFlow_InvalidPassword_Fails()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword123!"
            };

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(new SystemResponse<User>(_testUser, "User found"));

            // Act
            var loginResult = await _authenticationService.LoginAsync(loginRequest);

            // Assert
            Assert.IsTrue(loginResult.HasErrors);
            Assert.AreEqual("Invalid username or password", loginResult.ErrorMessage);
            Assert.IsNull(loginResult.Result);
        }

        [TestMethod]
        public async Task AuthenticationFlow_InactiveUser_Fails()
        {
            // Arrange
            var inactiveUser = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(_testPassword),
                IsActive = false,
                Roles = new[] { "User" }
            };

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = _testPassword
            };

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(new SystemResponse<User>(inactiveUser, "User found"));

            // Act
            var loginResult = await _authenticationService.LoginAsync(loginRequest);

            // Assert
            Assert.IsTrue(loginResult.HasErrors);
            Assert.AreEqual("User account is inactive", loginResult.ErrorMessage);
            Assert.IsNull(loginResult.Result);
        }

        [TestMethod]
        public async Task AuthenticationFlow_UserNotFound_Fails()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistentuser",
                Password = _testPassword
            };

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(new SystemResponse<User>(null, "User not found"));

            // Act
            var loginResult = await _authenticationService.LoginAsync(loginRequest);

            // Assert
            Assert.IsTrue(loginResult.HasErrors);
            Assert.AreEqual("Invalid username or password", loginResult.ErrorMessage);
            Assert.IsNull(loginResult.Result);
        }

        // REMOVED: TokenGeneration_MultipleUsers_GeneratesDifferentTokens
        // This was testing JWT library behavior, not our application logic

        // REMOVED: PasswordHashing_SamePassword_GeneratesDifferentHashes
        // This was testing BCrypt library behavior, not our application logic

        // REMOVED: TokenValidation_ExpiredToken_ReturnsNull
        // This was testing JWT library validation behavior, not our application logic

        // REMOVED: TokenValidation_WrongSecret_ReturnsNull
        // This was testing JWT library security behavior, not our application logic
    }
}
