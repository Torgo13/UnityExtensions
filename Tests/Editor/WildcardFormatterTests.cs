using NUnit.Framework;
using System.Collections.Generic;

namespace UnityExtensions.Tests
{
    public class WildcardFormatterTests
    {
        // A simple derived class to expose the Format method for testing
        private class TestWildcardFormatter : WildcardFormatter
        {
            public TestWildcardFormatter(Dictionary<string, string> replacements)
            {
                foreach (var pair in replacements)
                {
                    Replacements[pair.Key] = pair.Value;
                }
            }

            public string TestFormat(string input)
            {
                return Format(input);
            }
        }

        /// <summary>
        /// Verifies that the method correctly replaces wildcards with their values.
        /// </summary>
        [Test]
        public void Format_ReplacesWildcardsCorrectly()
        {
            // Arrange
            var replacements = new Dictionary<string, string>
            {
                { "{name}", "Alice" },
                { "{greeting}", "Hello" }
            };

            var formatter = new TestWildcardFormatter(replacements);
            string input = "{greeting}, {name}!";
            string expectedOutput = "Hello, Alice!";

            // Act
            string result = formatter.TestFormat(input);

            // Assert
            Assert.AreEqual(expectedOutput, result, "The Format method should replace wildcards correctly.");
        }

        /// <summary>
        /// Ensures that keys with null values are ignored during replacement.
        /// </summary>
        [Test]
        public void Format_IgnoresNullValues()
        {
            // Arrange
            var replacements = new Dictionary<string, string>
            {
                { "{name}", null },
                { "{greeting}", "Hello" }
            };

            var formatter = new TestWildcardFormatter(replacements);
            string input = "{greeting}, {name}!";
            string expectedOutput = "Hello, {name}!";

            // Act
            string result = formatter.TestFormat(input);

            // Assert
            Assert.AreEqual(expectedOutput, result, "The Format method should ignore replacements with null values.");
        }

        /// <summary>
        /// Checks that the method handles empty input without errors.
        /// </summary>
        [Test]
        public void Format_HandlesEmptyInput()
        {
            // Arrange
            var replacements = new Dictionary<string, string>
            {
                { "{name}", "Alice" }
            };

            var formatter = new TestWildcardFormatter(replacements);
            string input = "";
            string expectedOutput = "";

            // Act
            string result = formatter.TestFormat(input);

            // Assert
            Assert.AreEqual(expectedOutput, result, "The Format method should handle empty input gracefully.");
        }
    }
}
