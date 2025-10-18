using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace PKGE.Tests
{
    public class EnumUtilitiesTests
    {
        /// <summary>
        /// Sample enum with and without DescriptionAttribute
        /// </summary>
        private enum SampleEnum
        {
            [System.ComponentModel.Description("First Value")]
            First = 1,

            [System.ComponentModel.Description("Second Value")]
            Second = 2,

            Third = 3 // No description
        }

        private enum EmptyEnum { }

        private enum DuplicateDescriptionEnum
        {
            [System.ComponentModel.Description("Duplicate")]
            A = 1,

            [System.ComponentModel.Description("Duplicate")]
            B = 2
        }

        #region GetDescription
        [Test]
        public static void GetDescription_WithDescription_ReturnsCorrectDescription()
        {
            var result = SampleEnum.First.GetDescription();
            Assert.AreEqual("First Value", result);
        }

        [Test]
        public static void GetDescription_WithoutDescription_ReturnsEmptyString()
        {
            var result = SampleEnum.Third.GetDescription();
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public static void GetDescription_NonMatchingValue_ReturnsEmptyString()
        {
            var fakeEnum = (SampleEnum)999;
            var result = fakeEnum.GetDescription();
            Assert.AreEqual(string.Empty, result);
        }
        #endregion // GetDescription

        #region GetAllDescriptions
        [Test]
        public static void GetAllDescriptions_ReturnsOnlyDescribedValues()
        {
            using var _0 = UnityEngine.Pool.ListPool<string>.Get(out var descriptions);
            EnumUtilities.GetAllDescriptions<SampleEnum>(descriptions);
            CollectionAssert.AreEquivalent(new List<string> { "First Value", "Second Value" }, descriptions);
        }

        [Test]
        public static void GetAllDescriptions_EmptyEnum_ReturnsEmptyList()
        {
            using var _0 = UnityEngine.Pool.ListPool<string>.Get(out var descriptions);
            EnumUtilities.GetAllDescriptions<EmptyEnum>(descriptions);
            Assert.IsEmpty(descriptions);
        }

        [Test]
        public static void GetAllDescriptions_DuplicateDescriptions_ReturnsAllInstances()
        {
            using var _0 = UnityEngine.Pool.ListPool<string>.Get(out var descriptions);
            EnumUtilities.GetAllDescriptions<DuplicateDescriptionEnum>(descriptions);
            CollectionAssert.AreEqual(new List<string> { "Duplicate", "Duplicate" }, descriptions);
        }
        #endregion // GetAllDescriptions

        #region TypeFromName
        [Test]
        public static void TypeFromName_ValidDescription_ReturnsCorrectEnum()
        {
            var result = EnumUtilities.TypeFromName<SampleEnum>("Second Value");
            Assert.AreEqual(SampleEnum.Second, result);
        }

        [Test]
        public static void TypeFromName_InvalidDescription_ReturnsDefault()
        {
            var result = EnumUtilities.TypeFromName<SampleEnum>("Nonexistent");
            Assert.AreEqual(default(SampleEnum), result);
        }

        [Test]
        public static void TypeFromName_DuplicateDescription_ReturnsFirstMatch()
        {
            var result = EnumUtilities.TypeFromName<DuplicateDescriptionEnum>("Duplicate");
            Assert.AreEqual(DuplicateDescriptionEnum.A, result); // First match
        }

        [Test]
        public static void TypeFromName_EmptyEnum_ReturnsDefault()
        {
            var result = EnumUtilities.TypeFromName<EmptyEnum>("Anything");
            Assert.AreEqual(default(EmptyEnum), result);
        }
        #endregion // TypeFromName

        [Test]
        public static void GetDescription_NullableEnum_ReturnsCorrectDescription()
        {
            SampleEnum? nullableEnum = SampleEnum.First;
            var result = nullableEnum.Value.GetDescription();
            Assert.AreEqual("First Value", result);
        }
    }

    public static class EnumExtensionsTests
    {
        /// <summary>
        /// Sample enum with InspectorName attributes
        /// </summary>
        private enum DisplayEnum
        {
            [InspectorName("Alpha Display")]
            Alpha = 0,

            [InspectorName("Beta Display")]
            Beta = 1,

            Gamma = 2 // No InspectorName
        }

        private enum EmptyEnum { }

        private enum DuplicateDisplayEnum
        {
            [InspectorName("Duplicate")]
            A = 1,

            [InspectorName("Duplicate")]
            B = 2
        }

        #region GetDisplayName
        [Test]
        public static void GetDisplayName_WithInspectorName_ReturnsCorrectDisplayName()
        {
            var result = DisplayEnum.Alpha.GetDisplayName();
            Assert.AreEqual("Alpha Display", result);
        }

        [Test]
        public static void GetDisplayName_WithoutInspectorName_ReturnsEnumName()
        {
            var result = DisplayEnum.Gamma.GetDisplayName();
            Assert.AreEqual("Gamma", result);
        }

        [Test]
        public static void GetDisplayName_DuplicateDisplayNames_ReturnsFirstMatch()
        {
            var result = DuplicateDisplayEnum.A.GetDisplayName();
            Assert.AreEqual("Duplicate", result);
        }

        [Test]
        public static void GetDisplayName_EmptyEnum_ReturnsEnumName()
        {
            var result = ((EmptyEnum)0).GetDisplayName();
            Assert.AreEqual("0", result); // Enum.ToString() fallback
        }

        [Test]
        public static void GetDisplayName_InvalidEnumValue_ReturnsNumericFallback()
        {
            var result = ((DisplayEnum)999).GetDisplayName();
            Assert.AreEqual("999", result); // fallback to ToString()
        }

        [Test]
        public static void GetDisplayName_NullableEnumWithInspectorName_ReturnsDisplayName()
        {
            DisplayEnum? nullable = DisplayEnum.Beta;
            var result = nullable.Value.GetDisplayName();
            Assert.AreEqual("Beta Display", result);
        }

        [Test]
        public static void GetDisplayName_NullableEnumWithoutInspectorName_ReturnsEnumName()
        {
            DisplayEnum? nullable = DisplayEnum.Gamma;
            var result = nullable.Value.GetDisplayName();
            Assert.AreEqual("Gamma", result);
        }
        enum WeirdEnum
        {
            [InspectorName("Weird@Name#1")]
            Strange = 1
        }

        [Test]
        public static void GetDisplayName_EnumWithSpecialCharacters_ReturnsCorrectDisplayName()
        {
            var result = WeirdEnum.Strange.GetDisplayName();
            Assert.AreEqual("Weird@Name#1", result);
        }
        enum BlankDisplayEnum
        {
            [InspectorName("")]
            Blank = 0
        }

        [Test]
        public static void GetDisplayName_EnumWithEmptyInspectorName_ReturnsEmptyString()
        {
            var result = BlankDisplayEnum.Blank.GetDisplayName();
            Assert.AreEqual("", result);
        }
        enum WhitespaceDisplayEnum
        {
            [InspectorName("   ")]
            Spacey = 0
        }

        [Test]
        public static void GetDisplayName_EnumWithWhitespaceInspectorName_ReturnsWhitespace()
        {
            var result = WhitespaceDisplayEnum.Spacey.GetDisplayName();
            Assert.AreEqual("   ", result);
        }
        #endregion // GetDisplayName
    }

    public static class EnumValuesTests
    {
        private enum BasicEnum
        {
            Zero = 0,
            One = 1,
            Two = 2
        }

        private enum NegativeEnum
        {
            MinusTwo = -2,
            MinusOne = -1,
            Zero = 0
        }

        private enum EmptyEnum { }

        private enum NonSequentialEnum
        {
            A = 1,
            B = 10,
            C = 100
        }

        private enum DuplicateValueEnum
        {
            A = 1,
            B = 1,
            C = 2
        }

        [Test]
        public static void Values_ReturnsAllEnumValues()
        {
            var values = EnumValues<BasicEnum>.Values;
            CollectionAssert.AreEqual(new[] { BasicEnum.Zero, BasicEnum.One, BasicEnum.Two }, values);
        }

        [Test]
        public static void Values_ReturnsEmptyArrayForEmptyEnum()
        {
            var values = EnumValues<EmptyEnum>.Values;
            Assert.IsNotNull(values);
            Assert.IsEmpty(values);
        }

        [Test]
        public static void Values_ReturnsNegativeEnumValuesCorrectly()
        {
            var values = EnumValues<NegativeEnum>.SortedValues;
            CollectionAssert.AreEqual(new[] { NegativeEnum.MinusTwo, NegativeEnum.MinusOne, NegativeEnum.Zero }, values);
        }

        [Test]
        public static void Values_ReturnsNonSequentialEnumValuesCorrectly()
        {
            var values = EnumValues<NonSequentialEnum>.Values;
            CollectionAssert.AreEqual(new[] { NonSequentialEnum.A, NonSequentialEnum.B, NonSequentialEnum.C }, values);
        }

        [Test]
        public static void Values_ReturnsDuplicateValueEnumValuesCorrectly()
        {
            var values = EnumValues<DuplicateValueEnum>.Values;
            CollectionAssert.AreEqual(new[] { DuplicateValueEnum.A, DuplicateValueEnum.B, DuplicateValueEnum.C }, values);
        }

        #region Caching
        [Test]
        public static void Values_IsCachedAndConsistent()
        {
            var first = EnumValues<BasicEnum>.Values;
            var second = EnumValues<BasicEnum>.Values;
            Assert.AreSame(first, second); // Reference equality confirms caching
        }
        #endregion // Caching
    }
}