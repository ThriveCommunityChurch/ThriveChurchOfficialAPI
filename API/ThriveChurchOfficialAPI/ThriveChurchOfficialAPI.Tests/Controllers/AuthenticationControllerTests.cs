using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Controllers;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Controllers
{
    /// <summary>
    /// Minimal controller tests - only testing HTTP mapping, not business logic
    /// Business logic is thoroughly tested in service tests
    /// </summary>
    [TestClass]
    public class AuthenticationControllerTests
    {
        private Mock<IAuthenticationService> _mockAuthenticationService;
        private AuthenticationController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _controller = new AuthenticationController(_mockAuthenticationService.Object);
        }

        #region Login Tests - Basic HTTP Mapping

        [TestMethod]
        public async Task Login_ServiceReturnsSuccess_ReturnsOk()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "password" };
            var loginResponse = new LoginResponse { Token = "test.token" };

            _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                .ReturnsAsync(new SystemResponse<LoginResponse>(loginResponse, "Success"));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert - Test HTTP mapping: Service success → 200 OK
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Login_ServiceReturnsAuthError_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "password" };

            _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                .ReturnsAsync(new SystemResponse<LoginResponse>(true, "Invalid username or password"));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert - Test HTTP mapping: Auth error → 401 Unauthorized
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Login_ServiceReturnsValidationError_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "password" };

            _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                .ReturnsAsync(new SystemResponse<LoginResponse>(true, "Username and password are required"));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert - Test HTTP mapping: Validation error → 400 Bad Request
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        #endregion

        #region RefreshToken Tests - Basic HTTP Mapping

        [TestMethod]
        public async Task RefreshToken_ServiceReturnsSuccess_ReturnsOk()
        {
            // Arrange
            var refreshRequest = new RefreshTokenRequest { RefreshToken = "valid-token" };
            var loginResponse = new LoginResponse { Token = "new.token" };

            _mockAuthenticationService.Setup(s => s.RefreshTokenAsync(refreshRequest))
                .ReturnsAsync(new SystemResponse<LoginResponse>(loginResponse, "Success"));

            // Act
            var result = await _controller.RefreshToken(refreshRequest);

            // Assert - Test HTTP mapping: Service success → 200 OK
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        #endregion

        #region Logout Tests - Basic HTTP Mapping

        [TestMethod]
        public void Logout_ReturnsOk()
        {
            // Act
            var result = _controller.Logout();

            // Assert - Test HTTP mapping: Logout → 200 OK
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        #endregion

        // REMOVED: ModelState tests - these test ASP.NET Core framework behavior, not our code
    }
}
