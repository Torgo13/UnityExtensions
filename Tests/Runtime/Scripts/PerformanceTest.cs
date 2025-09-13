namespace PKGE.Tests
{
    abstract class PerformanceTest : PerformanceTestBase
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Runtime/Scripts/PerformanceTest.cs
        #region Unity.XR.CoreUtils.Tests
        public void Awake()
        {
            SetupData();

            RunTestFrame(); // make sure we JIT the code ahead of time
            m_Timer.Reset();
            m_ElapsedTicks = 0;

            m_TestClassLabel = GetType().Name;
        }

        protected override string GetReport()
        {
            var count = (float)(m_CallCount * m_FrameCounter);
            m_Report = $"{m_TestClassLabel} - {m_CallCount * m_FrameCount} calls\n\n";
            m_Report += $"using {m_MethodLabel}\naverage {m_ElapsedTicks / count} ticks / call\n";
            return m_Report;
        }
        #endregion // Unity.XR.CoreUtils.Tests
    }
}
