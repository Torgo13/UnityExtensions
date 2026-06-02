#nullable enable
using UnityEngine;
using UnityEngine.Assertions;

namespace PKGE
{
    public static class UITextExtensions
    {
        //https://github.com/Unity-Technologies/FPSSample/blob/6b8b27aca3690de9e46ca3fe5780af4f0eff5faa/Assets/Scripts/Utils/UIExtensionMethods.cs
        #region FPSSample
#if INCLUDE_UGUI
        public static void Set(this UnityEngine.UI.Text me, char[] text, int length)
        {
            if (Set(me.text, text, length))
                me.text = new string(text, 0, length);
        }
#endif // INCLUDE_UGUI

#if INCLUDE_TEXTMESH_PRO
        public static void Set(this TMPro.TextMeshProUGUI me, char[] text, int length)
        {
            if (Set(me.text, text, length))
                me.SetText(text, 0, length);
        }
#endif // INCLUDE_TEXTMESH_PRO

        private static bool Set(string old, char[] text, int length)
        {
            Assert.IsTrue(length >= 0);
            Assert.IsTrue(text.Length >= length);

            if (old.Length != length)
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

#if INCLUDE_UGUI
        public static void SetRGB(this UnityEngine.UI.Graphic graphic, Color color)
        {
            var c = graphic.color;
            graphic.color = new Color(color.r, color.g, color.b, c.a);
        }
#endif // INCLUDE_UGUI
        #endregion // FPSSample
    }
}
