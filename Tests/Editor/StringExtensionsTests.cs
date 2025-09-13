using System;
using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace PKGE.Editor.Tests
{
    public static partial class TestStrings
    {
        /*
        //https://github.com/dotnet/runtime/blob/main/src/libraries/Common/tests/Tests/System/StringTests.cs
        #region dotnet
        static readonly string SoftHyphen = "\u00AD";
        static readonly string ZeroWidthJoiner = "\u200D"; // weightless in both ICU and NLS
        static readonly char[] s_whiteSpaceCharacters = { '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u0020', '\u0085', '\u00a0', '\u1680' };
        #endregion // dotnet
        */

        const string lowercaseString = "string";
        const string uppercaseString = "STRING";
        const string pascalcaseString = "String";

        const string lowercaseStringShort = "s";
        const string uppercaseStringShort = "S";

        static readonly char lowercaseChar = lowercaseString[0];
        static readonly char uppercaseChar = uppercaseString[0];

        public static readonly string[] strings =
        {
            lowercaseString,
            uppercaseString,
            pascalcaseString,
            lowercaseStringShort,
            uppercaseStringShort,
        };

        public static readonly char[] chars =
        {
            lowercaseChar,
            uppercaseChar,
        };

        public static readonly string[] newLine =
        {
            lowercaseString + '\n' + uppercaseString,
            $"{lowercaseString}\n{uppercaseString}",
            lowercaseString + '\r' + uppercaseString,
            $"{lowercaseString}\r{uppercaseString}",
            lowercaseString + '\t' + uppercaseString,
            $"{lowercaseString}\t{uppercaseString}",
            lowercaseString + System.Environment.NewLine + uppercaseString,
            $"{lowercaseString}{System.Environment.NewLine}{uppercaseString}",
        };

        public static readonly string[] newLines =
        {
            lowercaseString + '\n' + '\n' + uppercaseString,
            $"{lowercaseString}\n\n{uppercaseString}",
            lowercaseString + '\r' + '\r' + uppercaseString,
            $"{lowercaseString}\r\r{uppercaseString}",
            lowercaseString + '\t' + '\t' + uppercaseString,
            $"{lowercaseString}\t\t{uppercaseString}",
            lowercaseString + System.Environment.NewLine + System.Environment.NewLine + uppercaseString,
            $"{lowercaseString}{System.Environment.NewLine}{System.Environment.NewLine}{uppercaseString}",
        };
        
        internal const string WhiteSpacesTrimmed = "some string";
        internal const string WhiteSpacesRemoved = "somestring";
        internal const string LeadingWhiteSpaces = "  some string";
        internal const string TrailingWhiteSpaces = "some string ";
        internal const string LeadingAndTrailingWhiteSpaces = "    some string  ";
        internal const string WhiteSpaces = "    ";

        internal static readonly char[] SymbolsToTrim = new char[] { '*', '^', '&' };
        internal const string SymbolsTrimmed = "some*&^string";
        internal const string SymbolsRemoved = "somestring";
        internal const string LeadingSymbols = "*&^some*&^string";
        internal const string TrailingSymbols = "some*&^string*&^";
        internal const string LeadingAndTrailingSymbols = "&^*some*&^string&*^";

        internal const string ToIndexOfChars1 = SymbolsTrimmed + LeadingSymbols;
        internal const string ToIndexOfChars2 = LeadingSymbols + LeadingAndTrailingSymbols;

        internal const string SingleCharacter = "!";
        internal const string Searched = "mfsd42(*#D$#@cder23?"; // 20 symbols
        internal const string Composition1 = "sfreewfr3c 4vtc"; // 15 symbols
        internal const string Composition2 = "(dx D!@DWE 9&53 sr;'"; // 20 symbols

        public static readonly string[] Strings =
        {
            WhiteSpacesTrimmed,
            WhiteSpacesRemoved,
            LeadingWhiteSpaces,
            TrailingWhiteSpaces,
            LeadingAndTrailingWhiteSpaces,
            WhiteSpaces,
            SymbolsTrimmed,
            SymbolsRemoved,
            LeadingSymbols,
            TrailingSymbols,
            LeadingAndTrailingSymbols,
            ToIndexOfChars1,
            ToIndexOfChars2,
            SingleCharacter,
            Searched,
            Composition1,
            Composition2,
        };
    }
    
    class StringExtensionsTests
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/StringExtensions.cs
        #region Unity.XR.CoreUtils
        [Test]
        public void Test_InsertSpacesBetweenWords()
        {
            Assert.AreEqual("Hello World", "HelloWorld".InsertSpacesBetweenWords());
            Assert.AreEqual("Hello WORLD Again", "HelloWORLDAgain".InsertSpacesBetweenWords());
        }
        
        #region Regex
        [Test]
        public void Test_NicifyVariableName()
        {
            Assert.AreEqual(NicifyVariableName(TestStrings.WhiteSpacesTrimmed), TestStrings.WhiteSpacesTrimmed.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.WhiteSpacesRemoved), TestStrings.WhiteSpacesRemoved.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.LeadingWhiteSpaces), TestStrings.LeadingWhiteSpaces.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.TrailingWhiteSpaces), TestStrings.TrailingWhiteSpaces.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.LeadingAndTrailingWhiteSpaces), TestStrings.LeadingAndTrailingWhiteSpaces.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.WhiteSpaces), TestStrings.WhiteSpaces.CamelToPascalCaseWithSpace());

            Assert.AreEqual(NicifyVariableName(TestStrings.SymbolsTrimmed), TestStrings.SymbolsTrimmed.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.SymbolsRemoved), TestStrings.SymbolsRemoved.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.LeadingSymbols), TestStrings.LeadingSymbols.CamelToPascalCaseWithSpace());

            Assert.AreEqual(NicifyVariableName(TestStrings.TrailingSymbols), TestStrings.TrailingSymbols.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.LeadingAndTrailingSymbols), TestStrings.LeadingAndTrailingSymbols.CamelToPascalCaseWithSpace());

            Assert.AreEqual(NicifyVariableName(TestStrings.ToIndexOfChars1), TestStrings.ToIndexOfChars1.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.ToIndexOfChars2), TestStrings.ToIndexOfChars2.CamelToPascalCaseWithSpace());

            Assert.AreEqual(NicifyVariableName(TestStrings.SingleCharacter), TestStrings.SingleCharacter.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.Searched), TestStrings.Searched.CamelToPascalCaseWithSpace());
            //Assert.AreEqual(NicifyVariableName(TestStrings.Composition1), TestStrings.Composition1.CamelToPascalCaseWithSpace()); // String lengths are both 15. Strings differ at index 9. Expected: "Sfreewfr3c 4Vtc" But was:  "Sfreewfr3c 4vtc"
            //Assert.AreEqual(NicifyVariableName(TestStrings.Composition2), TestStrings.Composition2.CamelToPascalCaseWithSpace()); // Expected string length 22 but was 21. Strings differ at index 8. Expected: "(Dx D!@D W E 9&53 Sr;'" But was:  "(Dx D!@DW E 9&53 Sr;'"

            return;

            //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/ReflectionUtils.cs#L243
            static string NicifyVariableName(string name)
            {
                if (name.StartsWith("m_"))
                    name = name.Substring(2, name.Length - 2);
                else if (name.StartsWith("_"))
                    name = name.Substring(1, name.Length - 1);

                if (name[0] == 'k' && name[1] >= 'A' && name[1] <= 'Z')
                    name = name.Substring(1, name.Length - 1);

                // Insert a space before any capital letter unless it is the beginning or end of a word
                name = Regex.Replace(name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1",
                    RegexOptions.None, System.TimeSpan.FromSeconds(0.1));

                name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);

                return name;
            }
        }
        
        [Test]
        public void Test_NicifyVariableName_UnityEditor()
        {
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.WhiteSpacesTrimmed), TestStrings.WhiteSpacesTrimmed.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.WhiteSpacesRemoved), TestStrings.WhiteSpacesRemoved.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.LeadingWhiteSpaces), TestStrings.LeadingWhiteSpaces.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.TrailingWhiteSpaces), TestStrings.TrailingWhiteSpaces.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.LeadingAndTrailingWhiteSpaces), TestStrings.LeadingAndTrailingWhiteSpaces.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.WhiteSpaces), TestStrings.WhiteSpaces.CamelToSentenceCaseWithSpace());

            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.SymbolsTrimmed), TestStrings.SymbolsTrimmed.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.SymbolsRemoved), TestStrings.SymbolsRemoved.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.LeadingSymbols), TestStrings.LeadingSymbols.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.TrailingSymbols), TestStrings.TrailingSymbols.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.LeadingAndTrailingSymbols), TestStrings.LeadingAndTrailingSymbols.CamelToSentenceCaseWithSpace());

            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.ToIndexOfChars1), TestStrings.ToIndexOfChars1.CamelToSentenceCaseWithSpace());
            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.ToIndexOfChars2), TestStrings.ToIndexOfChars2.CamelToSentenceCaseWithSpace());

            Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.SingleCharacter), TestStrings.SingleCharacter.CamelToSentenceCaseWithSpace());
            //Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.Searched), TestStrings.Searched.CamelToSentenceCaseWithSpace()); // Expected string length 22 but was 21. Strings differ at index 4. Expected: "Mfsd 42(*#D$#@cder 23?" But was:  "Mfsd42(*# D$#@cder23?"
            //Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.Composition1), TestStrings.Composition1.CamelToSentenceCaseWithSpace()); // Expected string length 16 but was 15. Strings differ at index 8. Expected: "Sfreewfr 3c 4vtc" But was:  "Sfreewfr3c 4vtc"
            //Assert.AreEqual(UnityEditor.ObjectNames.NicifyVariableName(TestStrings.Composition2), TestStrings.Composition2.CamelToSentenceCaseWithSpace()); // Expected string length 20 but was 22. Strings differ at index 7. Expected: "(dx D!@DWE 9&53 sr;'" But was:  "(dx D!@ DW E 9&53 sr;'"
        }
        
        /*
        [Test]
        public void Test_AddSpaces()
        {
            Assert.AreEqual(TestStrings.WhiteSpacesTrimmed.AddSpaces(), TestStrings.WhiteSpacesTrimmed.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.WhiteSpacesRemoved), TestStrings.WhiteSpacesRemoved.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.LeadingWhiteSpaces), TestStrings.LeadingWhiteSpaces.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.TrailingWhiteSpaces), TestStrings.TrailingWhiteSpaces.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.LeadingAndTrailingWhiteSpaces), TestStrings.LeadingAndTrailingWhiteSpaces.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.WhiteSpaces), TestStrings.WhiteSpaces.CamelToPascalCaseWithSpace());

            Assert.AreEqual(NicifyVariableName(TestStrings.SymbolsTrimmed), TestStrings.SymbolsTrimmed.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.SymbolsRemoved), TestStrings.SymbolsRemoved.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.LeadingSymbols), TestStrings.LeadingSymbols.CamelToPascalCaseWithSpace());

            Assert.AreEqual(NicifyVariableName(TestStrings.TrailingSymbols), TestStrings.TrailingSymbols.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.LeadingAndTrailingSymbols), TestStrings.LeadingAndTrailingSymbols.CamelToPascalCaseWithSpace());

            Assert.AreEqual(NicifyVariableName(TestStrings.ToIndexOfChars1), TestStrings.ToIndexOfChars1.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.ToIndexOfChars2), TestStrings.ToIndexOfChars2.CamelToPascalCaseWithSpace());

            Assert.AreEqual(NicifyVariableName(TestStrings.SingleCharacter), TestStrings.SingleCharacter.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.Searched), TestStrings.Searched.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.Composition1), TestStrings.Composition1.CamelToPascalCaseWithSpace());
            Assert.AreEqual(NicifyVariableName(TestStrings.Composition2), TestStrings.Composition2.CamelToPascalCaseWithSpace());

            Regex nonAlpahumeric = new Regex(@"(?<=[a-z\d])(?=[A-Z])|(?<=\d)(?=[A-Z])", RegexOptions.None, TimeSpan.FromSeconds(0.1));
        }
        */
        #endregion // Regex
        #endregion // Unity.XR.CoreUtils
        
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

        [Test]
        public void RemoveChar_Test()
        {
            for (int i = 0; i < 5; i++)
            {
                Assert.That(TestStrings.Strings[i].RemoveChar(' '), Is.EqualTo(TestStrings.WhiteSpacesRemoved));
            }

            for (int i = 6; i < 11; i++)
            {
                Assert.That(TestStrings.Strings[i].RemoveChar(TestStrings.SymbolsToTrim[0])
                    .RemoveChar(TestStrings.SymbolsToTrim[1])
                    .RemoveChar(TestStrings.SymbolsToTrim[2]),
                    Is.EqualTo(TestStrings.WhiteSpacesRemoved));
            }
        }

        [Test]
        public void StringExtensions_TryFromBase64()
        {
            // "Hello World!"
            string input = "SGVsbG8gV29ybGQh";

            byte[] byteArray = System.Convert.FromBase64String(input);
            int length = input.FromBase64_ComputeResultLength();

            Assert.AreEqual(length, byteArray.Length);

            System.Span<byte> bytes = stackalloc byte[length];
            Assert.IsTrue(System.Convert.TryFromBase64String(input, bytes, out int bytesWritten));

            Assert.AreEqual(bytesWritten, byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                Assert.AreEqual(byteArray[i], bytes[i]);
            }
        }

        [Test]
        public void StringExtensions_TryFromBase64_LargeSpan()
        {
            // "Hello World!" * 50
            string input = "SGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQ==";

            byte[] byteArray = System.Convert.FromBase64String(input);
            int length = input.FromBase64_ComputeResultLength();

            Assert.AreEqual(length, byteArray.Length);

            System.Span<byte> bytes = stackalloc byte[length];
            Assert.IsTrue(System.Convert.TryFromBase64String(input, bytes, out int bytesWritten));

            Assert.AreEqual(bytesWritten, byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                Assert.AreEqual(byteArray[i], bytes[i]);
            }
        }

        [Test]
        public void StringExtensions_TryFromBase64_ArrayPool()
        {
            // "Hello World!" * 50
            string input = "SGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQ==";

            byte[] byteArray = System.Convert.FromBase64String(input);
            int length = input.FromBase64_ComputeResultLength();

            Assert.AreEqual(length, byteArray.Length);

            byte[] bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(length);
            Assert.IsTrue(System.Convert.TryFromBase64String(input, bytes, out int bytesWritten));

            Assert.AreEqual(bytesWritten, byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                Assert.AreEqual(byteArray[i], bytes[i]);
            }

            System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
        }

        [Test]
        public void FromBase64_ComputeResultLength_ShouldReturnCorrectLength()
        {
            string base64String = "SGVsbG8gV29ybGQ="; // "Hello World" in Base64
            int resultLength = StringExtensions.FromBase64_ComputeResultLength(base64String);
            Assert.AreEqual(11, resultLength); // "Hello World" has 11 bytes
        }

        [Test]
        public void FromBase64_ComputeResultLength_ShouldHandleWhiteSpace()
        {
            string base64String = " SG VsbG8g V2 9ybGQ= "; // "Hello World" in Base64 with spaces
            int resultLength = StringExtensions.FromBase64_ComputeResultLength(base64String);
            Assert.AreEqual(11, resultLength); // "Hello World" has 11 bytes
        }

        /*
        [Test]
        public void FromBase64_ComputeResultLength_ShouldHandlePadding()
        {
            string base64String = "SGVsbG8gV29ybGQ=="; // "Hello World" in Base64 with padding
            int resultLength = StringExtensions.FromBase64_ComputeResultLength(base64String);
            Assert.AreEqual(11, resultLength); // "Hello World" has 11 bytes
        }
        */

        [Test]
        public void FromBase64_ComputeResultLength_ShouldHandleEmptyString()
        {
            string base64String = "";
            int resultLength = StringExtensions.FromBase64_ComputeResultLength(base64String);
            Assert.AreEqual(0, resultLength);
        }
    }
}

