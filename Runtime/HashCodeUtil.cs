namespace UnityExtensions
{
    static class HashCodeUtil
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/HashCodeUtil.cs
        #region Unity.XR.CoreUtils
        public static int Combine(int hash1, int hash2)
        {
            unchecked
            {
                return hash1 * 486187739 + hash2;
            }
        }

        public static int ReferenceHash(object obj) => obj != null ? obj.GetHashCode() : 0;

        public static int Combine(int hash1, int hash2, int hash3) => Combine(Combine(hash1, hash2), hash3);
        public static int Combine(int hash1, int hash2, int hash3, int hash4) => Combine(Combine(hash1, hash2, hash3), hash4);
        public static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5) => Combine(Combine(hash1, hash2, hash3, hash4), hash5);
        public static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6) => Combine(Combine(hash1, hash2, hash3, hash4, hash5), hash6);
        public static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6, int hash7) => Combine(Combine(hash1, hash2, hash3, hash4, hash5, hash6), hash7);
        public static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6, int hash7, int hash8) => Combine(Combine(hash1, hash2, hash3, hash4, hash5, hash6, hash7), hash8);
        #endregion // Unity.XR.CoreUtils
    }
}