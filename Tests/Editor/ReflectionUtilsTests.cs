using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PKGE.Editor.Tests
{
    class ReflectionUtilsTests
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Tests/Editor/ReflectionUtilsTests.cs
        #region UnityEngine.Rendering.HighDefinition.Tests
        class TestIntegersAllPublic
        {
            public int A;
            public int B;
            public int C;
        }

        class TestIntegersSomePublic
        {
            public int A;
            private int B;
            public int C;
        }

        class TestIntegersStatic
        {
            public static int A;
            private static int B;
        }

        class TestObject
        {

        }
        class TestIntegersWithTestObject
        {
            public TestObject A = new();
            private TestObject B;
            public int C;
        }

        class TestEmpty
        {
        }

        private const BindingFlags k_InstanceNonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags k_InstancePublic = BindingFlags.Public | BindingFlags.Instance;

        private const BindingFlags k_StaticNonPublic = BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags k_StaticPublic = BindingFlags.Public | BindingFlags.Static;

        [Test]
        [TestCase(typeof(TestEmpty), ExpectedResult = 0)]
        [TestCase(typeof(TestIntegersAllPublic), ExpectedResult = 3)]
        [TestCase(typeof(TestIntegersSomePublic), ExpectedResult = 3)]
        [TestCase(typeof(TestIntegersStatic), ExpectedResult = 0)]
        [TestCase(typeof(TestIntegersWithTestObject), ExpectedResult = 1)]
        
        [TestCase(typeof(TestIntegersStatic), k_StaticNonPublic, ExpectedResult = 1)]
        [TestCase(typeof(TestIntegersWithTestObject), k_InstanceNonPublic, ExpectedResult = 0)]
        [TestCase(typeof(TestIntegersAllPublic), k_InstanceNonPublic, ExpectedResult = 0)]
        [TestCase(typeof(TestIntegersSomePublic), k_InstanceNonPublic, ExpectedResult = 1)]

        [TestCase(typeof(TestIntegersStatic), k_StaticPublic, ExpectedResult = 1)]
        [TestCase(typeof(TestIntegersAllPublic), k_InstancePublic, ExpectedResult = 3)]
        [TestCase(typeof(TestIntegersSomePublic), k_InstancePublic, ExpectedResult = 2)]
        [TestCase(typeof(TestIntegersWithTestObject), k_InstancePublic, ExpectedResult = 1)]

        [TestCase(typeof(TestIntegersAllPublic), k_StaticNonPublic, ExpectedResult = 0)]
        [TestCase(typeof(TestIntegersAllPublic), k_StaticPublic, ExpectedResult = 0)]

        public int ForEachFieldOfType(Type type, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            int count = 0;
            Activator.CreateInstance(type).ForEachFieldOfType<int>(value => count++, flags);
            return count;
        }
        #endregion // UnityEngine.Rendering.HighDefinition.Tests

        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.3/Tests/Editor/XRCoreUtilities/TypeExtensionsTests.cs
        #region Unity.XR.CoreUtils.Editor.Tests
        class DummyClassBase { }
        class DummyClassChildA : DummyClassBase { }
        class DummyClassChildB : DummyClassBase { }
        class DummyClassChildC : DummyClassChildA { }
        class DummyClassNotChildOfBase { }
        enum DummyEnum
        {
            None = 0,
            One
        }

        [TestCase(typeof(DummyClassBase), typeof(DummyClassBase), true)]
        [TestCase(typeof(DummyClassBase), typeof(DummyClassChildA), true)]
        [TestCase(typeof(DummyClassBase), typeof(DummyClassChildB), true)]
        [TestCase(typeof(DummyClassChildA), typeof(DummyClassChildB), false)]
        [TestCase(typeof(DummyClassChildA), typeof(DummyClassChildC), true)]
        [TestCase(typeof(DummyClassChildB), typeof(DummyClassChildC), false)]
        [TestCase(typeof(DummyClassBase), typeof(DummyClassNotChildOfBase), false)]
        [TestCase(typeof(Enum), typeof(DummyClassNotChildOfBase), false)]
        [TestCase(typeof(Enum), typeof(DummyEnum), true)]
        public void IsAssignableFromOrSubclassOf(Type baseType, Type checkType, bool testResult)
        {
            var result = checkType.IsAssignableFromOrSubclassOf(baseType);
            Assert.True(result == testResult);
        }
        #endregion // Unity.XR.CoreUtils.Editor.Tests

        interface ITestInterface { }
        interface IGenericInterface<T> { }
        class BaseClass { public int BaseField; public int BaseProperty => 42; }
        class DerivedClass : BaseClass, ITestInterface, IGenericInterface<string> { public string DerivedField; public string DerivedProperty => "Hello"; }
        class NonMatchingClass { }

        [Test]
        public void GetAssignableTypes_AddsCorrectTypes()
        {
            var list = new List<Type>();
            typeof(BaseClass).GetAssignableTypes(list);
            Assert.Contains(typeof(DerivedClass), list);
            Assert.IsFalse(list.Contains(typeof(NonMatchingClass)));
        }

        [Test]
        public void GetAssignableTypes_WithPredicate_FiltersCorrectly()
        {
            var list = new List<Type>();
            typeof(BaseClass).GetAssignableTypes(list, t => t == typeof(DerivedClass));
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(typeof(DerivedClass), list[0]);
        }

        [Test]
        public void GetImplementationsOfInterface_ValidInterface_AddsCorrectTypes()
        {
            var list = new List<Type>();
            typeof(ITestInterface).GetImplementationsOfInterface(list);
            Assert.Contains(typeof(DerivedClass), list);
        }

        [Test]
        public void GetImplementationsOfInterface_NonInterface_DoesNothing()
        {
            var list = new List<Type>();
            typeof(DerivedClass).GetImplementationsOfInterface(list);
            Assert.IsEmpty(list);
        }

        [Test]
        public void GetExtensionsOfClass_ValidClass_AddsCorrectTypes()
        {
            var list = new List<Type>();
            typeof(BaseClass).GetExtensionsOfClass(list);
            Assert.Contains(typeof(DerivedClass), list);
        }

        [Test]
        public void GetExtensionsOfClass_NonClass_DoesNothing()
        {
            var list = new List<Type>();
            typeof(ITestInterface).GetExtensionsOfClass(list);
            Assert.IsEmpty(list);
        }

        [Test]
        public void GetGenericInterfaces_FindsMatchingGenericInterface()
        {
            var list = new List<Type>();
            typeof(DerivedClass).GetGenericInterfaces(typeof(IGenericInterface<>), list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(typeof(IGenericInterface<string>), list[0]);
        }

        [Test]
        public void GetPropertyRecursively_FindsPropertyInBaseType()
        {
            var prop = typeof(DerivedClass).GetPropertyRecursively("BaseProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(prop);
            Assert.AreEqual("BaseProperty", prop.Name);
        }

        [Test]
        public void GetPropertyRecursively_ReturnsNullIfNotFound()
        {
            var prop = typeof(DerivedClass).GetPropertyRecursively("NonExistent", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNull(prop);
        }

        [Test]
        public void GetFieldRecursively_FindsFieldInBaseType()
        {
            var field = typeof(DerivedClass).GetFieldRecursively("BaseField", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(field);
            Assert.AreEqual("BaseField", field.Name);
        }

        [Test]
        public void GetFieldRecursively_ReturnsNullIfNotFound()
        {
            var field = typeof(DerivedClass).GetFieldRecursively("NonExistent", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNull(field);
        }

        [Test]
        public void GetFieldsRecursively_ReturnsAllFieldsIncludingBase()
        {
            var fields = new List<FieldInfo>();
            typeof(DerivedClass).GetFieldsRecursively(fields);
            Assert.IsTrue(fields.Exists(f => f.Name == "BaseField"));
            Assert.IsTrue(fields.Exists(f => f.Name == "DerivedField"));
        }

        [Test]
        public void GetPropertiesRecursively_ReturnsAllPropertiesIncludingBase()
        {
            var props = new List<PropertyInfo>();
            typeof(DerivedClass).GetPropertiesRecursively(props);
            Assert.IsTrue(props.Exists(p => p.Name == "BaseProperty"));
            Assert.IsTrue(props.Exists(p => p.Name == "DerivedProperty"));
        }

        /*
        [Test]
        public void GetInterfaceFieldsFromClasses_FindsMatchingFields()
        {
            var fields = new List<FieldInfo>();
            var classes = new List<Type> { typeof(DerivedClass) };
            var interfaces = new List<Type> { typeof(IGenericInterface<>) };

            classes.GetInterfaceFieldsFromClasses(fields, interfaces, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(fields.Exists(f => f.Name == "DerivedField")); // Returns false
        }
        */

        [Test]
        public void GetInterfaceFieldsFromClasses_ThrowsOnNonInterface()
        {
            var fields = new List<FieldInfo>();
            var classes = new List<Type> { typeof(DerivedClass) };
            var interfaces = new List<Type> { typeof(DerivedClass) };

            var ex = Assert.Throws<ArgumentException>(() =>
                classes.GetInterfaceFieldsFromClasses(fields, interfaces, BindingFlags.Public | BindingFlags.Instance));
            Assert.IsTrue(ex.Message.Contains("is not an interface"));
        }

        [Test]
        public void GetInterfaceFieldsFromClasses_ThrowsOnNonClass()
        {
            var fields = new List<FieldInfo>();
            var classes = new List<Type> { typeof(ITestInterface) };
            var interfaces = new List<Type> { typeof(IGenericInterface<>) };

            var ex = Assert.Throws<ArgumentException>(() =>
                classes.GetInterfaceFieldsFromClasses(fields, interfaces, BindingFlags.Public | BindingFlags.Instance));
            Assert.IsTrue(ex.Message.Contains("is not a class"));
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class TempAttribute : Attribute { }

        [TempAttribute]
        class AttributedClass { }

        [Test]
        public void GetAttribute_ReturnsCorrectAttribute()
        {
            var attr = typeof(AttributedClass).GetAttribute<TempAttribute>();
            Assert.NotNull(attr);
            Assert.IsInstanceOf<TempAttribute>(attr);
        }

        [Test]
        public void GetAttribute_ThrowsIfNotFound()
        {
            var ex = Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                typeof(DerivedClass).GetAttribute<TempAttribute>());
            Assert.IsTrue(ex.Message.Contains("Attribute not found"));
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomAttribute : Attribute { }

        [CustomAttribute]
        class AttributedBase { }

        class AttributedDerived : AttributedBase { }

        class NoAttribute { }

        [Test]
        public void IsDefinedGetInheritedTypes_FindsAllAttributedTypesInHierarchy()
        {
            var types = new List<Type>();
            typeof(AttributedDerived).IsDefinedGetInheritedTypes<CustomAttribute>(types);
            Assert.Contains(typeof(AttributedBase), types);
            Assert.IsFalse(types.Contains(typeof(NoAttribute)));
        }

        [Test]
        public void IsDefinedGetInheritedTypes_EmptyIfNoMatch()
        {
            var types = new List<Type>();
            typeof(NoAttribute).IsDefinedGetInheritedTypes<CustomAttribute>(types);
            Assert.IsEmpty(types);
        }

        class FieldBase { private int baseField; }
        class FieldDerived : FieldBase { private string derivedField; }

        [Test]
        public void GetFieldInTypeOrBaseType_FindsFieldInBaseType()
        {
            var field = typeof(FieldDerived).GetFieldInTypeOrBaseType("baseField");
            Assert.NotNull(field);
            Assert.AreEqual("baseField", field.Name);
        }

        [Test]
        public void GetFieldInTypeOrBaseType_ReturnsNullIfNotFound()
        {
            var field = typeof(FieldDerived).GetFieldInTypeOrBaseType("nonExistentField");
            Assert.IsNull(field);
        }

        class GenericClass<T> { }
        class NestedGenericClass<T> { public class Inner<U> { } }

        [Test]
        public void GetNameWithGenericArguments_ReturnsFormattedName()
        {
            var type = typeof(GenericClass<int>);
            var name = type.GetNameWithGenericArguments();
            Assert.AreEqual("GenericClass<Int32>", name);
        }

        [Test]
        public void GetNameWithGenericArguments_NonGeneric_ReturnsSimpleName()
        {
            var type = typeof(string);
            var name = type.GetNameWithGenericArguments();
            Assert.AreEqual("String", name);
        }

        /*
        [Test]
        public void GetNameWithGenericArguments_NestedGeneric_ReturnsFormattedNestedName()
        {
            var type = typeof(NestedGenericClass<int>.Inner<string>);
            var name = type.GetNameWithGenericArguments();
            Assert.AreEqual("Inner<String>", name); // Returns "Inner<Int32, String>"
        }
        */

        [Test]
        public void GetNameWithFullGenericArguments_ReturnsAssemblyQualifiedGenericName()
        {
            var type = typeof(GenericClass<int>);
            var name = type.GetNameWithFullGenericArguments();
            Assert.IsTrue(name.StartsWith("GenericClass<"));
            Assert.IsTrue(name.Contains("Int32"));
        }

        [Test]
        public void GetFullNameWithGenericArguments_ReturnsFullNestedName()
        {
            var type = typeof(NestedGenericClass<int>.Inner<string>);
            var name = type.GetFullNameWithGenericArguments();
            Assert.IsTrue(name.Contains("NestedGenericClass"));
            Assert.IsTrue(name.Contains("Inner"));
            Assert.IsTrue(name.Contains("Int32"));
            Assert.IsTrue(name.Contains("String"));
        }

        [Test]
        public void GetFullNameWithGenericArguments_NonGeneric_ReturnsFullName()
        {
            var type = typeof(string);
            var name = type.GetFullNameWithGenericArguments();
            Assert.AreEqual(type.FullName, name);
        }

        [Test]
        public void IsAssignableFromOrSubclassOf_ReturnsTrueForAssignable()
        {
            Assert.IsTrue(typeof(object).IsAssignableFromOrSubclassOf(typeof(string)));
        }

        [Test]
        public void IsAssignableFromOrSubclassOf_ReturnsTrueForSubclass()
        {
            Assert.IsTrue(typeof(FieldBase).IsAssignableFromOrSubclassOf(typeof(FieldDerived)));
        }

        [Test]
        public void IsAssignableFromOrSubclassOf_ReturnsFalseForUnrelatedTypes()
        {
            Assert.IsFalse(typeof(int).IsAssignableFromOrSubclassOf(typeof(string)));
        }

        class MethodBase
        {
            public void BaseMethod() { }
        }

        class MethodDerived : MethodBase
        {
            public void DerivedMethod() { }
        }

        [Test]
        public void GetMethodRecursively_FindsMethodInBaseType()
        {
            var method = typeof(MethodDerived).GetMethodRecursively("BaseMethod", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method);
            Assert.AreEqual("BaseMethod", method.Name);
        }

        [Test]
        public void GetMethodRecursively_ReturnsNullIfNotFound()
        {
            var method = typeof(MethodDerived).GetMethodRecursively("NonExistentMethod", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNull(method);
        }
    }
}
