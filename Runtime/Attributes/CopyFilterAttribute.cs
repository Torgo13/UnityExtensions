using System;

namespace PKGE.Attributes
{
    /// <summary>
    /// When adding a field to a component, it's easy to forget to also add it in the CopyTo method.
    /// To ensure we don't forget it anymore, a Runtime test have been set.
    /// But sometimes we don't want to copy all fields. This attribute is here to whitelist some.
    /// Also in some case we want to copy the content and not the actual reference.
    /// </summary>
    /// <example><code>
    /// class Example
    /// {
    ///     int field1;                     //will check if value are equal
    ///     object field2;                  //will check if references are equal
    ///     [ValueCopy]
    ///     object field3;                  //will not check the reference but will check that each value inside are the same.
    ///     [ExcludeCopy]
    ///     int field4;                     //will not check anything
    ///     int property1 { get; set; }     //will check if the generated backing field is copied
    ///     object property3 { get; set; }  //will check if the generated backing field's references are equal
    ///     [field: ValueCopy]
    ///     object property3 { get; set; }  //will not check the reference but will check that each value inside are the same, in the generated backing field.
    ///     [field: ExcludeCopy]
    ///     int property2 { get; set; }     //will not check anything
    ///
    ///     // Also all delegate (include Action and Func) and backing field using them (such as event)
    ///     // will not be checked as moving a functor is touchy and should not be done most of the time.
    ///
    ///     void CopyTo(Example other)
    ///     {
    ///         // copy each relevant field here
    ///
    ///         // If Example is added to the type list in com.unity.render-pipelines.high-definition\Tests\Editor\CopyToTests.cs
    ///         // Every field and backing field non-white listed will raise an error if not copied in this CopyTo
    ///     }
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    class CopyFilterAttribute : Attribute
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/Utilities/CopyFilter.cs
        #region UnityEngine.Rendering.HighDefinition
        public enum Filter
        {
            Exclude = 1,        // field or backing field will not be checked by CopyTo test (whitelisting)
            CheckContent = 2    // check the content of object value instead of doing a simple reference check
        }
#if UNITY_EDITOR
        public readonly Filter filter;
#endif

        protected CopyFilterAttribute(Filter test)
        {
#if UNITY_EDITOR
            this.filter = test;
#endif
        }
        #endregion // UnityEngine.Rendering.HighDefinition
    }

    sealed class ExcludeCopyAttribute : CopyFilterAttribute
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/Utilities/CopyFilter.cs
        #region UnityEngine.Rendering.HighDefinition
        public ExcludeCopyAttribute()
            : base(Filter.Exclude)
        { }
        #endregion // UnityEngine.Rendering.HighDefinition
    }

    sealed class ValueCopyAttribute : CopyFilterAttribute
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/Utilities/CopyFilter.cs
        #region UnityEngine.Rendering.HighDefinition
        public ValueCopyAttribute()
            : base(Filter.CheckContent)
        { }
        #endregion // UnityEngine.Rendering.HighDefinition
    }
}
