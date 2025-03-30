using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Tests
{
    public class DynamicResolutionTests
    {
        private GameObject _testGameObject;
        private DynamicResolution _dynamicResolution;

        [SetUp]
        public void Setup()
        {
            // Create a GameObject with DynamicResolution component for testing
            _testGameObject = new GameObject("DynamicResolutionTestObject");
            _dynamicResolution = _testGameObject.AddComponent<DynamicResolution>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after tests
            Object.DestroyImmediate(_testGameObject);
        }

        /// <summary>
        /// Verify the default state of DynamicResolution.
        /// </summary>
        [Test]
        public void IsEnabled_DefaultSystemState_ReturnsTrue()
        {
            // Verify default state of SystemEnabled
            Assert.IsTrue(DynamicResolution.IsEnabled());
        }

        /// <summary>
        /// Check the behaviour of the Enable method.
        /// </summary>
        [Test]
        public void Enable_EnablesDynamicResolutionSystem()
        {
            DynamicResolution.Disable();
            Assert.IsFalse(DynamicResolution.IsEnabled());

            DynamicResolution.Enable();
            Assert.IsTrue(DynamicResolution.IsEnabled());
        }

        /// <summary>
        /// Check the behaviour of the Disable method, including resetting the scale factor.
        /// </summary>
        [Test]
        public void Disable_DisablesDynamicResolutionSystemAndResetsScale()
        {
            DynamicResolution.Enable();
            Assert.IsTrue(DynamicResolution.IsEnabled());

            DynamicResolution.Disable();
            Assert.IsFalse(DynamicResolution.IsEnabled());

            float updatedScaleFactor = (float)_dynamicResolution.GetType()
                .GetField("CurrentScaleFactor",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.GetValue(null)!;
            Assert.AreEqual(1.0f, updatedScaleFactor); // Ensure scale is reset
        }

        /// <summary>
        /// Verify that the target frame rate and frame time are updated correctly via the SetTargetFramerate method.
        /// </summary>
        [Test]
        public void SetTargetFramerate_UpdatesDesiredFrameRateAndFrameTime()
        {
            double newFrameRate = 75.0;
            DynamicResolution.SetTargetFramerate(newFrameRate);

            Assert.AreEqual(newFrameRate, DynamicResolution.GetTargetFramerate());
        }

        /*
        /// <summary>
        /// Simulate conditions where the scale factor needs adjustment based on the headroom and frame timings.
        /// </summary>
        [UnityTest]
        public IEnumerator Update_ScaleDecreasesWhenHeadroomNegative()
        {
            // Wait for frame times to be captured
            yield return null;
            yield return null;
            yield return null;
            
            // Simulate a case where headroom is negative
            _dynamicResolution.GetType()
                .GetField("GPUFrameTime",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_dynamicResolution, 200.0);
            _dynamicResolution.GetType()
                .GetField("DesiredFrameTime",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, double.Epsilon);

            float initialScaleFactor = 1.0f;
            _dynamicResolution.GetType()
                .GetField("CurrentScaleFactor",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, initialScaleFactor);

            // Call Update once
            yield return null;

            float updatedScaleFactor = (float)_dynamicResolution.GetType()
                .GetField("CurrentScaleFactor",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.GetValue(null)!;

            Assert.Less(updatedScaleFactor, initialScaleFactor); // Scale factor should decrease
        }
        */
    }
}
