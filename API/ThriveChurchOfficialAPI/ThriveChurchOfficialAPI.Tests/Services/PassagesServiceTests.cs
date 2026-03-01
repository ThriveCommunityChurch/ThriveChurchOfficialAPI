using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Tests.Services
{
    [TestClass]
    public class PassagesServiceTests
    {
        private Mock<IPassagesRepository> _mockPassagesRepository;
        private PassagesService _passagesService;

        [TestInitialize]
        public void Setup()
        {
            _mockPassagesRepository = new Mock<IPassagesRepository>();
            _passagesService = new PassagesService(_mockPassagesRepository.Object);
        }

        #region GetSinglePassageForSearch - Validation Tests

        [TestMethod]
        public async Task GetSinglePassageForSearch_NullSearchCriteria_ReturnsError()
        {
            // Arrange
            string searchCriteria = null;

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "searchCriteria"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_EmptySearchCriteria_ReturnsError()
        {
            // Arrange
            string searchCriteria = "";

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.NullProperty, "searchCriteria"), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_TooShortSearchCriteria_ReturnsError()
        {
            // Arrange
            string searchCriteria = "ab"; // Less than 3 characters

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.PropertyNameCharactersLengthRange, "searchCriteria", 3, 200), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_TooLongSearchCriteria_ReturnsError()
        {
            // Arrange
            string searchCriteria = new string('a', 201); // More than 200 characters

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(string.Format(SystemMessages.PropertyNameCharactersLengthRange, "searchCriteria", 3, 200), result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_ExactlyThreeCharacters_DoesNotReturnLengthError()
        {
            // Arrange
            string searchCriteria = "abc"; // Exactly 3 characters (valid)
            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync((PassageTextInfo)null);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Should not fail on length validation, but will fail on ESV API null response
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.ErrorWithESVApi, result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_Exactly200Characters_DoesNotReturnLengthError()
        {
            // Arrange
            string searchCriteria = new string('a', 200); // Exactly 200 characters (valid)
            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync((PassageTextInfo)null);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Should not fail on length validation, but will fail on ESV API null response
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.ErrorWithESVApi, result.ErrorMessage);
        }

        #endregion

        #region GetSinglePassageForSearch - Cache Tests

        [TestMethod]
        public async Task GetSinglePassageForSearch_CacheHit_ReturnsCachedPassage()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            var cachedPassage = new BiblePassage
            {
                PassageRef = searchCriteria,
                PassageText = "For God so loved the world..."
            };

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(cachedPassage, "Success!"));

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("For God so loved the world...", result.Result.Passage);
            _mockPassagesRepository.Verify(r => r.GetPassagesForSearch(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_CacheMissWithError_GoesToEsvApi()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found in cache"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync((PassageTextInfo)null);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Demonstrates cache miss path was taken
            _mockPassagesRepository.Verify(r => r.GetPassagesForSearch(searchCriteria), Times.Once);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_CacheMissWithNullResult_GoesToEsvApi()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            // Cache returns success but with null result
            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>((BiblePassage)null, "Success but null"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync((PassageTextInfo)null);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Demonstrates cache miss path was taken
            _mockPassagesRepository.Verify(r => r.GetPassagesForSearch(searchCriteria), Times.Once);
        }

        #endregion

        #region GetSinglePassageForSearch - ESV API Tests

        [TestMethod]
        public async Task GetSinglePassageForSearch_EsvApiReturnsNull_ReturnsError()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync((PassageTextInfo)null);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.ErrorWithESVApi, result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_EsvApiReturnsEmptyPassages_ReturnsError()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo { passages = new List<string>() });

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.ErrorWithESVApi, result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_EsvApiReturnsNullPassages_ReturnsError()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo { passages = null });

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(SystemMessages.ErrorWithESVApi, result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_EsvApiSuccess_ReturnsFormattedPassage()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            string rawPassage = "John 3:16\n\n[16] For God so loved the world (ESV)";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "John 3:16"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Result.Passage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_EsvApiSuccess_CachesResult()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            string rawPassage = "For God so loved the world (ESV)";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "John 3:16"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Verify cache was set
            _mockPassagesRepository.Verify(r => r.SetPassageForCache(It.Is<BiblePassage>(p =>
                p.PassageRef == searchCriteria)), Times.Once);
        }

        #endregion

        #region RemoveFooterTagsAndFormatVerseNumbers Tests (via GetSinglePassageForSearch)

        [TestMethod]
        public async Task GetSinglePassageForSearch_PassageWithFootnoteNumbers_RemovesFootnotes()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            // Passage with footnote markers like (1), (2)
            string rawPassage = "For God so loved (1) the world (2) (ESV)";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "John 3:16"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Footnote markers should be removed
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsFalse(result.Result.Passage.Contains("(1)"));
            Assert.IsFalse(result.Result.Passage.Contains("(2)"));
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_PassageWithVerseNumbers_FormatsToSuperscript()
        {
            // Arrange
            string searchCriteria = "John 3:16-17";
            // Passage with verse markers like [16], [17]
            string rawPassage = "[16] For God so loved the world [17] For God did not send (ESV)";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "John 3:16-17"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Verse numbers should be converted to superscript
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            // [16] should be replaced with superscript
            Assert.IsFalse(result.Result.Passage.Contains("[16]"));
            Assert.IsFalse(result.Result.Passage.Contains("[17]"));
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_PassageWithNonNumericBrackets_PreservesContent()
        {
            // Arrange
            string searchCriteria = "Test passage";
            // Passage with non-numeric content in brackets
            string rawPassage = "(abc) For God [xyz] so loved (ESV)";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "Test"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Non-numeric brackets should not be removed
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            // (abc) should still be in the passage since abc is not a number
            Assert.IsTrue(result.Result.Passage.Contains("(abc)"));
            Assert.IsTrue(result.Result.Passage.Contains("[xyz]"));
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_PassageWithMultiDigitVerses_FormatsCorrectly()
        {
            // Arrange
            string searchCriteria = "Psalm 119:100-105";
            // Passage with multi-digit verse numbers
            string rawPassage = "[100] I understand [101] I refrain [105] Your word (ESV)";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "Psalm 119:100-105"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            // Multi-digit verse numbers should be converted
            Assert.IsFalse(result.Result.Passage.Contains("[100]"));
            Assert.IsFalse(result.Result.Passage.Contains("[101]"));
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_PassageWithFootnotes_RemovesFootnoteSection()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            // Passage with footnotes section
            string rawPassage = "For God so loved the world\n\nFootnotes\n(1) Greek text here(ESV)";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "John 3:16"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Footnotes section should be removed
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.IsFalse(result.Result.Passage.Contains("Footnotes"));
            Assert.IsFalse(result.Result.Passage.Contains("Greek text"));
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_PassageWithCanonical_RemovesCanonicalHeader()
        {
            // Arrange
            string searchCriteria = "John 3:16";
            // Passage with canonical header
            string rawPassage = "John 3:16\n\nFor God so loved the world (ESV)";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "John 3:16"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Canonical header should be removed
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            // The passage should not start with the canonical reference
            Assert.IsFalse(result.Result.Passage.StartsWith("John 3:16\n\n"));
        }

        #endregion

        #region BaseService GetBetween Edge Cases

        [TestMethod]
        public async Task GetSinglePassageForSearch_EmptyPassageFromApi_ProcessesWithoutError()
        {
            // Arrange - This test covers BaseService.GetBetween lines 49-51
            // where GetBetween receives an empty string source
            string searchCriteria = "John 3:16";
            string rawPassage = ""; // Empty passage - triggers GetBetween null/empty check

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "John 3:16"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Should process without throwing, returns empty passage
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("", result.Result.Passage);
        }

        [TestMethod]
        public async Task GetSinglePassageForSearch_FootnotesAfterESVMarker_HandlesCorrectly()
        {
            // Arrange - This test covers BaseService.GetBetween lines 60-62
            // where End == -1 because "(ESV)" appears before "Footnotes"
            string searchCriteria = "John 3:16";
            // Passage where (ESV) appears BEFORE Footnotes
            string rawPassage = "For God so loved (ESV) the world Footnotes here";

            _mockPassagesRepository.Setup(r => r.GetPassageFromCache(searchCriteria))
                .ReturnsAsync(new SystemResponse<BiblePassage>(true, "Not found"));
            _mockPassagesRepository.Setup(r => r.GetPassagesForSearch(searchCriteria))
                .ReturnsAsync(new PassageTextInfo
                {
                    passages = new List<string> { rawPassage },
                    canonical = "John 3:16"
                });
            _mockPassagesRepository.Setup(r => r.SetPassageForCache(It.IsAny<BiblePassage>()))
                .ReturnsAsync((BiblePassage p) => p);

            // Act
            var result = await _passagesService.GetSinglePassageForSearch(searchCriteria);

            // Assert - Should handle the edge case gracefully
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasErrors);
            // The passage should still be processed even when Footnotes/ESV markers are in unusual positions
            Assert.IsNotNull(result.Result.Passage);
        }

        #endregion
    }
}

