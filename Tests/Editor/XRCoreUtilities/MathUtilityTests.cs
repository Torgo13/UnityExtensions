using NUnit.Framework;
using UnityEngine;

namespace PKGE.Editor.Tests
{
    class MathUtilityTests
    {
        // each test case makes the range between the numbers much smaller,
        // so that we can be confident that we get the same answers even in very precise cases
        [TestCase(0.00001f)]
        [TestCase(0.0000000001f)]
        [TestCase(0.00000000000000000001f)]
        [TestCase(0.000000000000000000000000000001f)]
        [TestCase(0.0000000000000000000000000000000000001f)]
        public void ApproximatelyGivesSameResultsAsMathf(float r)
        {
            const int callCount = 100;
            var testCases = new Vector2[callCount];
            Random.InitState(0);
            for (var i = 0; i < callCount; i++)
            {
                testCases[i] = new Vector2(Random.Range(-r, r), Random.Range(-r, r));
            }

            foreach (var pair in testCases)
            {
                var ourResult = MathUtility.Approximately(pair.x, pair.y);
                var mathfResult = Mathf.Approximately(pair.x, pair.y);
                Assert.AreEqual(mathfResult, ourResult, $"comparing {pair.x} to {pair.y} - Mathf said {mathfResult} , we said {ourResult}");
            }
        }

        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture.tests/Tests/Editor/Core/MathUtilityTests.cs
        #region Unity.LiveCapture.Tests.Editor
        [TestCase(0d, 0d, 1d, ExpectedResult = 0d)]
        [TestCase(-1d, 0d, 1d, ExpectedResult = 0d)]
        [TestCase(0d, -1d, 1d, ExpectedResult = 0d)]
        [TestCase(0d, 0d, -1d, ExpectedResult = 0d)]
        [TestCase(3d, 0d, -1d, ExpectedResult = 0d)]
        [TestCase(3d, 0d, 1d, ExpectedResult = 1d)]
        public double Clamp(double value, double min, double max)
        {
            return MathUtility.Clamp(value, min, max);
        }
        #endregion // Unity.LiveCapture.Tests.Editor

        /// <summary>
        /// Validates that the distance from zero to zero degrees is zero.
        /// </summary>
        [Test]
        public void ShortestAngleDistance_ZeroToZero_ReturnsZero()
        {
            Assert.AreEqual(0, MathUtility.ShortestAngleDistance(0, 0, 180, 360));
            Assert.AreEqual(0, MathUtility.ShortestAngleDistance(0.0, 0.0, 180.0, 360.0));
        }

        /// <summary>
        /// Tests that the function returns the shortest distance when given positive angles.
        /// </summary>
        [Test]
        public void ShortestAngleDistance_PositiveAngles_ReturnsShortestDistance()
        {
            Assert.AreEqual(-20, MathUtility.ShortestAngleDistance(10, 350, 180, 360));
            Assert.AreEqual(-20, MathUtility.ShortestAngleDistance(10.0, 350.0, 180.0, 360.0));
        }

        /// <summary>
        /// Tests that the function returns the shortest distance when given negative angles.
        /// </summary>
        [Test]
        public void ShortestAngleDistance_NegativeAngles_ReturnsShortestDistance()
        {
            Assert.AreEqual(20, MathUtility.ShortestAngleDistance(-10, -350, 180, 360));
            Assert.AreEqual(20, MathUtility.ShortestAngleDistance(-10.0, -350.0, 180.0, 360.0));
        }

        /// <summary>
        /// Validates that the function returns the shortest distance when the angles cross zero.
        /// </summary>
        [Test]
        public void ShortestAngleDistance_CrossingZero_ReturnsShortestDistance()
        {
            Assert.AreEqual(20, MathUtility.ShortestAngleDistance(350, 10, 180, 360));
            Assert.AreEqual(20, MathUtility.ShortestAngleDistance(350.0, 10.0, 180.0, 360.0));
        }

        /// <summary>
        /// Tests that the function returns the shortest distance when the angles cross the halfMax value.
        /// </summary>
        [Test]
        public void ShortestAngleDistance_CrossingHalfMax_ReturnsShortestDistance()
        {
            Assert.AreEqual(20, MathUtility.ShortestAngleDistance(170, 190, 180, 360));
            Assert.AreEqual(20, MathUtility.ShortestAngleDistance(170.0, 190.0, 180.0, 360.0));
        }

        /// <summary>
        /// Ensures that the function returns zero when the start and end angles are equal.
        /// </summary>
        [Test]
        public void ShortestAngleDistance_EqualStartEnd_ReturnsZero()
        {
            Assert.AreEqual(0, MathUtility.ShortestAngleDistance(45, 45, 180, 360));
            Assert.AreEqual(0, MathUtility.ShortestAngleDistance(45.0, 45.0, 180.0, 360.0));
        }
    }
}
