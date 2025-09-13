namespace PKGE.Unsafe
{
    static class Debug
    {
        public unsafe static int ExtractStackTraceNoAlloc(byte* buffer, int bufferMax, string projectFolder) => UnityEngine.Debug.ExtractStackTraceNoAlloc(buffer, bufferMax, projectFolder);
    }
}
