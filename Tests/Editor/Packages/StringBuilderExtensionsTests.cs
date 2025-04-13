using NUnit.Framework;
using System;
using System.Text;

namespace UnityExtensions.Packages.Tests
{
    class StringBuilderExtensionsTests
    {
        //https://github.com/Unity-Technologies/Graphics/blob/274b2c01bdceac862ed35742dcfa90e48e5f3248/Packages/com.unity.shadergraph/Editor/Utilities/StringBuilderExtensions.cs
        #region UnityEditor.ShaderGraph
        /// <summary>
        /// Ensures that the function handles an empty lines string gracefully.
        /// </summary>
        [Test]
        public void AppendIndentedLines_AppendsEmptyLines()
        {
            // Arrange
            var stringBuilder = new StringBuilder();
            var lines = string.Empty;
            var indentation = "  ";

            // Act
            stringBuilder.AppendIndentedLines(lines, indentation);

            // Assert
            Assert.AreEqual(string.Empty, stringBuilder.ToString());
        }

        /// <summary>
        /// Verifies that the function correctly indents and appends a single line.
        /// </summary>
        [Test]
        public void AppendIndentedLines_AppendsSingleLine()
        {
            // Arrange
            var stringBuilder = new StringBuilder();
            var lines = "Hello, World!";
            var indentation = "  ";

            // Act
            stringBuilder.AppendIndentedLines(lines, indentation);

            // Assert
            Assert.AreEqual("  Hello, World!" + Environment.NewLine, stringBuilder.ToString());
        }

        /// <summary>
        /// Tests appending multiple lines with different content.
        /// </summary>
        [Test]
        public void AppendIndentedLines_AppendsMultipleLines()
        {
            // Arrange
            var stringBuilder = new StringBuilder();
            var lines = "First Line" + Environment.NewLine + "Second Line";
            var indentation = "--";

            // Act
            stringBuilder.AppendIndentedLines(lines, indentation);

            // Assert
            Assert.AreEqual(
                "--First Line" + Environment.NewLine +
                "--Second Line" + Environment.NewLine,
                stringBuilder.ToString());
        }

        /// <summary>
        /// Ensures that the function handles input where lines end with newline characters.
        /// </summary>
        [Test]
        public void AppendIndentedLines_HandlesLinesEndingWithNewline()
        {
            // Arrange
            var stringBuilder = new StringBuilder();
            var lines = "First Line" + Environment.NewLine + "Second Line" + Environment.NewLine;
            var indentation = "***";

            // Act
            stringBuilder.AppendIndentedLines(lines, indentation);

            // Assert
            Assert.AreEqual(
                "***First Line" + Environment.NewLine +
                "***Second Line" + Environment.NewLine,
                stringBuilder.ToString());
        }

        /// <summary>
        /// Tests input that contains only newline characters.
        /// </summary>
        [Test]
        public void AppendIndentedLines_HandlesOnlyNewLineCharacters()
        {
            // Arrange
            var stringBuilder = new StringBuilder();
            var lines = Environment.NewLine + Environment.NewLine;
            var indentation = "->";

            // Act
            stringBuilder.AppendIndentedLines(lines, indentation);

            // Assert
            Assert.AreEqual(
                "->" + Environment.NewLine +
                "->" + Environment.NewLine,
                stringBuilder.ToString());
        }

        /// <summary>
        /// Ensures that the function appends to a pre-existing content in the StringBuilder without overwriting it.
        /// </summary>
        [Test]
        public void AppendIndentedLines_HandlesPrepopulatedStringBuilder()
        {
            // Arrange
            var stringBuilder = new StringBuilder("Pre-existing content" + Environment.NewLine);
            var lines = "New Line";
            var indentation = "+++";

            // Act
            stringBuilder.AppendIndentedLines(lines, indentation);

            // Assert
            Assert.AreEqual(
                "Pre-existing content" + Environment.NewLine +
                "+++New Line" + Environment.NewLine,
                stringBuilder.ToString());
        }
        #endregion // UnityEditor.ShaderGraph
        
        /// <summary>
        /// Verifies that the function gracefully handles an empty StringBuilder.
        /// </summary>
        [Test]
        public void RemoveChar_RemovesCharacterFromEmptyStringBuilder()
        {
            // Arrange
            var stringBuilder = new StringBuilder();
            var removeChar = 'a';

            // Act
            stringBuilder.Remove(removeChar);

            // Assert
            Assert.AreEqual(string.Empty, stringBuilder.ToString());
        }

        /// <summary>
        /// Ensures correct functionality for removing one occurrence of a character.
        /// </summary>
        [Test]
        public void RemoveChar_RemovesSingleOccurrence()
        {
            // Arrange
            var stringBuilder = new StringBuilder("abc");
            var removeChar = 'b';

            // Act
            stringBuilder.Remove(removeChar);

            // Assert
            Assert.AreEqual("ac", stringBuilder.ToString());
        }

        /// <summary>
        /// Checks removal of a character appearing multiple times.
        /// </summary>
        [Test]
        public void RemoveChar_RemovesMultipleOccurrences()
        {
            // Arrange
            var stringBuilder = new StringBuilder("banana");
            var removeChar = 'a';

            // Act
            stringBuilder.Remove(removeChar);

            // Assert
            Assert.AreEqual("bnn", stringBuilder.ToString());
        }

        /// <summary>
        /// Ensures no changes when the target character is absent.
        /// </summary>
        [Test]
        public void RemoveChar_DoesNotRemoveWhenCharacterNotFound()
        {
            // Arrange
            var stringBuilder = new StringBuilder("hello");
            var removeChar = 'z';

            // Act
            stringBuilder.Remove(removeChar);

            // Assert
            Assert.AreEqual("hello", stringBuilder.ToString());
        }

