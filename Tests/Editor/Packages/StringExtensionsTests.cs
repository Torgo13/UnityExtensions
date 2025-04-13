using NUnit.Framework;
using System.Collections.Generic;

namespace UnityExtensions.Packages.Tests
{
    class StringExtensionsTests
    {
        /// <summary>
        /// Edge Case: Empty string
        /// </summary>
        [Test]
        public void Remove_WithEmptyString_ReturnsEmptyString()
        {
            string input = string.Empty;
            var removeChars = new List<char> { 'a', 'b', 'c' };
            string result = input.Remove(removeChars);
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Edge Case: String with no characters to remove
        /// </summary>
        [Test]
        public void Remove_WithNoCharactersToRemove_ReturnsOriginalString()
        {
            string input = "Hello, World!";
            var removeChars = new List<char>();
            string result = input.Remove(removeChars);
            Assert.AreEqual(input, result);
        }

        /// <summary>
        /// Edge Case: String with characters that can be removed
        /// </summary>
        [Test]
        public void Remove_WithCharactersToRemove_RemovesThem()
        {
            string input = "Hello, World!";
            var removeChars = new List<char> { 'o', 'l' };
            string expected = "He, Wrd!";
            string result = input.Remove(removeChars);
            Assert.AreEqual(expected, result);
        }
        
        /// <summary>
        /// Edge Case: String where all characters are removed
        /// </summary>
        [Test]
        public void Remove_WithAllCharactersToRemove_ReturnsEmptyString()
        {
            string input = "abc";
            var removeChars = new List<char> { 'a', 'b', 'c' };
            string result = input.Remove(removeChars);
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Complex Inputs: Duplicate characters
        /// </summary>
        [Test]
        public void Remove_WithDuplicateCharacters_RemovesAllOccurrences()
        {
            string input = "Mississippi";
            var removeChars = new List<char> { 'i', 's' };
            string expected = "Mpp";
            string result = input.Remove(removeChars);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Complex Inputs: Whitespace and special characters
        /// </summary>
        [Test]
        public void Remove_WithWhitespaceAndSpecialCharacters_RemovesThem()
        {
            string input = "Hello, World!";
            var removeChars = new List<char> { ' ', '!', ',' };
            string expected = "HelloWorld";
            string result = input.Remove(removeChars);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Empty Removal List: Confirms that passing an empty list as removeChars results in the original string being returned.
        /// </summary>
        [Test]
        public void Remove_WithEmptyRemoveListAndTrimmedInput_ReturnsTrimmedInput()
        {
            string input = "   Hello, World!   ";
            var removeChars = new List<char>();
            string expected = input.Trim();
            string result = input.Trim().Remove(removeChars);
            Assert.AreEqual(expected, result);
        }
        
        /// <summary>
        /// Edge Case: Empty string
        /// </summary>
        [Test]
        public void RemoveEmptyLines_WithEmptyString_ReturnsEmptyString()
        {
            string input = string.Empty;
            string result = input.RemoveEmptyLines();
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Behaviour: Preserves non-empty lines
        /// </summary>
        [Test]
        public void RemoveEmptyLines_WithNoEmptyLines_ReturnsOriginalText()
        {
            string input = "This is a line.\nThis is another line.";
            string result = input.RemoveEmptyLines();
            Assert.AreEqual(input, result);
        }

        /// <summary>
        /// Behaviour: Removes trailing empty lines.
        /// </summary>
        [Test]
        public void RemoveEmptyLines_WithLeadingAndTrailingEmptyLines_RemovesThem()
        {
            string input = "\n\nThis is a line.\nThis is another line.\n\n";
            string expected = "This is a line.\nThis is another line.\n";
            string result = input.RemoveEmptyLines();
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Behaviour: Removes consecutive empty lines.
        /// </summary>
        [Test]
        public void RemoveEmptyLines_WithMultipleConsecutiveEmptyLines_RemovesExtraNewLines()
        {
            string input = "This is a line.\n\n\n\nThis is another line.\n\n\n";
            string expected = "This is a line.\nThis is another line.\n";
            string result = input.RemoveEmptyLines();
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Option Handling: Strips trailing whitespace when trimEnd is true.
        /// </summary>
        [Test]
        public void RemoveEmptyLines_WithTrimEndTrue_TrimsWhitespaceAtEnd()
        {
            string input = "This is a line.\n  \nThis is another line.\n\n\n   ";
            string expected = "This is a line.\nThis is another line.";
            string result = input.RemoveEmptyLines(trimEnd: true);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Option Handling: Preserves trailing whitespace when trimEnd is false.
        /// </summary>
        [Test]
        public void RemoveEmptyLines_WithTrimEndFalse_PreservesWhitespaceAtEnd()
        {
            string input = "This is a line.\n  \nThis is another line.\n\n\n   ";
            string expected = "This is a line.\nThis is another line.\n   ";
            string result = input.RemoveEmptyLines(trimEnd: false);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Mixed Line Endings: Tests check for robustness with line endings like \r, \n, and \r\n.
        /// </summary>
        [Test]
        public void RemoveEmptyLines_WithWhitespaceOnly_RemovesWhitespace()
        {
            string input = " \n \n \n";
            string result = input.RemoveEmptyLines();
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Mixed Line Endings: Tests check for robustness with line endings like \r, \n, and \r\n.
        /// </summary>
        [Test]
        public void RemoveEmptyLines_WithMixedLineEndings_ReturnsProperlyFormattedText()
        {
            string input = "Line 1\r\n\r\nLine 2\n\nLine 3\r\r";
            string expected = "Line 1\r\nLine 2\nLine 3\r";
            string result = input.RemoveEmptyLines();
            Assert.AreEqual(expected, result);
        }
    }
}
