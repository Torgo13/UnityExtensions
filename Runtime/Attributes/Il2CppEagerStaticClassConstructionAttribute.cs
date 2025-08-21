using System;

namespace UnityExtensions.Attributes
{
    /// <summary>
    /// Modelled after: <see href="https://github.com/dotnet/corert/blob/master/src/Runtime.Base/src/System/Runtime/CompilerServices/EagerStaticClassConstructionAttribute.cs"/>
    /// </summary>
    /// <remarks>
    /// When applied to a type this custom attribute will cause any static class constructor to be run eagerly
    /// at module load time rather than deferred till just before the class is used.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class Il2CppEagerStaticClassConstructionAttribute : Attribute
    {
        //https://docs.unity3d.com/Manual/scripting-backends-il2cpp.html#EnablingRuntimeChecksUsingIl2CppSetOption
        #region Unity.IL2CPP.CompilerServices
        #endregion // Unity.IL2CPP.CompilerServices
    }
}
