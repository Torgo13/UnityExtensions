using System.Diagnostics;
using UnityEngine;

namespace UnityExtensions.Tests
{
    abstract class PerformanceTestBase : MonoBehaviour
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Runtime/Scripts/PerformanceTestBase.cs
        #region Unity.XR.CoreUtils.Tests
        [SerializeField]
        protected int m_CallCount = 1000;

        [SerializeField]
        protected int m_FrameCount = 10;

#if INCLUDE_IMGUI
#pragma warning disable 649
        [SerializeField]
        GUIStyle m_TextStyle;
#pragma warning restore 649
#endif

        protected long m_ElapsedTicks;

        protected string m_Report;

        protected readonly Stopwatch m_Timer = new Stopwatch();

        protected string m_TestClassLabel;
        protected string m_MethodLabel;
        protected int m_FrameCounter;

        bool m_IsTestFinished;

        protected void Update()
        {
            if (m_IsTestFinished)
                return;

            if (m_FrameCounter >= m_FrameCount)
            {
                GetReport();
                m_IsTestFinished = true;
                return;
            }

            RunTestFrame();
            m_FrameCounter++;
        }

        protected abstract string GetReport();

#if INCLUDE_IMGUI
        void OnGUI()
        {
            UnityEngine.GUI.TextArea(new Rect(0, 0, Screen.width, Screen.height), m_Report, m_TextStyle);
        }
#endif

        protected abstract void SetupData();
        protected abstract void RunTestFrame();
        #endregion // Unity.XR.CoreUtils.Tests
    }
}
