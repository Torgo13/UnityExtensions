using System.Buffers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UnityExtensions.Unsafe
{
    public static class UITextExtensions
    {
        //https://github.com/Unity-Technologies/FPSSample/blob/6b8b27aca3690de9e46ca3fe5780af4f0eff5faa/Assets/Scripts/Utils/UIExtensionMethods.cs
        #region FPSSample
        public static void Format<T0>(this Text me, string format, T0 arg0)
        {
            char[] buf = ArrayPool<char>.Shared.Rent(1000);
            
            int l = StringFormatter.Write(ref buf, 0, format, arg0);
            me.Set(buf, l);
            
            ArrayPool<char>.Shared.Return(buf);
        }

#if PACKAGE_TEXTMESH_PRO
        public static void Format<T0>(this TMPro.TextMeshProUGUI me, string format, T0 arg0)
        {
            char[] buf = ArrayPool<char>.Shared.Rent(1000);
            
            int l = StringFormatter.Write(ref buf, 0, format, arg0);
            me.Set(buf, l);
            
            ArrayPool<char>.Shared.Return(buf);
        }
#endif // PACKAGE_TEXTMESH_PRO

        public static void Format<T0, T1>(this Text me, string format, T0 arg0, T1 arg1)
        {
            char[] buf = ArrayPool<char>.Shared.Rent(1000);
            
            int l = StringFormatter.Write(ref buf, 0, format, arg0, arg1);
            me.Set(buf, l);
            
            ArrayPool<char>.Shared.Return(buf);
        }

#if PACKAGE_TEXTMESH_PRO
        public static void Format<T0, T1>(this TMPro.TextMeshProUGUI me, string format, T0 arg0, T1 arg1)
        {
            char[] buf = ArrayPool<char>.Shared.Rent(1000);
            
            int l = StringFormatter.Write(ref buf, 0, format, arg0, arg1);
            me.Set(buf, l);
            
            ArrayPool<char>.Shared.Return(buf);
        }
#endif // PACKAGE_TEXTMESH_PRO

        public static void Format<T0, T1, T2>(this Text me, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            char[] buf = ArrayPool<char>.Shared.Rent(1000);
            
            int l = StringFormatter.Write(ref buf, 0, format, arg0, arg1, arg2);
            me.Set(buf, l);
            
            ArrayPool<char>.Shared.Return(buf);
        }

#if PACKAGE_TEXTMESH_PRO
        public static void Format<T0, T1, T2>(this TMPro.TextMeshProUGUI me, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            char[] buf = ArrayPool<char>.Shared.Rent(1000);
            
            int l = StringFormatter.Write(ref buf, 0, format, arg0, arg1, arg2);
            me.Set(buf, l);
            
            ArrayPool<char>.Shared.Return(buf);
        }
#endif // PACKAGE_TEXTMESH_PRO

        public static void Set(this Text me, char[] text, int length)
        {
            if (Set(me.text, text, length))
                me.text = new string(text, 0, length);
        }

#if PACKAGE_TEXTMESH_PRO
        public static void Set(this TMPro.TextMeshProUGUI me, char[] text, int length)
        {
            if (Set(me.text, text, length))
                me.SetText(text, 0, length);
        }
#endif // PACKAGE_TEXTMESH_PRO

        private static bool Set(string old, char[] text, int length)
        {
            Assert.IsNotNull(text);
            Assert.IsTrue(length >= 0);
            Assert.IsTrue(text.Length >= length);

            if (old == null || old.Length != length)
                return true;

            for (var i = 0; i < length; i++)
            {
                if (text[i] != old[i])
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetRGB(this Graphic graphic, Color color)
        {
            var c = graphic.color;
            graphic.color = new Color(color.r, color.g, color.b, c.a);
        }
        #endregion // FPSSample
    }
}