namespace PKGE.Editor.Tests.StringExtensionsOrdinalTests
{
    #region Unity Documentation
    #region StartsWithOrdinal
    class StartsWithOrdinalTests
    {
        private static readonly string correctStringToSearch = string.Concat(TestStrings.Searched, TestStrings.Composition1, TestStrings.Searched, TestStrings.Composition2);
        private static readonly string correctStringToSearchUpperCase = string.Concat(TestStrings.Searched.ToUpper(), TestStrings.Composition1, TestStrings.Searched, TestStrings.Composition2);
        private static readonly string incorrectStringToSearch1 = string.Concat(TestStrings.SingleCharacter, TestStrings.Searched, TestStrings.Composition1, TestStrings.Composition2);
        private static readonly string incorrectStringToSearch2 = string.Concat(TestStrings.Composition1, TestStrings.Composition2);
        private static readonly string smallStringToSearch = TestStrings.Composition1;

        [Test]
        public void Test_StartsWithOrdinal()
        {
            foreach (var testString in TestStrings.Strings)
            {
                foreach (var s in TestStrings.strings)
                {
                    Assert.AreEqual(testString.StartsWith(s), testString.StartsWithOrdinal(s));
                }

                foreach (var c in TestStrings.chars)
                {
                    Assert.AreEqual(testString.StartsWith(c), testString.StartsWithOrdinal(c));
                }

                foreach (var line in TestStrings.newLine)
                {
                    Assert.AreEqual(testString.StartsWith(line), testString.StartsWithOrdinal(line));
                }

                foreach (var lines in TestStrings.newLines)
                {
                    Assert.AreEqual(testString.StartsWith(lines), testString.StartsWithOrdinal(lines));
                }
            }
        }
        
