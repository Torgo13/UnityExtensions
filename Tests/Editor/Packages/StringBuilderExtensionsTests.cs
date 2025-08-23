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
    }
}
