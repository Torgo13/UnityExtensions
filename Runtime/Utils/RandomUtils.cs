using System;

#if INCLUDE_MATHEMATICS
using Random = Unity.Mathematics.Random;
#else
using Random = System.Random;
#endif // INCLUDE_MATHEMATICS

namespace PKGE
{
    //https://github.com/Unity-Technologies/ml-agents/blob/cfb26e3eaf10e4866c0835a68087de392d631e88/com.unity.ml-agents/Runtime/Inference/Utils/RandomNormal.cs
    #region Unity.MLAgents.Inference.Utils
    /// <summary>
    /// RandomNormal - A random number generator that produces normally distributed random
    /// numbers using the Marsaglia polar method:
    /// https://en.wikipedia.org/wiki/Marsaglia_polar_method
    /// </summary>
    public struct RandomNormal
    {
        readonly double m_Mean;
        readonly double m_StdDev;
        readonly Random m_Random;

#if INCLUDE_MATHEMATICS
        public RandomNormal(uint seed, float mean = 0.0f, float stddev = 1.0f)
#else
        public RandomNormal(int seed, float mean = 0.0f, float stddev = 1.0f)
#endif // INCLUDE_MATHEMATICS
        {
            m_Mean = mean;
            m_StdDev = stddev;
            m_Random = new Random(seed);

            m_HasSpare = false;
            m_SpareUnscaled = 0;
        }

        // Each iteration produces two numbers. Hold one here for next call
        bool m_HasSpare;
        double m_SpareUnscaled;

        /// <summary>
        /// Return the next random double number.
        /// </summary>
        /// <returns>Next random double number.</returns>
        public double NextDouble()
        {
            if (m_HasSpare)
            {
                m_HasSpare = false;
                return m_SpareUnscaled * m_StdDev + m_Mean;
            }

            double u, v, s;
            do
            {
                u = m_Random.NextDouble() * 2.0 - 1.0;
                v = m_Random.NextDouble() * 2.0 - 1.0;
                s = u * u + v * v;
            }
            while (s >= 1.0 || Math.Abs(s) < double.Epsilon);

            s = Math.Sqrt(-2.0 * Math.Log(s) / s);
            m_SpareUnscaled = u * s;
            m_HasSpare = true;

            return v * s * m_StdDev + m_Mean;
        }
    }
    #endregion // Unity.MLAgents.Inference.Utils
}
