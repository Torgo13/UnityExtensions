using NUnit.Framework;

namespace PKGE.Tests
{
    public class RandomNormalTest
    {
#if INCLUDE_MATHEMATICS
        const double k_FirstValue = -0.36536631752896848;
        const double k_SecondValue = 0.27865908699308078;
#else
        const float k_FirstValue = -1.19580f;
        const float k_SecondValue = -0.97345f;
#endif // INCLUDE_MATHEMATICS
        const double k_Epsilon = 0.0001;

        [Test]
        public void RandomNormalTestTwoDouble()
        {
            var rand = new Unity.Mathematics.Random(2018);
            double spareGaussian = double.NaN;

            var rn = RandomExtensions.NextGaussian(ref rand, ref spareGaussian);
            Assert.AreEqual(k_FirstValue, rn, k_Epsilon);

            rn = RandomExtensions.NextGaussian(ref rand, ref spareGaussian);
            Assert.AreEqual(k_SecondValue, rn, k_Epsilon);
        }

        [Test]
        public void RandomNormalTestWithMean()
        {
            var rand = new Unity.Mathematics.Random(2018);
            double spareGaussian = double.NaN;

            var rn = RandomExtensions.NextGaussian(ref rand, ref spareGaussian, mean: 5);
            Assert.AreEqual(k_FirstValue + 5.0, rn, k_Epsilon);

            rn = RandomExtensions.NextGaussian(ref rand, ref spareGaussian, mean: 5);
            Assert.AreEqual(k_SecondValue + 5.0, rn, k_Epsilon);
        }

        [Test]
        public void RandomNormalTestWithStddev()
        {
            var rand = new Unity.Mathematics.Random(2018);
            double spareGaussian = double.NaN;

            var rn = RandomExtensions.NextGaussian(ref rand, ref spareGaussian, mean: 0, stdDev: 4.2);
            Assert.AreEqual(k_FirstValue * 4.2, rn, k_Epsilon);

            rn = RandomExtensions.NextGaussian(ref rand, ref spareGaussian, mean: 0, stdDev: 4.2);
            Assert.AreEqual(k_SecondValue * 4.2, rn, k_Epsilon);
        }

        [Test]
        public void RandomNormalTestWithMeanStddev()
        {
            const float mean = -3.2f;
            const float stddev = 2.2f;
            var rand = new Unity.Mathematics.Random(2018);
            double spareGaussian = double.NaN;

            var rn = RandomExtensions.NextGaussian(ref rand, ref spareGaussian, mean, stddev);
            Assert.AreEqual(k_FirstValue * stddev + mean, rn, k_Epsilon);

            rn = RandomExtensions.NextGaussian(ref rand, ref spareGaussian, mean, stddev);
            Assert.AreEqual(k_SecondValue * stddev + mean, rn, k_Epsilon);
        }

        [Test]
        public void RandomNormalTestDistribution()
        {
            const float mean = -3.2f;
            const float stddev = 2.2f;
            var rand = new Unity.Mathematics.Random(2018);
            double spareGaussian = double.NaN;

            const int numSamples = 100000;
            // Adapted from https://www.johndcook.com/blog/standard_deviation/
            // Computes stddev and mean without losing precision
            double oldM = 0.0, newM = 0.0, oldS = 0.0, newS = 0.0;

            for (var i = 0; i < numSamples; i++)
            {
                var x = RandomExtensions.NextGaussian(ref rand, ref spareGaussian, mean, stddev);
                if (i == 0)
                {
                    oldM = newM = x;
                    oldS = 0.0;
                }
                else
                {
                    newM = oldM + (x - oldM) / i;
                    newS = oldS + (x - oldM) * (x - newM);

                    // set up for next iteration
                    oldM = newM;
                    oldS = newS;
                }
            }

            var sampleMean = newM;
            var sampleVariance = newS / (numSamples - 1);
            var sampleStdDev = System.Math.Sqrt(sampleVariance);

            // Note a larger epsilon here. We could get closer to the true values with more samples.
            Assert.AreEqual(mean, sampleMean, 0.01);
            Assert.AreEqual(stddev, sampleStdDev, 0.01);
        }
    }
}