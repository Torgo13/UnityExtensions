using System.Collections;

namespace UnityExtensions.Editor.Tests
{
    public static class TestUtils
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture.tests/Tests/Editor/TestUtils.cs
        #region Unity.LiveCapture.Tests.Editor
        /// <summary>
        /// Delays a unity test until the given number of frame updates have occured.
        /// </summary>
        public static IEnumerator WaitForPlayerLoopUpdates(int frames)
        {
            while (frames > 0)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
                frames--;
            }
        }
        #endregion // Unity.LiveCapture.Tests.Editor
    }
}
