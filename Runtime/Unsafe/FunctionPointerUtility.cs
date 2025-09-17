#if INCLUDE_COLLECTIONS
using System;
using System.Reflection;
using Unity.Burst;

namespace PKGE.Unsafe
{
    public struct FunctionPointerUtility
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Runtime/Supplementary/Utility/FunctionPointerUtility.cs
        #region Unity.Kinematica
        public static FunctionPointer<TDelegate> CompileStaticMemberFunction<TDelegate>(Type type, string methodName) where TDelegate : class
        {
            if (type.GetCustomAttribute<BurstCompileAttribute>() == null)
            {
                throw new ArgumentException($"Compilation of function {methodName} from {type.Name} failed : class is missing [BurstCompile] attribute.");
            }

            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                throw new ArgumentException($"Compilation of function {methodName} from {type.Name} failed : method not found.");
            }

            if (method.GetCustomAttribute<BurstCompileAttribute>() == null)
            {
                throw new ArgumentException($"Compilation of function {methodName} from {type.Name} failed : method is missing [BurstCompile] attribute.");
            }

            TDelegate functionDelegate = (TDelegate)(object)method.CreateDelegate(typeof(TDelegate));

            try
            {
                FunctionPointer<TDelegate> functionPointer = BurstCompiler.CompileFunctionPointer<TDelegate>(functionDelegate);
                return functionPointer;
            }
            catch (Exception e)
            {
                throw new Exception($"Compilation of function {methodName} from {type.Name} failed : {e}");
            }
        }

        public static bool IsFunctionPointerValid<TDelegate>(ref FunctionPointer<TDelegate> functionPointer) where TDelegate : class
        {
            return UnsafeExtensions.AddressOf(ref functionPointer) != IntPtr.Zero;
        }
        #endregion // Unity.Kinematica
    }
}
#endif // INCLUDE_COLLECTIONS
