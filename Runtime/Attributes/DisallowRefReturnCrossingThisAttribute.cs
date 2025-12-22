using System;

namespace PKGE.Attributes
{
    /// <summary>
    ///     Attribute signaling that ref returned values, of a type that has this attribute, cannot intersect with
    ///     calls to methods that also have this attribute.
    ///     Motivation(s): ref returns of values that are backed by native memory (unsafe), like IComponentData in ecs chunks,
    ///     can have the referenced
    ///     memory invalidated by certain methods. A way is needed to detect these situations a compilation time to prevent
    ///     accessing invalidated references.
    ///     Notes:
    ///     - This attribute is used/feeds a Static Analyzer at compilation time.
    ///     - Attribute transfers with aggregations: struct A has this attribute, struct B has a field of type A; both A and B
    ///     are considered to have the attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Interface)]
    public class DisallowRefReturnCrossingThisAttribute : Attribute
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Entities/DisallowRefReturnCrossingThisAttribute.cs
        #region Unity.Entities
        #endregion // Unity.Entities
    }
}