using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IJwtService> _mockJwtService;
        private Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private Mock<ILogger<AuthenticationService>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<ConnectionInfo> _mockConnectionInfo;
        private AuthenticationService _authenticationService;
        private User _testUser;
        private string _testPassword = "TestPassword123!";
        private string _testPasswordHash;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _mockLogger = new Mock<ILogger<AuthenticationService>>();

            // Setup mock HttpContext with fake IP address
            _mockHttpContext = new Mock<HttpContext>();
            _mockConnectionInfo = new Mock<ConnectionInfo>();
            _mockConnectionInfo.Setup(c => c.RemoteIpAddress).Returns(IPAddress.Parse("192.168.1.100"));
            _mockHttpContext.Setup(c => c.Connection).Returns(_mockConnectionInfo.Object);

            _authenticationService = new AuthenticationService(_mockUserRepository.Object, _mockJwtService.Object, _mockRefreshTokenRepository.Object, _mockLogger.Object);

            // Create test password hash
            _testPasswordHash = BCrypt.Net.BCrypt.HashPassword(_testPassword);

            _testUser = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = _testPasswordHash,
                IsActive = true,
                Roles = new[] { "Admin" },
                FailedLoginAttempts = 0,
                LockoutEnd = null
            };
        }

        #region LoginAsync Tests

        [TestMethod]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = _testPassword
            };

            var expectedToken = "test.jwt.token";
            var expectedRefreshToken = "refresh-token";
            var expectedExpiration = DateTime.UtcNow.AddHours(1);

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(new SystemResponse<User>(_testUser, "User found"));

            _mockUserRepository.Setup(r => r.ResetFailedLoginAttemptsAsync(_testUser.Id))
                .ReturnsAsync(new SystemResponse<User>(_testUser, "Attempts reset"));

            _mockRefreshTokenRepository.Setup(r => r.CreateRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync(new SystemResponse<RefreshToken>(new RefreshToken(), "Token created"));

            _mockJwtService.Setup(j => j.GenerateToken(_testUser))
                .Returns(expectedToken);

            _mockJwtService.Setup(j => j.GenerateRefreshToken())
                .Returns(expectedRefreshToken);

            _mockJwtService.Setup(j => j.GetTokenExpiration())
                .Returns(expectedExpiration);

            // Act
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, loginRequest);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(expectedToken, result.Result.Token);
            Assert.AreEqual(expectedRefreshToken, result.Result.RefreshToken);
            Assert.AreEqual(expectedExpiration, result.Result.ExpiresAt);
        }

        [TestMethod]
        public async Task LoginAsync_NullRequest_ReturnsErrorResponse()
        {
            // Act
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Username and password are required", result.ErrorMessage);
        }

        [TestMethod]
        public async Task LoginAsync_EmptyUsername_ReturnsErrorResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "",
                Password = _testPassword
            };

            // Act
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, loginRequest);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Username and password are required", result.ErrorMessage);
        }

        [TestMethod]
        public async Task LoginAsync_EmptyPassword_ReturnsErrorResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = ""
            };

            // Act
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, loginRequest);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Username and password are required", result.ErrorMessage);
        }

        [TestMethod]
        public async Task LoginAsync_UserNotFound_ReturnsErrorResponse()
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
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, loginRequest);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Invalid username or password", result.ErrorMessage);
        }

        [TestMethod]
        public async Task LoginAsync_RepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = _testPassword
            };

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(new SystemResponse<User>(true, "Database error"));

            // Act
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, loginRequest);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Authentication failed", result.ErrorMessage);
        }

        [TestMethod]
        public async Task LoginAsync_InactiveUser_ReturnsErrorResponse()
        {
            // Arrange
            var inactiveUser = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = _testPasswordHash,
                IsActive = false
            };

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = _testPassword
            };

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(new SystemResponse<User>(inactiveUser, "User found"));

            // Act
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, loginRequest);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("User account is inactive", result.ErrorMessage);
        }

        [TestMethod]
        public async Task LoginAsync_InvalidPassword_ReturnsErrorResponse()
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
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, loginRequest);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Invalid username or password", result.ErrorMessage);
        }

        #endregion

        #region RefreshTokenAsync Tests

        [TestMethod]
        public async Task RefreshTokenAsync_NullRequest_ReturnsErrorResponse()
        {
            // Act
            var result = await _authenticationService.RefreshTokenAsync(_mockHttpContext.Object, null);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Refresh token is required", result.ErrorMessage);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_EmptyRefreshToken_ReturnsErrorResponse()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "" };

            // Act
            var result = await _authenticationService.RefreshTokenAsync(_mockHttpContext.Object, request);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Refresh token is required", result.ErrorMessage);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "valid-refresh-token" };
            var refreshToken = new RefreshToken
            {
                Id = "507f1f77bcf86cd799439013",
                Token = "valid-refresh-token",
                UserId = _testUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsUsed = false,
                IsRevoked = false
            };

            _mockRefreshTokenRepository.Setup(r => r.GetRefreshTokenAsync(request.RefreshToken))
                .ReturnsAsync(new SystemResponse<RefreshToken>(refreshToken, "Token found"));

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(_testUser.Id))
                .ReturnsAsync(new SystemResponse<User>(_testUser, "User found"));

            _mockRefreshTokenRepository.Setup(r => r.MarkTokenAsUsedAsync(refreshToken.Id, null))
                .ReturnsAsync(new SystemResponse<RefreshToken>(refreshToken, "Token marked as used"));

            _mockRefreshTokenRepository.Setup(r => r.CreateRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync(new SystemResponse<RefreshToken>(new RefreshToken(), "Token created"));

            _mockJwtService.Setup(j => j.GenerateToken(_testUser))
                .Returns("new.jwt.token");

            _mockJwtService.Setup(j => j.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _mockJwtService.Setup(j => j.GetTokenExpiration())
                .Returns(DateTime.UtcNow.AddHours(1));

            // Act
            var result = await _authenticationService.RefreshTokenAsync(_mockHttpContext.Object, request);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("new.jwt.token", result.Result.Token);
            Assert.AreEqual("new-refresh-token", result.Result.RefreshToken);
        }

        #endregion

        #region ValidatePassword Tests

        [TestMethod]
        public void ValidatePassword_ValidPassword_ReturnsTrue()
        {
            // Act
            var result = _authenticationService.ValidatePassword(_testUser, _testPassword);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidatePassword_InvalidPassword_ReturnsFalse()
        {
            // Act
            var result = _authenticationService.ValidatePassword(_testUser, "WrongPassword");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_NullUser_ReturnsFalse()
        {
            // Act
            var result = _authenticationService.ValidatePassword(null, _testPassword);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_NullPassword_ReturnsFalse()
        {
            // Act
            var result = _authenticationService.ValidatePassword(_testUser, null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_EmptyPassword_ReturnsFalse()
        {
            // Act
            var result = _authenticationService.ValidatePassword(_testUser, "");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_UserWithNullPasswordHash_ReturnsFalse()
        {
            // Arrange
            var userWithNullHash = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = null,
                IsActive = true
            };

            // Act
            var result = _authenticationService.ValidatePassword(userWithNullHash, _testPassword);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region HashPassword Tests

        [TestMethod]
        public void HashPassword_ValidPassword_ReturnsHashedPassword()
        {
            // Act
            var hashedPassword = _authenticationService.HashPassword(_testPassword);

            // Assert
            Assert.IsNotNull(hashedPassword);
            Assert.IsTrue(hashedPassword.Length > 0);
            Assert.AreNotEqual(_testPassword, hashedPassword);
            Assert.IsTrue(BCrypt.Net.BCrypt.Verify(_testPassword, hashedPassword));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HashPassword_NullPassword_ThrowsArgumentException()
        {
            // Act & Assert
            _authenticationService.HashPassword(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HashPassword_EmptyPassword_ThrowsArgumentException()
        {
            // Act & Assert
            _authenticationService.HashPassword("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HashPassword_TooShort_ThrowsArgumentException()
        {
            // Act & Assert
            _authenticationService.HashPassword("short");
        }

        // REMOVED: HashPassword_SamePasswordTwice_ReturnsDifferentHashes
        // This was testing BCrypt's behavior, not our application logic

        #endregion

        #region Account Lockout Tests

        [TestMethod]
        public async Task LoginAsync_LockedOutUser_ReturnsLockoutError()
        {
            // Arrange
            var lockedUser = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Username = "lockeduser",
                Email = "locked@example.com",
                PasswordHash = _testPasswordHash,
                IsActive = true,
                FailedLoginAttempts = 5,
                LockoutEnd = DateTime.UtcNow.AddMinutes(30)
            };

            var request = new LoginRequest
            {
                Username = "lockeduser",
                Password = _testPassword
            };

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(request.Username))
                .ReturnsAsync(new SystemResponse<User>(lockedUser, "User found"));

            // Act
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, request);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("locked out"));
        }

        [TestMethod]
        public async Task LoginAsync_InvalidPassword_RecordsFailedAttempt()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(request.Username))
                .ReturnsAsync(new SystemResponse<User>(_testUser, "User found"));

            _mockUserRepository.Setup(r => r.RecordFailedLoginAttemptAsync(_testUser.Id))
                .ReturnsAsync(new SystemResponse<User>(_testUser, "Failed attempt recorded"));

            // Act
            var result = await _authenticationService.LoginAsync(_mockHttpContext.Object, request);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("Invalid username or password", result.ErrorMessage);
            _mockUserRepository.Verify(r => r.RecordFailedLoginAttemptAsync(_testUser.Id), Times.Once);
        }

        [TestMethod]
        public async Task UnlockUserAccountAsync_ValidUserId_ReturnsSuccess()
        {
            // Arrange
            var userId = "507f1f77bcf86cd799439011";
            var unlockedUser = new User { Id = userId, FailedLoginAttempts = 0, LockoutEnd = null };

            _mockUserRepository.Setup(r => r.UnlockUserAsync(userId))
                .ReturnsAsync(new SystemResponse<User>(unlockedUser, "User unlocked"));

            // Act
            var result = await _authenticationService.UnlockUserAccountAsync(userId);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("Account unlocked successfully", result.Result);
        }

        [TestMethod]
        public async Task UnlockUserAccountAsync_EmptyUserId_ReturnsError()
        {
            // Act
            var result = await _authenticationService.UnlockUserAccountAsync("");

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual("User ID is required", result.ErrorMessage);
        }

        #endregion

        #region Password Validation Tests

        [TestMethod]
        public void ValidatePasswordComplexity_ValidPassword_ReturnsSuccess()
        {
            // Arrange
            var password = "ValidPassword123";

            // Act
            var result = _authenticationService.ValidatePasswordComplexity(password);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("Password meets complexity requirements", result.Result);
        }

        [TestMethod]
        public void ValidatePasswordComplexity_TooShort_ReturnsError()
        {
            // Arrange
            var password = "short";

            // Act
            var result = _authenticationService.ValidatePasswordComplexity(password);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.ErrorMessage.Contains("10 characters"));
        }



        #endregion
    }
}
