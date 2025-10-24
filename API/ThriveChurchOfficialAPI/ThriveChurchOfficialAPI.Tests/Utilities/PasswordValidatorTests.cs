using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThriveChurchOfficialAPI.Core.Utilities;

namespace ThriveChurchOfficialAPI.Tests.Utilities
{
    /// <summary>
    /// Unit tests for PasswordValidator utility
    /// </summary>
    [TestClass]
    public class PasswordValidatorTests
    {
        #region IsValidPassword Tests

        [TestMethod]
        public void IsValidPassword_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var password = "ValidPassword123";

            // Act
            var result = PasswordValidator.IsValidPassword(password);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidPassword_ExactlyMinimumLength_ReturnsTrue()
        {
            // Arrange
            var password = "1234567890"; // Exactly 10 characters

            // Act
            var result = PasswordValidator.IsValidPassword(password);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidPassword_TooShort_ReturnsFalse()
        {
            // Arrange
            var password = "123456789"; // 9 characters

            // Act
            var result = PasswordValidator.IsValidPassword(password);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidPassword_NullPassword_ReturnsFalse()
        {
            // Act
            var result = PasswordValidator.IsValidPassword(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidPassword_EmptyPassword_ReturnsFalse()
        {
            // Act
            var result = PasswordValidator.IsValidPassword("");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidPassword_WhitespacePassword_ReturnsFalse()
        {
            // Act
            var result = PasswordValidator.IsValidPassword("   ");

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region ValidatePasswordWithMessage Tests

        [TestMethod]
        public void ValidatePasswordWithMessage_ValidPassword_ReturnsNull()
        {
            // Arrange
            var password = "ValidPassword123";

            // Act
            var result = PasswordValidator.ValidatePasswordWithMessage(password);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ValidatePasswordWithMessage_TooShort_ReturnsErrorMessage()
        {
            // Arrange
            var password = "short";

            // Act
            var result = PasswordValidator.ValidatePasswordWithMessage(password);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("10 characters"));
        }

        [TestMethod]
        public void ValidatePasswordWithMessage_NullPassword_ReturnsErrorMessage()
        {
            // Act
            var result = PasswordValidator.ValidatePasswordWithMessage(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Password is required", result);
        }

        [TestMethod]
        public void ValidatePasswordWithMessage_EmptyPassword_ReturnsErrorMessage()
        {
            // Act
            var result = PasswordValidator.ValidatePasswordWithMessage("");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Password is required", result);
        }

        #endregion

        #region GetPasswordRequirements Tests

        [TestMethod]
        public void GetPasswordRequirements_ReturnsCorrectMessage()
        {
            // Act
            var result = PasswordValidator.GetPasswordRequirements();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("10 characters"));
        }

        #endregion
    }
}
