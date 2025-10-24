using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    /// <summary>
    /// Tests for JwtService - focusing on OUR application logic, not external JWT library behavior
    /// </summary>
    [TestClass]
    public class JwtServiceTests
    {
        private JwtService _jwtService;
        private JwtSettings _jwtSettings;
        private User _testUser;

        [TestInitialize]
        public void Setup()
        {
            // Arrange - Setup test JWT settings
            _jwtSettings = new JwtSettings
            {
                SecretKey = "TestSecretKey123456789012345678901234567890", // Must be at least 32 characters
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };

            var options = Options.Create(_jwtSettings);
            _jwtService = new JwtService(options);

            // Setup test user
            _testUser = new User
            {
                Id = "507f1f77bcf86cd799439011",
                Username = "testuser",
                Email = "test@example.com",
                Roles = new[] { "Admin", "User" }
            };
        }

        #region Constructor Tests - Testing OUR validation logic

        [TestMethod]
        public void Constructor_ValidSettings_CreatesService()
        {
            // Arrange
            var validSettings = new JwtSettings
            {
                SecretKey = "ValidSecretKey123456789012345678901234567890",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };

            // Act & Assert - Test OUR constructor logic
            var service = new JwtService(Options.Create(validSettings));
            Assert.IsNotNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert - Test OUR null validation logic
            new JwtService(null);
        }

        #endregion

        #region GenerateToken Tests - Testing OUR token generation logic

        [TestMethod]
        public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
        {
            // Act
            var token = _jwtService.GenerateToken(_testUser);

            // Assert - Test OUR logic: that we return a non-empty token
            Assert.IsNotNull(token);
            Assert.IsTrue(token.Length > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateToken_NullUser_ThrowsArgumentNullException()
        {
            // Act & Assert - Test OUR error handling logic
            _jwtService.GenerateToken(null);
        }

        [TestMethod]
        public void GenerateToken_UserWithNullRoles_DoesNotThrow()
        {
            // Arrange
            var userWithNullRoles = new User
            {
                Id = "507f1f77bcf86cd799439012",
                Username = "noroleuser",
                Email = "norole@example.com",
                Roles = null
            };

            // Act & Assert - Test OUR null handling logic
            var token = _jwtService.GenerateToken(userWithNullRoles);
            Assert.IsNotNull(token);
            Assert.IsTrue(token.Length > 0);
        }

        #endregion

        #region GenerateRefreshToken Tests - Testing OUR refresh token logic

        [TestMethod]
        public void GenerateRefreshToken_ReturnsNonEmptyString()
        {
            // Act
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Assert - Test OUR logic: that we return a non-empty string
            Assert.IsNotNull(refreshToken);
            Assert.IsTrue(refreshToken.Length > 0);
        }

        #endregion

        #region GetTokenExpiration Tests - Testing OUR calculation logic

        [TestMethod]
        public void GetTokenExpiration_ReturnsCorrectExpirationTime()
        {
            // Arrange
            var beforeCall = DateTime.UtcNow;

            // Act
            var expiration = _jwtService.GetTokenExpiration();

            // Assert - Test OUR calculation logic
            var afterCall = DateTime.UtcNow;
            var expectedMin = beforeCall.AddMinutes(_jwtSettings.ExpirationMinutes);
            var expectedMax = afterCall.AddMinutes(_jwtSettings.ExpirationMinutes);

            Assert.IsTrue(expiration >= expectedMin && expiration <= expectedMax);
        }

        #endregion

        #region ValidateToken Tests - Testing OUR validation logic

        [TestMethod]
        public void ValidateToken_ValidToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            var token = _jwtService.GenerateToken(_testUser);

            // Act
            var principal = _jwtService.ValidateToken(token);

            // Assert - Test OUR logic: that we return a valid principal
            Assert.IsNotNull(principal);
        }

        [TestMethod]
        public void ValidateToken_InvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var principal = _jwtService.ValidateToken(invalidToken);

            // Assert - Test OUR error handling logic
            Assert.IsNull(principal);
        }

        [TestMethod]
        public void ValidateToken_NullToken_ReturnsNull()
        {
            // Act
            var principal = _jwtService.ValidateToken(null);

            // Assert - Test OUR null handling logic
            Assert.IsNull(principal);
        }

        [TestMethod]
        public void ValidateToken_EmptyToken_ReturnsNull()
        {
            // Act
            var principal = _jwtService.ValidateToken("");

            // Assert - Test OUR empty string handling logic
            Assert.IsNull(principal);
        }

        #endregion

        #region GetUserIdFromToken Tests - Testing OUR extraction logic

        [TestMethod]
        public void GetUserIdFromToken_ValidToken_ReturnsUserId()
        {
            // Arrange
            var token = _jwtService.GenerateToken(_testUser);

            // Act
            var userId = _jwtService.GetUserIdFromToken(token);

            // Assert - Test OUR extraction logic
            Assert.AreEqual(_testUser.Id, userId);
        }

        [TestMethod]
        public void GetUserIdFromToken_InvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var userId = _jwtService.GetUserIdFromToken(invalidToken);

            // Assert - Test OUR error handling logic
            Assert.IsNull(userId);
        }

        [TestMethod]
        public void GetUserIdFromToken_NullToken_ReturnsNull()
        {
            // Act
            var userId = _jwtService.GetUserIdFromToken(null);

            // Assert - Test OUR null handling logic
            Assert.IsNull(userId);
        }

        #endregion
    }
}
