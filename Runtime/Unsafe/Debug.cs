namespace PKGE.Unsafe
{
    static class Debug
    {
        public static unsafe int ExtractStackTraceNoAlloc(byte* buffer, int bufferMax, string projectFolder)
            => UnityEngine.Debug.ExtractStackTraceNoAlloc(buffer, bufferMax, projectFolder);
    }
}
