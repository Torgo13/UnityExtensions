using NUnit.Framework;

namespace UnityExtensions.Editor.Tests
{
    class StringExtensionsTests
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/1.3.9/Unity.Entities.Editor.Tests/Extensions/StringExtensionsTests.cs
        #region Unity.Entities.Editor.Tests
        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Simple")]
        [TestCase("  Simple")]
        [TestCase("Simple  ")]
        [TestCase("`Simple")]
        [TestCase("`String`")]
        public void CanSingleQuoteAString(string value)
        {
            var trimmedValue = value.Trim('\'');
            Assert.That(value.SingleQuoted(), Is.EqualTo('\'' + trimmedValue + '\''));
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Simple")]
        [TestCase("  Simple")]
        [TestCase("Simple  ")]
        [TestCase("`Simple")]
        [TestCase("`String`")]
        [TestCase("\"String`")]
        [TestCase("\"String\"")]
        public void CanDoubleQuoteAString(string value)
        {
            var trimmedValue = value.Trim('\"');
            Assert.That(value.DoubleQuoted(), Is.EqualTo("\"" + trimmedValue + "\""));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("", "Key")]
        [TestCase("Value", "")]
        [TestCase("Value", "Key")]
        public void CanHyperLinkAString(string value, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Assert.That(value.ToHyperLink(string.Empty), Is.EqualTo($"<a>{value}</a>"));
            }
            else
            {
                Assert.That(value.ToHyperLink(key), Is.EqualTo($"<a {key}={value.DoubleQuoted()}>{value}</a>"));
            }
        }

        [Test]
        [TestCase("Simple", "Simple")]
        [TestCase("Simple With Space", "Simple_With_Space")]
        [TestCase("234v54.345", "234v54_345")]
        [TestCase("#Hashtag", "_Hashtag")]
        public void CanConvertStringToIdentifier(string value, string expected)
        {
            Assert.That(value.ToIdentifier(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase("Simple", "Simple")]
        [TestCase("/Simple", "/Simple")]
        [TestCase("Simple/", "Simple/")]
        [TestCase("\\Simple", "/Simple")]
        [TestCase("Simple\\", "Simple/")]
        [TestCase("Simple/Simple/Simple", "Simple/Simple/Simple")]
        [TestCase("Simple/Simple\\Simple", "Simple/Simple/Simple")]
        public void CanConvertStringToForwardSlashes(string value, string expected)
        {
            Assert.That(value.ToForwardSlash(), Is.EqualTo(expected));
        }
        #endregion // Unity.Entities.Editor.Tests

        [Test]
        [TestCase("simple", "Simple")]
        [TestCase("Simple", "Simple")]
        [TestCase("\\Simple", "\\Simple")]
        public void FirstToUpper_Test(string value, string expected)
        {
            Assert.That(value.FirstToUpper(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase("HelloWorld", "Hello World")]
        [TestCase("HelloWORLDAgain", "Hello WORLD Again")]
        public void InsertSpacesBetweenWords_Test(string value, string expected)
        {
            Assert.That(value.InsertSpacesBetweenWords(), Is.EqualTo(expected));
        }

        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Tests/Editor/Misc/StringExtensionsTests.cs
        #region UnityEditor.GraphToolsFoundation.Overdrive.Tests.Misc
        [TestCase("AnUpperCamelCaseString", "An Upper Camel Case String")]
        [TestCase("aLowerCamelCaseString", "A Lower Camel Case String")]
        [TestCase("AnACRONYMString", "An ACRONYM String")]
        public void NificyTest(string value, string expected)
        {
            Assert.That(value.CamelToPascalCaseWithSpace(preserveAcronyms: true), Is.EqualTo(expected));
        }

        [TestCase("Asd Qwe_Asd-rr", "Asd_Qwe_Asd_rr")]
        [TestCase("asd%-$yy", "asd___yy")]
        [TestCase("uu%yy", "uu_yy")]
        [TestCase("asd--qwe_", "asd__qwe_")]
        public void CodifyNameTest(string actual, string expected)
        {
            Assert.That(actual.CodifyString(), Is.EqualTo(expected));
        }
        #endregion // UnityEditor.GraphToolsFoundation.Overdrive.Tests.Misc
        
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/StringExtensionsTests.cs
        #region UnityEditor.Rendering.Tests
        static TestCaseData[] s_Input =
        {
            new TestCaseData("A/B")
                .Returns("A_B")
                .SetName("Fogbugz - 1408027"),
            new TestCaseData("A" + new string(System.IO.Path.GetInvalidFileNameChars()) + "B")
                .Returns("A_B")
                .SetName("All Chars Replaced"),
            new TestCaseData("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ")
                .Returns("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ")
                .SetName("Nothing replaced")
        };

        [Test, TestCaseSource(nameof(s_Input))]
        [Property("Fogbugz", "1408027")]
        public string ReplaceInvalidFileNameCharacters(string input)
        {
            return input.ReplaceInvalidFileNameCharacters();
        }

        [Test]
        public void CheckExtensionTests(
            [Values("Folder1/file.testextension", "Folder1/file.TestExtension", "Folder1/file.TESTEXTENSION", "file.testextension", "Folder1/Folder2/Folder3/file.testextension")] string input,
            [Values(".testextension", ".TESTEXTENSION", ".TestExtension", ".wrong")]string extension)
        {
            bool expected = input.ToLower().EndsWith(extension.ToLower());
            bool actual = input.HasExtension(extension);
            Assert.AreEqual(expected, actual);
        }
        #endregion // UnityEditor.Rendering.Tests
    }
}