        /// <summary>
        /// Confirms all consecutive occurrences are removed.
        /// </summary>
        [Test]
        public void RemoveChar_RemovesAllOccurrencesIncludingConsecutive()
        {
            // Arrange
            var stringBuilder = new StringBuilder("aaabbbaaa");
            var removeChar = 'a';

            // Act
            stringBuilder.Remove(removeChar);

            // Assert
            Assert.AreEqual("bbb", stringBuilder.ToString());
        }

        /// <summary>
        /// Validates the ArgumentNullException is thrown when sb is null.
        /// </summary>
        [Test]
        public void RemoveChar_HandlesNullStringBuilder()
        {
            // Arrange
            StringBuilder stringBuilder = null;
            var removeChar = 'x';

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => stringBuilder.Remove(removeChar));
        }

        /// <summary>
        /// Ensures proper handling of StringBuilder with pre-existing content.
        /// </summary>
        [Test]
        public void RemoveChar_HandlesPrepopulatedStringBuilder()
        {
            // Arrange
            var stringBuilder = new StringBuilder("initial content");
            var removeChar = 'i';

            // Act
            stringBuilder.Remove(removeChar);

            // Assert
            Assert.AreEqual("ntal content", stringBuilder.ToString());
        }

        /// <summary>
        /// Tests removing the null character, ensuring no unexpected behavior.
        /// </summary>
        [Test]
        public void RemoveChar_HandlesEmptyCharacterInput()
        {
            // Arrange
            var stringBuilder = new StringBuilder("empty test");
            var removeChar = '\0'; // Null character

            // Act
            stringBuilder.Remove(removeChar);

            // Assert
            Assert.AreEqual("empty test", stringBuilder.ToString());
        }

        /// <summary>
        /// Verifies the function's ability to handle and remove a Unicode character.
        /// </summary>
        [Test]
        public void RemoveChar_RemovesUnicodeCharacter()
        {
            // Arrange
            var stringBuilder = new StringBuilder("cafè ☕");
            var removeChar = 'è';

            // Act
            stringBuilder.Remove(removeChar);

            // Assert
            Assert.AreEqual("caf ☕", stringBuilder.ToString());
        }
        
        [Test]
        public void ReplaceStringWithInt_ReplacesSingleOccurrenceCaseSensitive()
        {
            // Arrange
            var stringBuilder = new StringBuilder("Hello, world!");
            var oldValue = "world";
            var newValue = 123;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: false);

            // Assert
            Assert.AreEqual("Hello, 123!", stringBuilder.ToString());
        }

        [Test]
        public void ReplaceStringWithInt_ReplacesMultipleOccurrencesCaseSensitive()
        {
            // Arrange
            var stringBuilder = new StringBuilder("world world world!");
            var oldValue = "world";
            var newValue = 456;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: false);

            // Assert
            Assert.AreEqual("456 456 456!", stringBuilder.ToString());
        }

        [Test]
        public void ReplaceStringWithInt_DoesNotReplaceIfCaseDoesNotMatch()
        {
            // Arrange
            var stringBuilder = new StringBuilder("Hello, World!");
            var oldValue = "world";
            var newValue = 789;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: false);

            // Assert
            Assert.AreEqual("Hello, World!", stringBuilder.ToString());
        }

        [Test]
        public void ReplaceStringWithInt_ReplacesSingleOccurrenceIgnoreCase()
        {
            // Arrange
            var stringBuilder = new StringBuilder("Hello, World!");
            var oldValue = "world";
            var newValue = 987;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: true);

            // Assert
            Assert.AreEqual("Hello, 987!", stringBuilder.ToString());
        }

        [Test]
        public void ReplaceStringWithInt_ReplacesMultipleOccurrencesIgnoreCase()
        {
            // Arrange
            var stringBuilder = new StringBuilder("World world WoRlD!");
            var oldValue = "world";
            var newValue = 654;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: true);

            // Assert
            Assert.AreEqual("654 654 654!", stringBuilder.ToString());
        }

        [Test]
        public void ReplaceStringWithInt_HandlesEmptyStringBuilder()
        {
            // Arrange
            var stringBuilder = new StringBuilder();
            var oldValue = "test";
            var newValue = 42;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: false);

            // Assert
            Assert.AreEqual(string.Empty, stringBuilder.ToString());
        }

        [Test]
        public void ReplaceStringWithInt_HandlesEmptyOldValue()
        {
            // Arrange
            var stringBuilder = new StringBuilder("Some content");
            var oldValue = string.Empty;
            var newValue = 99;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: false);

            // Assert
            Assert.AreEqual("Some content", stringBuilder.ToString());
        }

        [Test]
        public void ReplaceStringWithInt_HandlesNullOldValue()
        {
            // Arrange
            var stringBuilder = new StringBuilder("Null test");
            string oldValue = null;
            var newValue = 123;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: false);

            // Assert
            Assert.AreEqual("Null test", stringBuilder.ToString());
        }

        [Test]
        public void ReplaceStringWithInt_DoesNotReplaceIfOldValueNotFound()
        {
            // Arrange
            var stringBuilder = new StringBuilder("Not here!");
            var oldValue = "missing";
            var newValue = 456;

            // Act
            stringBuilder.Replace(oldValue, newValue, ignoreCase: true);

            // Assert
            Assert.AreEqual("Not here!", stringBuilder.ToString());
        }
    }
}