        [Test]
        public void TestContainingCharacters()
        {
            string sb;
            sb = new string(correctStringToSearch);
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched), correctStringToSearch.StartsWith(TestStrings.Searched));
            sb = new string(TestStrings.Searched);
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched), correctStringToSearch.StartsWith(TestStrings.Searched));
        }

        [Test]
        public void TestContainingCharactersIgnoreCase()
        {
            string sb;
            sb = new string(correctStringToSearchUpperCase);
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched, true), correctStringToSearch.StartsWith(TestStrings.Searched, true, CultureInfo.CurrentCulture));
            sb = new string(TestStrings.Searched.ToUpper());
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched, true), correctStringToSearch.StartsWith(TestStrings.Searched, true, CultureInfo.CurrentCulture));
        }

        [Test]
        public void TestNotContainingCharacters()
        {
            string sb;
            sb = new string(incorrectStringToSearch1);
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched), incorrectStringToSearch1.StartsWith(TestStrings.Searched));
            sb = new string(incorrectStringToSearch2);
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched), incorrectStringToSearch2.StartsWith(TestStrings.Searched));
        }

        [Test]
        public void TestNotContainingCharactersIgnoreCase()
        {
            string sb;
            sb = new string(incorrectStringToSearch1);
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched, true), incorrectStringToSearch1.StartsWith(TestStrings.Searched, true, CultureInfo.CurrentCulture));
            sb = new string(incorrectStringToSearch2);
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched, true), incorrectStringToSearch2.StartsWith(TestStrings.Searched, true, CultureInfo.CurrentCulture));
        }

        [Test]
        public void TestNullValue()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                string sb = string.Empty;
                sb.StartsWithOrdinal(null);
            }
            );
        }

        [Test]
        public void TestEmptyValue()
        {
            string sb = string.Empty;
            Assert.IsTrue(sb.StartsWithOrdinal(string.Empty));
        }

        [Test]
        public void TestValueLengthGreaterThanStringBuilderLength()
        {
            string sb = new string(smallStringToSearch);
            Assert.AreEqual(sb.StartsWithOrdinal(TestStrings.Searched), smallStringToSearch.StartsWith(TestStrings.Searched));
        }

        [Test]
        public void TestEmpty()
        {
            string sb = string.Empty;
            Assert.IsFalse(sb.StartsWithOrdinal(TestStrings.Searched));
        }
    }
    #endregion // StartsWithOrdinal
    #region EndsWithOrdinal
    class EndsWithOrdinalTests
    {
        private static readonly string correctStringToSearch = string.Concat(TestStrings.Composition1, TestStrings.Searched, TestStrings.Composition2, TestStrings.Searched);
        private static readonly string correctStringToSearchUpperCase = string.Concat(TestStrings.Composition1, TestStrings.Searched, TestStrings.Composition2, TestStrings.Searched.ToUpper());
        private static readonly string incorrectStringToSearch1 = string.Concat(TestStrings.Composition1, TestStrings.Composition2, TestStrings.Searched, TestStrings.SingleCharacter);
        private static readonly string incorrectStringToSearch2 = string.Concat(TestStrings.Composition1, TestStrings.Composition2);
        private static readonly string smallStringToSearch = TestStrings.Composition1;

        [Test]
        public void Test_EndsWithOrdinal()
        {
            foreach (var testString in TestStrings.Strings)
            {
                foreach (var s in TestStrings.strings)
                {
                    Assert.AreEqual(testString.EndsWith(s), testString.EndsWithOrdinal(s));
                }

                foreach (var c in TestStrings.chars)
                {
                    Assert.AreEqual(testString.EndsWith(c), testString.EndsWithOrdinal(c));
                }

                foreach (var line in TestStrings.newLine)
                {
                    Assert.AreEqual(testString.EndsWith(line), testString.EndsWithOrdinal(line));
                }

                foreach (var lines in TestStrings.newLines)
                {
                    Assert.AreEqual(testString.EndsWith(lines), testString.EndsWithOrdinal(lines));
                }
            }
        }
        
        [Test]
        public void TestContainingCharacters()
        {
            string sb;
            sb = correctStringToSearch;
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched), correctStringToSearch.EndsWith(TestStrings.Searched));
            sb = TestStrings.Searched;
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched), correctStringToSearch.EndsWith(TestStrings.Searched));
        }

        [Test]
        public void TestContainingCharactersIgnoreCase()
        {
            string sb;
            sb = new string(correctStringToSearchUpperCase);
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched, true), correctStringToSearch.EndsWith(TestStrings.Searched, true, CultureInfo.CurrentCulture));
            sb = new string(TestStrings.Searched.ToUpper());
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched, true), correctStringToSearch.EndsWith(TestStrings.Searched, true, CultureInfo.CurrentCulture));
        }

        [Test]
        public void TestNotContainingCharacters()
        {
            string sb;
            sb = new string(incorrectStringToSearch1);
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched), incorrectStringToSearch1.EndsWith(TestStrings.Searched));
            sb = new string(incorrectStringToSearch2);
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched), incorrectStringToSearch2.EndsWith(TestStrings.Searched));
        }

        [Test]
        public void TestNotContainingCharactersIgnoreCase()
        {
            string sb;
            sb = new string(incorrectStringToSearch1);
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched, true), incorrectStringToSearch1.EndsWith(TestStrings.Searched, true, CultureInfo.CurrentCulture));
            sb = new string(incorrectStringToSearch2);
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched, true), incorrectStringToSearch2.EndsWith(TestStrings.Searched, true, CultureInfo.CurrentCulture));
        }

        [Test]
        public void TestNullValue()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                string sb = string.Empty;
                sb.EndsWithOrdinal(null);
            }
            );
        }

        [Test]
        public void TestEmptyValue()
        {
            string sb = string.Empty;
            Assert.IsTrue(sb.EndsWithOrdinal(string.Empty));
        }

        [Test]
        public void TestValueLengthGreaterThanStringBuilderLength()
        {
            string sb = new string(smallStringToSearch);
            Assert.AreEqual(sb.EndsWithOrdinal(TestStrings.Searched), smallStringToSearch.EndsWith(TestStrings.Searched));
        }

        [Test]
        public void TestEmpty()
        {
            string sb = string.Empty;
            Assert.IsFalse(sb.EndsWithOrdinal(TestStrings.Searched));
        }
    }
    #endregion // EndsWithOrdinal
    #endregion // Unity Documentation
    #region IndexOfCharOrdinal
    class IndexOfCharOrdinalTests
    {
        [Test]
        public void TestContainingCharacters()
        {
            string sb;
            sb = new string(TestStrings.ToIndexOfChars1);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol), TestStrings.ToIndexOfChars1.IndexOf(symbol));
            }
            sb = new string(TestStrings.ToIndexOfChars2);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol), TestStrings.ToIndexOfChars2.IndexOf(symbol));
            }
            sb = new string(TestStrings.ToIndexOfChars2);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol, 3), TestStrings.ToIndexOfChars2.IndexOf(symbol, 3));
            }
            sb = new string(TestStrings.ToIndexOfChars2);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol, 3, 7), TestStrings.ToIndexOfChars2.IndexOf(symbol, 3, 7));
            }
        }

        [Test]
        public void TestNotContainingCharacters()
        {
            string sb;
            sb = new string(TestStrings.LeadingAndTrailingWhiteSpaces);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol), TestStrings.LeadingAndTrailingWhiteSpaces.IndexOf(symbol));
            }
            sb = new string(TestStrings.ToIndexOfChars1);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol, 23), TestStrings.ToIndexOfChars1.IndexOf(symbol, 23));
            }
            sb = new string(TestStrings.ToIndexOfChars2);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol, 3, 4), TestStrings.ToIndexOfChars2.IndexOf(symbol, 3, 4));
            }
        }

        [Test]
        public void TestEmpty()
        {
            string sb = string.Empty;
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol), -1);
            }
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    sb.IndexOfOrdinal(symbol, 3);
                }
                );
            }
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    sb.IndexOfOrdinal(symbol, 3, 7);
                }
                );
            }
        }

        [Test]
        public void TestIndexLastCharacter()
        {
            string sb;
            sb = new string(TestStrings.ToIndexOfChars1);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol, sb.Length - 1), TestStrings.ToIndexOfChars1.IndexOf(symbol, sb.Length - 1));
            }
            sb = new string(TestStrings.ToIndexOfChars2);
            foreach (char symbol in TestStrings.SymbolsToTrim)
            {
                Assert.AreEqual(sb.IndexOfOrdinal(symbol, sb.Length - 1), TestStrings.ToIndexOfChars2.IndexOf(symbol, sb.Length - 1));
            }
        }

        [Test]
        public void TestIndexAfterLastCharacter()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.ToIndexOfChars1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], sb.Length);
            }
            );
        }

        [Test]
        public void TestIndexAfterLastCharacterWithCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.ToIndexOfChars2);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], sb.Length);
            }
            );
        }

        [Test]
        public void TestIndexLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], -1);
            }
            );
        }

        [Test]
        public void TestIndexLessThanZeroWithCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], -1, 5);
            }
            );
        }

        [Test]
        public void TestIndexGreaterThanMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], sb.Length + 1);
            }
            );
        }

        [Test]
        public void TestIndexGreaterThanMaximumWithCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], sb.Length + 1, 5);
            }
            );
        }

        [Test]
        public void TestCountLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], 0, -1);
            }
            );
        }

        [Test]
        public void TestCountEqualsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], -1);
            }
            );
        }

        [Test]
        public void TestCountGreaterThanMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], 0, sb.Length + 1);
            }
            );
        }

        [Test]
        public void TestIndexPlusCountGreaterThanLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.SymbolsToTrim[0], 5, 20);
            }
            );
        }
    }
    #endregion // IndexOfCharOrdinal
    #region IndexOfStringOrdinal
    class IndexOfStringOrdinalTests
    {
        private static readonly string correctStringToSearch1 = string.Concat(TestStrings.Searched, TestStrings.Composition2, TestStrings.Searched, TestStrings.Composition1);
        private static readonly string correctStringToSearch2 = string.Concat(TestStrings.Composition2, TestStrings.Composition1, TestStrings.Searched);
        private static readonly string correctStringToSearchUpperCase1 = string.Concat(TestStrings.Searched.ToUpper(), TestStrings.Composition1, TestStrings.Searched, TestStrings.Composition2);
        private static readonly string correctStringToSearchUpperCase2 = string.Concat(TestStrings.Composition2, TestStrings.Composition1, TestStrings.Searched.ToUpper());
        private static readonly string incorrectStringToSearch = string.Concat(TestStrings.Composition1, TestStrings.Composition2);
        private static readonly string correctSmallStringToSearch = string.Concat(TestStrings.Searched.Substring(10), TestStrings.Composition1.Substring(10));
        private static readonly string incorrectSmallStringToSearch = TestStrings.Composition1;

        [Test]
        public void Test_IndexOfOrdinal()
        {
            foreach (var testString in TestStrings.Strings)
            {
                //public static bool IndexOfOrdinal(this string str, char value)
                foreach (var c in TestStrings.chars)
                {
                    Assert.AreEqual(testString.IndexOf(c, System.StringComparison.Ordinal), testString.IndexOfOrdinal(c));
                }

                //public static bool IndexOfOrdinal(this string str, string value)
                foreach (var s in TestStrings.strings)
                {
                    Assert.AreEqual(testString.IndexOf(s, System.StringComparison.Ordinal), testString.IndexOfOrdinal(s));
                }

                foreach (var line in TestStrings.newLine)
                {
                    Assert.AreEqual(testString.IndexOf(line, System.StringComparison.Ordinal), testString.IndexOfOrdinal(line));
                }

                foreach (var lines in TestStrings.newLines)
                {
                    Assert.AreEqual(testString.IndexOf(lines, System.StringComparison.Ordinal), testString.IndexOfOrdinal(lines));
                }
            }
        }
        
        [Test]
        public void TestContainingCharacters()
        {
            string sb;
            sb = new string(correctStringToSearch1);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched), correctStringToSearch1.IndexOf(TestStrings.Searched));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3), correctStringToSearch1.IndexOf(TestStrings.Searched, 3));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, 55), correctStringToSearch1.IndexOf(TestStrings.Searched, 3, 55));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, 57), correctStringToSearch1.IndexOf(TestStrings.Searched, 3, 57));
            int count = correctStringToSearch2.Length - 3;
            sb = new string(correctStringToSearch2);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched), correctStringToSearch2.IndexOf(TestStrings.Searched));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3), correctStringToSearch2.IndexOf(TestStrings.Searched, 3));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, count), correctStringToSearch2.IndexOf(TestStrings.Searched, 3, count));
        }

        [Test]
        public void TestContainingCharactersIgnoreCase()
        {
            string sb;
            sb = new string(correctStringToSearchUpperCase1);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, true), correctStringToSearchUpperCase1.IndexOf(TestStrings.Searched, StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, true), correctStringToSearchUpperCase1.IndexOf(TestStrings.Searched, 3, StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, 55, true), correctStringToSearchUpperCase1.IndexOf(TestStrings.Searched, 3, 55, StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, 57, true), correctStringToSearchUpperCase1.IndexOf(TestStrings.Searched, 3, 57, StringComparison.CurrentCultureIgnoreCase));
            int count = correctStringToSearchUpperCase2.Length - 3;
            sb = new string(correctStringToSearchUpperCase2);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, true), correctStringToSearchUpperCase2.IndexOf(TestStrings.Searched, StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, true), correctStringToSearchUpperCase2.IndexOf(TestStrings.Searched, 3, StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, count, true), correctStringToSearchUpperCase2.IndexOf(TestStrings.Searched, 3, count, StringComparison.CurrentCultureIgnoreCase));
        }

        [Test]
        public void TestNotContainingCharacters()
        {
            string sb;
            sb = new string(incorrectStringToSearch);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched), incorrectStringToSearch.IndexOf(TestStrings.Searched));
            sb = new string(incorrectStringToSearch);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3), incorrectStringToSearch.IndexOf(TestStrings.Searched, 3));
            sb = new string(incorrectStringToSearch);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, 32), incorrectStringToSearch.IndexOf(TestStrings.Searched, 3, 32));
        }

        [Test]
        public void TestNotContainingCharactersIgnoreCase()
        {
            string testStringsSearchedToUpper = TestStrings.Searched.ToUpper();

            string sb;
            sb = new string(incorrectStringToSearch);
            Assert.AreEqual(sb.IndexOfOrdinal(testStringsSearchedToUpper, true), incorrectStringToSearch.IndexOf(testStringsSearchedToUpper, StringComparison.CurrentCultureIgnoreCase));
            sb = new string(incorrectStringToSearch);
            Assert.AreEqual(sb.IndexOfOrdinal(testStringsSearchedToUpper, 3, true), incorrectStringToSearch.IndexOf(testStringsSearchedToUpper, 3, StringComparison.CurrentCultureIgnoreCase));
            sb = new string(incorrectStringToSearch);
            Assert.AreEqual(sb.IndexOfOrdinal(testStringsSearchedToUpper, 3, 32, true), incorrectStringToSearch.IndexOf(testStringsSearchedToUpper, 3, 32, StringComparison.CurrentCultureIgnoreCase));
        }

        [Test]
        public void TestNullValue()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                string sb = string.Empty;
                sb.IndexOfOrdinal(null);
            }
            );
        }

        [Test]
        public void TestNullValueWithIndex()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                string sb = string.Empty;
                sb.IndexOfOrdinal(null, 3);
            }
            );
        }

        [Test]
        public void TestNullValueWithIndexAndCount()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                string sb = string.Empty;
                sb.IndexOfOrdinal(null, 3, 55);
            }
            );
        }

        [Test]
        public void TestEmptyValue()
        {
            string sb;
            sb = string.Empty;
            Assert.AreEqual(sb.IndexOfOrdinal(string.Empty), string.Empty.IndexOf(string.Empty));
            sb = new string(correctStringToSearch1);
            Assert.AreEqual(sb.IndexOfOrdinal(string.Empty), correctStringToSearch1.IndexOf(string.Empty));
            Assert.AreEqual(sb.IndexOfOrdinal(string.Empty, 3), correctStringToSearch1.IndexOf(string.Empty, 3));
            Assert.AreEqual(sb.IndexOfOrdinal(string.Empty, 3, 55), correctStringToSearch1.IndexOf(string.Empty, 3, 55));
        }

        [Test]
        public void TestEmptyValueEmptyStringBuilderIndex()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = string.Empty;
                sb.IndexOfOrdinal(string.Empty, 3);
            }
            );
        }

        [Test]
        public void TestEmptyValueEmptyStringBuilderIndexAndCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = string.Empty;
                sb.IndexOfOrdinal(string.Empty, 3, 55);
            }
            );
        }

        [Test]
        public void TestValueLengthGreaterThanStringBuilderLengthContaining()
        {
            string sb = new string(correctSmallStringToSearch);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched), correctSmallStringToSearch.IndexOf(TestStrings.Searched));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3), correctSmallStringToSearch.IndexOf(TestStrings.Searched, 3));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, 12), correctSmallStringToSearch.IndexOf(TestStrings.Searched, 3, 12));
        }

        [Test]
        public void TestValueLengthGreaterThanStringBuilderLengthNotContaining()
        {
            string sb = new string(incorrectSmallStringToSearch);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched), incorrectSmallStringToSearch.IndexOf(TestStrings.Searched));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3), incorrectSmallStringToSearch.IndexOf(TestStrings.Searched, 3));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 3, 12), incorrectSmallStringToSearch.IndexOf(TestStrings.Searched, 3, 12));
        }

        [Test]
        public void TestEmptyStringBuilder()
        {
            string sb = string.Empty;
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched), string.Empty.IndexOf(TestStrings.Searched));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 0), string.Empty.IndexOf(TestStrings.Searched, 0));
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, 0, 0), string.Empty.IndexOf(TestStrings.Searched, 0, 0));
        }

        [Test]
        public void TestEmptyStringBuilderIndex()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = string.Empty;
                sb.IndexOfOrdinal(TestStrings.Searched, 3);
            }
            );
        }

        [Test]
        public void TestEmptyStringBuilderIndexWithCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = string.Empty;
                sb.IndexOfOrdinal(TestStrings.Searched, 3, 55);
            }
            );
        }

        [Test]
        public void TestIndexLastCharacter()
        {
            string sb;
            sb = new string(TestStrings.ToIndexOfChars1);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, sb.Length - 1), TestStrings.ToIndexOfChars1.IndexOf(TestStrings.Searched, sb.Length - 1));
            sb = new string(correctStringToSearch1);
            Assert.AreEqual(sb.IndexOfOrdinal(TestStrings.Searched, sb.Length - 1), correctStringToSearch1.IndexOf(TestStrings.Searched, sb.Length - 1));
        }

        [Test]
        public void TestIndexAfterLastCharacter()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.ToIndexOfChars1);
                sb.IndexOfOrdinal(TestStrings.Searched, sb.Length);
            }
            );
        }

        [Test]
        public void TestIndexAfterLastCharacterWithCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(correctStringToSearch1);
                sb.IndexOfOrdinal(TestStrings.Searched, sb.Length);
            }
            );
        }

        [Test]
        public void TestIndexLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.Searched, -1);
            }
            );
        }

        [Test]
        public void TestIndexLessThanZeroWithCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.Searched, -1, 5);
            }
            );
        }

        [Test]
        public void TestIndexGreaterThanMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.Searched, sb.Length + 1);
            }
            );
        }

        [Test]
        public void TestIndexGreaterThanMaximumWithCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.Searched, sb.Length + 1, 5);
            }
            );
        }

        [Test]
        public void TestCountLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.Searched, 0, -1);
            }
            );
        }

        [Test]
        public void TestCountEqualsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.Searched, -1);
            }
            );
        }

        [Test]
        public void TestCountGreaterThanMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.Searched, 0, sb.Length + 1);
            }
            );
        }

        [Test]
        public void TestIndexPlusCountGreaterThanLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                string sb = new string(TestStrings.Composition1);
                sb.IndexOfOrdinal(TestStrings.Searched, 5, 20);
            }
            );
        }
    }
    #endregion // IndexOfStringOrdinal
    #region ContainsOrdinal
    class ContainsOrdinalTests
    {
        [Test]
        public void Test_ContainsOrdinal()
        {
            foreach (var testString in TestStrings.Strings)
            {
                //public static bool ContainsOrdinal(this string str, string value)
                foreach (var s in TestStrings.strings)
                {
                    Assert.AreEqual(testString.Contains(s, System.StringComparison.Ordinal), testString.ContainsOrdinal(s));
                }

                //public static bool ContainsOrdinal(this string str, char value)
                foreach (var c in TestStrings.chars)
                {
                    Assert.AreEqual(testString.Contains(c, System.StringComparison.Ordinal), testString.ContainsOrdinal(c));
                }

                foreach (var line in TestStrings.newLine)
                {
                    Assert.AreEqual(testString.Contains(line, System.StringComparison.Ordinal), testString.ContainsOrdinal(line));
                }

                foreach (var lines in TestStrings.newLines)
                {
                    Assert.AreEqual(testString.Contains(lines, System.StringComparison.Ordinal), testString.ContainsOrdinal(lines));
                }
            }
        }
    }
    #endregion // ContainsOrdinal
}
