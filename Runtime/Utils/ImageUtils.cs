using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Collections;

namespace UnityExtensions
{
    //https://github.com/Unity-Technologies/com.unity.search.extensions/blob/0896c65212ba17c718719ce75e53b9e97b0d261d/package-examples/Editor/ImageIndexing/ThreadUtils.cs
    #region UnityEditor.Search
    public static class ThreadUtils
    {
        public static int GetBatchSizeByCore(int totalSize, int minSizePerCore = 8)
        {
            return Mathf.Max(totalSize / Environment.ProcessorCount, minSizePerCore);
        }
    }
    #endregion // UnityEditor.Search
    
    //https://github.com/Unity-Technologies/com.unity.search.extensions/blob/0896c65212ba17c718719ce75e53b9e97b0d261d/package-examples/Editor/ImageIndexing/Filtering.cs
    #region UnityEditor.Search
    class Kernel
    {
        public int sizeX;
        public int sizeY;

        public double[] values;

        public double factor { get; }

        public double this[int y, int x] => values[y * sizeX + x];

        public Kernel(int sizeX, int sizeY, double[] values)
            : this(sizeX, sizeY, MathUtility.SafeDivide(1.0, values.Sum()), values)
        {}

        public Kernel(int sizeX, int sizeY, double factor, double[] values)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.values = values;
            this.factor = factor;
        }
    }

    static class Filtering
    {
        public static ImagePixels Convolve(ImagePixels texture, Kernel kernel)
        {
            var width = texture.width;
            var height = texture.height;
            var outputPixels = new Color[width * height];

            var pixels = texture.pixels;

            var halfXOffset = kernel.sizeX / 2;
            var halfYOffset = kernel.sizeY / 2;

            var rangeSize = ThreadUtils.GetBatchSizeByCore(height);
            var result = Parallel.ForEach(Partitioner.Create(0, height, rangeSize), range =>
            {
                for (var i = range.Item1; i < range.Item2; ++i)
                {
                    for (var j = 0; j < width; ++j)
                    {
                        var currentPixelIndex = i * width + j;
                        var currentPixel = pixels[currentPixelIndex];
                        var outputPixel = new Color();
                        for (var m = -halfYOffset; m <= halfYOffset; ++m)
                        {
                            var offsetY = i + m;
                            if (offsetY < 0 || offsetY >= height)
                                continue;
                            for (var n = -halfXOffset; n <= halfXOffset; ++n)
                            {
                                var offsetX = j + n;
                                if (offsetX < 0 || offsetX >= width)
                                    continue;

                                var offsetPixel = pixels[offsetY * width + offsetX];

                                // In a convolution, the signal is inverted
                                var kernelValue = kernel[-m + halfYOffset, -n + halfXOffset];

                                outputPixel += (float)(kernel.factor * kernelValue) * offsetPixel[3] * offsetPixel;
                            }
                        }

                        // Set the same alpha as the input
                        outputPixel[3] = currentPixel[3];
                        outputPixels[i * width + j] = outputPixel;
                    }
                }
            });

            if (!result.IsCompleted)
                Debug.LogError("Filtering did not complete successfully.");

            var outputTexture = new ImagePixels(width, height, outputPixels);
            return outputTexture;
        }

        public static ImagePixels Subtract(ImagePixels sourceA, ImagePixels sourceB)
        {
            if (sourceA.height != sourceB.height || sourceA.width != sourceB.width)
                throw new ArgumentException("Images don't have the same size");

            var width = sourceA.width;
            var height = sourceA.height;
            var outputPixels = new Color[width * height];

            var pixelsA = sourceA.pixels;
            var pixelsB = sourceB.pixels;

            var batchSize = ThreadUtils.GetBatchSizeByCore(height);
            Parallel.ForEach(Partitioner.Create(0, height, batchSize), range =>
            {
                for (var i = range.Item1; i < range.Item2; ++i)
                {
                    for (var j = 0; j < width; ++j)
                    {
                        var index = i * width + j;
                        outputPixels[index] = pixelsA[index] - pixelsB[index];
                    }
                }
            });

            return new ImagePixels(width, height, outputPixels);
        }
    }
    #endregion // UnityEditor.Search
    
    //https://github.com/Unity-Technologies/com.unity.search.extensions/blob/0896c65212ba17c718719ce75e53b9e97b0d261d/package-examples/Editor/ImageIndexing/ImagePixels.cs
    #region UnityEditor.Search
    public class ImagePixels
    {
        public int width;
        public int height;
        public Color[] pixels;

        public ImagePixels(Texture2D texture)
        {
            width = texture.width;
            height = texture.height;
            pixels = texture.GetPixels();
        }

        public ImagePixels(int width, int height, Color[] pixels)
        {
            this.width = width;
            this.height = height;
            this.pixels = pixels;
        }
    }
    #endregion // UnityEditor.Search
    
    //https://github.com/Unity-Technologies/com.unity.search.extensions/blob/0896c65212ba17c718719ce75e53b9e97b0d261d/package-examples/Editor/ImageIndexing/Filters.cs
    #region UnityEditor.Search
    interface IImageFilter
    {
        ImagePixels Apply(ImagePixels source);
    }

    class SobelXFilter : IImageFilter
    {
        public ImagePixels Apply(ImagePixels source)
        {
            var edge = Filtering.Convolve(source, SobelFilter.SobelX);
            var stretchedImage = ImageUtils.StretchImage(edge, 0f, 1f);
            return stretchedImage;
        }
    }

    class SobelYFilter : IImageFilter
    {
        public ImagePixels Apply(ImagePixels source)
        {
            var edge = Filtering.Convolve(source, SobelFilter.SobelY);
            var stretchedImage = ImageUtils.StretchImage(edge, 0f, 1f);
            return stretchedImage;
        }
    }

    class SobelFilter : IImageFilter
    {
        public static readonly Kernel SobelX = new Kernel(3, 3, new double[] { 1, 0, -1, 2, 0, -2, 1, 0, -1 });
        public static readonly Kernel SobelY = new Kernel(3, 3, new double[] { 1, 2, 1, 0, 0, 0, -1, -2, -1 });

        public Color[] gradients { get; private set; }

        public float threshold;

        public SobelFilter(float threshold)
        {
            this.threshold = threshold;
        }

        public ImagePixels Apply(ImagePixels source)
        {
            var edgeX = Filtering.Convolve(source, SobelX);
            var edgeY = Filtering.Convolve(source, SobelY);

            var magnitudePixels = new Color[source.height * source.width];
            gradients = new Color[source.height * source.width];
            var rangeSize = ThreadUtils.GetBatchSizeByCore(source.height);
            Parallel.ForEach(Partitioner.Create(0, source.height, rangeSize), range =>
            {
                for (var i = range.Item1; i < range.Item2; ++i)
                {
                    for (var j = 0; j < source.width; ++j)
                    {
                        var index = i * source.width + j;
                        var colorX = edgeX.pixels[index];
                        var colorY = edgeY.pixels[index];
                        var edgeOutput = new Color();
                        var gradientOutput = new Color();
                        for (var k = 0; k < 3; ++k)
                        {
                            var mag = Mathf.Clamp01(Mathf.Sqrt(colorX[k] * colorX[k] + colorY[k] * colorY[k]));
                            edgeOutput[k] = mag >= threshold ? 1f : 0f;
                            gradientOutput[k] = Mathf.Atan2(colorY[k], colorX[k]);
                        }

                        magnitudePixels[index] = edgeOutput;
                        gradients[index] = gradientOutput;
                    }
                }
            });

            return new ImagePixels(source.width, source.height, magnitudePixels);
        }
    }

    class GaussianFilter : IImageFilter
    {
        Kernel m_KernelX;
        Kernel m_KernelY;

        int m_Size;
        public int size
        {
            get
            {
                return m_Size;
            }
            set
            {
                m_Size = value;
                RebuildKernels(size, sigma);
            }
        }

        double m_Sigma;
        public double sigma
        {
            get
            {
                return m_Sigma;
            }
            set
            {
                m_Sigma = value;
                RebuildKernels(size, sigma);
            }
        }

        public GaussianFilter(int size, double sigma)
        {
            RebuildKernels(size, sigma);
        }

        public ImagePixels Apply(ImagePixels source)
        {
            var resultX = Filtering.Convolve(source, m_KernelX);
            return Filtering.Convolve(resultX, m_KernelY);
        }

        void RebuildKernels(int size, double sigma)
        {
            if (size % 2 == 0)
                throw new ArgumentException("Kernel size must be odd.", nameof(size));

            var kernelValues = new double[size];
            var halfSize = size / 2;
            var sigmaSquare = sigma * sigma;
            var expoScale = 1 / (2 * 3.14159 * sigmaSquare);
            for (var halfX = -halfSize; halfX <= halfSize; ++halfX)
            {
                var value = expoScale * Math.Exp(-(halfX * halfX) / (2 * sigmaSquare));
                kernelValues[halfX + halfSize] = value;
            }

            m_KernelX = new Kernel(size, 1, kernelValues);
            m_KernelY = new Kernel(1, size, kernelValues);
        }

        public static int GetSizeFromSigma(double sigma)
        {
            var size = Mathf.FloorToInt(3 * (float)sigma);
            if (size % 2 == 0)
                ++size;
            return size;
        }
    }

    class DifferenceOfGaussian : IImageFilter
    {
        GaussianFilter m_SmallFilter;
        GaussianFilter m_LargeFilter;

        public int smallSize
        {
            get => m_SmallFilter.size;
            set => m_SmallFilter.size = value;
        }

        public int largeSize
        {
            get => m_LargeFilter.size;
            set => m_LargeFilter.size = value;
        }

        public double sigma
        {
            get => m_SmallFilter.sigma;
            set
            {
                m_SmallFilter.sigma = value;
                m_LargeFilter.sigma = value;
            }
        }

        public bool stretchImageForViewing { get; private set; }

        public DifferenceOfGaussian(int sizeSmall, int sizeLarge, double sigma, bool stretchImageForViewing = false)
        {
            m_SmallFilter = new GaussianFilter(sizeSmall, sigma);
            m_LargeFilter = new GaussianFilter(sizeLarge, sigma);
            this.stretchImageForViewing = stretchImageForViewing;
        }

        public ImagePixels Apply(ImagePixels source)
        {
            var sourceA = m_SmallFilter.Apply(source);
            var sourceB = m_LargeFilter.Apply(source);

            var sub = Filtering.Subtract(sourceA, sourceB);
            if (stretchImageForViewing)
                return ImageUtils.StretchImage(sub, 0.0f, 1.0f);
            return sub;
        }
    }
    #endregion // UnityEditor.Search
    
    //https://github.com/Unity-Technologies/com.unity.search.extensions/blob/0896c65212ba17c718719ce75e53b9e97b0d261d/package-examples/Editor/ImageIndexing/ImageData.cs
    #region UnityEditor.Search
    public struct ColorInfo
    {
        public uint color;
        public double ratio;

        public override string ToString()
        {
            return $"{ImageUtils.IntToColor32(color)} [{(ratio * 100)}%]";
        }
    }

    public interface IHistogram
    {
        int bins { get; }
        int channels { get; }

        float[] GetBins(int channel);
    }

    public class Histogram : IHistogram
    {
        public const int histogramSize = 256;

        public virtual int bins => histogramSize;
        public int channels => 3;

        public float[] valuesR = new float[histogramSize];
        public float[] valuesG = new float[histogramSize];
        public float[] valuesB = new float[histogramSize];

        public void AddPixel(Color32 pixel)
        {
            ++valuesR[pixel.r];
            ++valuesG[pixel.g];
            ++valuesB[pixel.b];
        }

        public void Normalize(int totalPixels)
        {
            for (var i = 0; i < bins; ++i)
            {
                valuesR[i] /= totalPixels;
                valuesG[i] /= totalPixels;
                valuesB[i] /= totalPixels;
            }
        }

        public void Normalize(int[] totalPixels)
        {
            if (totalPixels.Length != 3)
                throw new ArgumentException($"Array size should be {channels}", nameof(totalPixels));

            for (var i = 0; i < bins; ++i)
            {
                valuesR[i] /= totalPixels[0];
                valuesG[i] /= totalPixels[1];
                valuesB[i] /= totalPixels[2];
            }
        }

        // Combine multiple partial histogram before normalizing
        public void Combine(Histogram histogram)
        {
            for (var i = 0; i < bins; ++i)
            {
                valuesR[i] += histogram.valuesR[i];
                valuesG[i] += histogram.valuesG[i];
                valuesB[i] += histogram.valuesB[i];
            }
        }

        public float[] GetBins(int channel)
        {
            switch (channel)
            {
                case 0:
                    return valuesR;
                case 1:
                    return valuesG;
                case 2:
                    return valuesB;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel));
            }
        }

        public override string ToString()
        {
            using var _0 = StringBuilderPool.Get(out var sb);
            sb.Append("R: (").AppendJoin(", ", valuesR).Append(')').AppendLine();
            sb.Append("G: (").AppendJoin(", ", valuesG).Append(')').AppendLine();
            sb.Append("B: (").AppendJoin(", ", valuesB).Append(')').AppendLine();
            return sb.ToString();
        }
    }

    public enum EdgeDirection
    {
        DEG_0 = 0,
        DEG_45 = 1,
        DEG_90 = 2,
        DEG_135 = 3
    }

    public class EdgeHistogram : Histogram
    {
        public static readonly int edgeDirections = Enum.GetNames(typeof(EdgeDirection)).Length;

        public override int bins => edgeDirections;

        public EdgeHistogram()
        {
            valuesR = new float[edgeDirections];
            valuesG = new float[edgeDirections];
            valuesB = new float[edgeDirections];
        }

        public void AddEdge(int channel, EdgeDirection direction)
        {
            var values = GetBins(channel);
            ++values[(int)direction];
        }

        public void AddEdge(int channel, float degree)
        {
            var direction = GetDirection(degree);
            AddEdge(channel, direction);
        }

        public static EdgeDirection GetDirection(float degree)
        {
            while (degree < 0)
                degree += 180;
            while (degree > 180)
                degree -= 180;

            var region = Mathf.RoundToInt(degree / 45) % 4;
            return (EdgeDirection)region;
        }
    }

    public struct ImageData
    {
        public const int version = 0x03;

        public Hash128 guid;
        public ColorInfo[] bestColors;
        public ColorInfo[] bestShades;
        public Histogram histogram;
        public EdgeHistogram edgeHistogram;
        public double[] edgeDensities;
        public double[] geometricMoments;

        public ImageData(string assetPath)
        {
            guid = Hash128.Compute(assetPath);
            bestColors = new ColorInfo[5];
            bestShades = new ColorInfo[5];
            histogram = new Histogram();
            edgeHistogram = new EdgeHistogram();
            edgeDensities = new double[3];
            geometricMoments = new double[3];
        }

        public ImageData(Hash128 assetGuid)
        {
            guid = assetGuid;
            bestColors = new ColorInfo[5];
            bestShades = new ColorInfo[5];
            histogram = new Histogram();
            edgeHistogram = new EdgeHistogram();
            edgeDensities = new double[3];
            geometricMoments = new double[3];
        }
    }
    #endregion // UnityEditor.Search
    
    //https://github.com/Unity-Technologies/com.unity.search.extensions/blob/0896c65212ba17c718719ce75e53b9e97b0d261d/package-examples/Editor/ImageIndexing/ImageUtils.cs
    #region UnityEditor.Search

    public enum HistogramDistance
    {
        CityBlock,
        Euclidean,
        Bhattacharyya,
        MDPA
    }

    enum XYZObserver
    {
        TwoDeg,
        TenDeg
    }

    enum XYZIlluminant
    {
        A,
        B,
        C,
        D50,
        D55,
        D65,
        D75,
        E,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12
    }

    static class XYZReferences
    {
        static Vector3[,] s_References;

        static XYZReferences()
        {
            s_References = new Vector3[Enum.GetNames(typeof(XYZObserver)).Length, Enum.GetNames(typeof(XYZIlluminant)).Length];

            SetReference(XYZObserver.TwoDeg, XYZIlluminant.A, new Vector3(109.850f, 100.000f, 35.585f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.B, new Vector3(99.0927f, 100.000f, 85.313f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.C, new Vector3(98.074f, 100.000f, 118.232f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.D50, new Vector3(96.422f, 100.000f, 82.521f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.D55, new Vector3(95.682f, 100.000f, 92.149f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.D65, new Vector3(95.047f, 100.000f, 108.883f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.D75, new Vector3(94.972f, 100.000f, 122.638f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.E, new Vector3(100.000f, 100.000f, 100.000f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F1, new Vector3(92.834f, 100.000f, 103.665f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F2, new Vector3(99.187f, 100.000f, 67.395f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F3, new Vector3(103.754f, 100.000f, 49.861f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F4, new Vector3(109.147f, 100.000f, 38.813f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F5, new Vector3(90.872f, 100.000f, 98.723f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F6, new Vector3(97.309f, 100.000f, 60.191f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F7, new Vector3(95.044f, 100.000f, 108.755f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F8, new Vector3(96.413f, 100.000f, 82.333f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F9, new Vector3(100.365f, 100.000f, 67.868f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F10, new Vector3(96.174f, 100.000f, 81.712f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F11, new Vector3(100.966f, 100.000f, 64.370f));
            SetReference(XYZObserver.TwoDeg, XYZIlluminant.F12, new Vector3(108.046f, 100.000f, 39.228f));

            SetReference(XYZObserver.TenDeg, XYZIlluminant.A, new Vector3(111.144f, 100.000f, 35.200f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.B, new Vector3(99.178f, 100.000f, 84.3493f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.C, new Vector3(97.285f, 100.000f, 116.145f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.D50, new Vector3(96.720f, 100.000f, 81.427f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.D55, new Vector3(95.799f, 100.000f, 90.926f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.D65, new Vector3(94.811f, 100.000f, 107.304f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.D75, new Vector3(94.416f, 100.000f, 120.641f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.E, new Vector3(100.000f, 100.000f, 100.000f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F1, new Vector3(94.791f, 100.000f, 103.191f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F2, new Vector3(103.280f, 100.000f, 69.026f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F3, new Vector3(108.968f, 100.000f, 51.965f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F4, new Vector3(114.961f, 100.000f, 40.963f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F5, new Vector3(93.369f, 100.000f, 98.636f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F6, new Vector3(102.148f, 100.000f, 62.074f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F7, new Vector3(95.792f, 100.000f, 107.687f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F8, new Vector3(97.115f, 100.000f, 81.135f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F9, new Vector3(102.116f, 100.000f, 67.826f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F10, new Vector3(99.001f, 100.000f, 83.134f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F11, new Vector3(103.866f, 100.000f, 65.627f));
            SetReference(XYZObserver.TenDeg, XYZIlluminant.F12, new Vector3(111.428f, 100.000f, 40.353f));
        }

        public static Vector3 GetReference(XYZObserver observer, XYZIlluminant illuminant)
        {
            return s_References[(int)observer, (int)illuminant];
        }

        static void SetReference(XYZObserver observer, XYZIlluminant illuminant, Vector3 reference)
        {
            s_References[(int)observer, (int)illuminant] = reference;
        }
    }

    class ColorCluster
    {
        int[] m_CurrentTotals = {0, 0, 0, 0};
        Color32 m_Average;

        public Color32 average => m_Average;
        public int count { get; set; }

        public void AddColor(Color32 color)
        {
            ++count;
            for (var i = 0; i < 4; ++i)
            {
                m_CurrentTotals[i] += color[i];
                m_Average[i] = (byte)(m_CurrentTotals[i] / count);
            }
        }

        public void Combine(ColorCluster cluster)
        {
            if (cluster.count == 0)
                return;
            count += cluster.count;
            for (var i = 0; i < 4; ++i)
            {
                m_CurrentTotals[i] += cluster.m_CurrentTotals[i];
                m_Average[i] = (byte)(m_CurrentTotals[i] / count);
            }
        }
    }

    class RGBClusters
    {
        const int k_AxisDivisions = 8;
        const int k_BucketSize = 256 / k_AxisDivisions;

        List<ColorCluster> m_Clusters;

        public RGBClusters()
        {
            m_Clusters = new List<ColorCluster>(k_AxisDivisions * k_AxisDivisions * k_AxisDivisions);
            for (var i = 0; i < k_AxisDivisions * k_AxisDivisions * k_AxisDivisions; i++)
            {
                m_Clusters.Add(new ColorCluster());
            }
        }

        public void AddColor(Color32 color)
        {
            var indexR = FindAxisIndex(color.r);
            var indexG = FindAxisIndex(color.g);
            var indexB = FindAxisIndex(color.b);
            var index = (indexR * k_AxisDivisions + indexG) * k_AxisDivisions + indexB;

            m_Clusters[index].AddColor(color);
        }

        static int FindAxisIndex(byte color)
        {
            return color / k_BucketSize;
        }

        public IEnumerable<ColorCluster> GetBestClusters(int count)
        {
            m_Clusters.Sort((cluster1, cluster2) => cluster2.count.CompareTo(cluster1.count));
            return m_Clusters.GetRange(0, count);
        }

        public void Combine(RGBClusters clusters)
        {
            for (var i = 0; i < m_Clusters.Count; ++i)
            {
                m_Clusters[i].Combine(clusters.m_Clusters[i]);
            }
        }
    }

    public struct MinMaxColor
    {
        public Color min;
        public Color max;

        public MinMaxColor(Color min, Color max)
        {
            this.min = min;
            this.max = max;
        }
    }
    #endregion // UnityEditor.Search
    
    public static class ImageUtils
    {
        //https://github.com/Unity-Technologies/com.unity.search.extensions/blob/0896c65212ba17c718719ce75e53b9e97b0d261d/package-examples/Editor/ImageIndexing/ImageUtils.cs
        #region UnityEditor.Search
        static readonly float k_MaxColorDistance = Vector3.one.magnitude;
        static readonly float k_MaxColorCIEDistance = k_MaxColorDistance * 100f;
        static readonly float k_MaxExponentialDistance = Mathf.Exp(k_MaxColorDistance) - 1;

        public static uint ColorToInt(Color32 color)
        {
            var value = 0U;
            value |= (uint)(color.r << 24);
            value |= (uint)(color.g << 16);
            value |= (uint)(color.b << 8);
            value |= (uint)(color.a << 0);
            return value;
        }

        public static Color32 IntToColor32(uint colorValue)
        {
            var color = new Color32();
            color.r = (byte)(colorValue >> 24);
            color.g = (byte)(colorValue >> 16);
            color.b = (byte)(colorValue >> 8);
            color.a = (byte)(colorValue >> 0);
            return color;
        }

        public static int ColorSquareDistance(Color32 colorA, Color32 colorB)
        {
            var valueA = new Vector3Int(colorA.r, colorA.g, colorA.b);
            var valueB = new Vector3Int(colorB.r, colorB.g, colorB.b);
            return (valueB - valueA).sqrMagnitude;
        }

        public static float ColorDistance(Color32 colorA, Color32 colorB)
        {
            return Mathf.Sqrt(ColorSquareDistance(colorA, colorB));
        }

        public static float ColorSquareDistance(Color colorA, Color colorB)
        {
            var valueA = new Vector3(colorA.r, colorA.g, colorA.b);
            var valueB = new Vector3(colorB.r, colorB.g, colorB.b);
            return (valueB - valueA).sqrMagnitude;
        }

        public static float ColorDistance(Color colorA, Color colorB)
        {
            return Mathf.Sqrt(ColorSquareDistance(colorA, colorB));
        }

        public static void RGBToXYZ(Color rgb, out float[] xyz)
        {
            float[] scaledColors = System.Buffers.ArrayPool<float>.Shared.Rent(3);
            for (var i = 0; i < 3; ++i)
            {
                if (rgb[i] > 0.04045) scaledColors[i] = Mathf.Pow((rgb[i] + 0.055f) / 1.055f, 2.4f);
                else scaledColors[i] = rgb[i] / 12.92f;

                scaledColors[i] *= 100f;
            }

            xyz = new float[3];
            xyz[0] = scaledColors[0] * 0.4124f + scaledColors[1] * 0.3576f + scaledColors[2] * 0.1805f;
            xyz[1] = scaledColors[0] * 0.2126f + scaledColors[1] * 0.7152f + scaledColors[2] * 0.0722f;
            xyz[2] = scaledColors[0] * 0.0193f + scaledColors[1] * 0.1192f + scaledColors[2] * 0.9505f;
            
            System.Buffers.ArrayPool<float>.Shared.Return(scaledColors);
        }

        public static void XYZToCIELab(float[] xyz, out float[] lab, Vector3 reference)
        {
            float[] scaledXYZ = System.Buffers.ArrayPool<float>.Shared.Rent(3);
            for (var i = 0; i < 3; ++i)
            {
                scaledXYZ[i] = xyz[i] / reference[i];
                if (scaledXYZ[i] > 0.008856) scaledXYZ[i] = Mathf.Pow(scaledXYZ[i], (1 / 3f));
                else scaledXYZ[i] = (7.787f * scaledXYZ[i]) + (16f / 116f);
            }

            lab = new float[3];
            lab[0] = (116f * scaledXYZ[1]) - 16f;
            lab[1] = 500f * (scaledXYZ[0] - scaledXYZ[1]);
            lab[2] = 200f * (scaledXYZ[1] - scaledXYZ[2]);
            
            System.Buffers.ArrayPool<float>.Shared.Return(scaledXYZ);
        }

        public static void RGBToYUV(Color rgb, out float[] yuv)
        {
            yuv = new float[3];
            yuv[0] = 0.299f * rgb.r + 0.587f * rgb.g + 0.114f * rgb.b;
            yuv[1] = -0.14713f * rgb.r + -0.28886f * rgb.g + 0.436f * rgb.b;
            yuv[2] = 0.615f * rgb.r + -0.51499f * rgb.g + -0.10001f * rgb.b;
        }

        public static float DeltaECIE(float[] lab1, float[] lab2)
        {
            using var _0 = ListPool<float>.Get(out var diffs);
            diffs.Add(lab1[0] - lab2[0]);
            diffs.Add(lab1[0] - lab2[0]);
            diffs.Add(lab1[0] - lab2[0]);
            return Mathf.Sqrt((diffs[0] * diffs[0]) + (diffs[1] * diffs[1]) + (diffs[2] * diffs[2]));
        }

        public static float DeltaE1994(float[] lab1, float[] lab2)
        {
            const float WHTL = 1.0f;
            const float WHTC = 1.0f;
            const float WHTH = 1.0f;

            var xC1 = Mathf.Sqrt((lab1[1] * lab1[1]) + (lab1[2] * lab1[2]));
            var xC2 = Mathf.Sqrt((lab2[1] * lab2[1]) + (lab2[2] * lab2[2]));
            var xDL = lab2[0] - lab1[0];
            var xDC = xC2 - xC1;

            var sum = 0f;
            for (var i = 0; i < lab1.Length; ++i)
            {
                var diff = lab1[0] - lab2[0];
                sum += diff * diff;
            }
            var xDE = Mathf.Sqrt(sum);

            var xDH = (xDE * xDE) - (xDL * xDL) - (xDC * xDC);
            if (xDH > 0)
            {
                xDH = Mathf.Sqrt(xDH);
            }
            else
            {
                xDH = 0;
            }

            var xSC = 1f + (0.045f * xC1);
            var xSH = 1f + (0.015f * xC1);
            xDL /= WHTL;
            xDC /= WHTC * xSC;
            xDH /= WHTH * xSH;

            return Mathf.Sqrt(xDL * xDL + xDC * xDC + xDH * xDH);
        }

        public static float CIELabDistance(Color colorA, Color colorB)
        {
            RGBToXYZ(colorA, out var xyzA);
            RGBToXYZ(colorB, out var xyzB);
            XYZToCIELab(xyzA, out var labA, XYZReferences.GetReference(XYZObserver.TwoDeg, XYZIlluminant.D65));
            XYZToCIELab(xyzB, out var labB, XYZReferences.GetReference(XYZObserver.TwoDeg, XYZIlluminant.D65));
            return DeltaE1994(labA, labB);
        }

        public static float YUVDistance(Color colorA, Color colorB)
        {
            RGBToYUV(colorA, out var yuvA);
            RGBToYUV(colorB, out var yuvB);

            var sum = 0f;
            for (var i = 1; i < yuvA.Length; ++i)
            {
                var diff = yuvA[i] - yuvB[i];
                sum += diff * diff;
            }

            return Mathf.Sqrt(sum);
        }

        public static double WeightedSimilarity(Color colorA, double ratio, Color colorB)
        {
            var distance = ColorDistance(colorA, colorB) / k_MaxColorDistance;

            // The similarity must drop very quickly based on the distance
            // var exponentialDistance = (Mathf.Exp(distance) - 1) / k_MaxExponentialDistance;

            return ratio * (1.0f - distance);
        }

        public static void ComputeHistogram(NativeArray<Color32> pixels, Histogram histogram)
        {
            foreach (var pixel in pixels)
            {
                histogram.AddPixel(pixel);
            }
            histogram.Normalize(pixels.Length);
        }

        public static void ComputeHistogram(Texture2D texture, Histogram histogram)
        {
            var pixels = texture.GetPixelData<Color32>(mipLevel: 0);
            ComputeHistogram(pixels, histogram);
        }

        struct LocalColorMap
        {
            public Histogram histogram;
            public Dictionary<uint, long> colorMap;
            public RGBClusters clusters;

            public LocalColorMap(Histogram histogram, Dictionary<uint, long> colorMap, RGBClusters clusters)
            {
                this.histogram = histogram;
                this.colorMap = colorMap;
                this.clusters = clusters;
            }
        }

        public static void ComputeBestColorsAndHistogram(Color32[] pixels, ColorInfo[] bestColors, ColorInfo[] bestShades, Histogram histogram)
        {
            var nbPixels = pixels.Length;
            using var _0 = DictionaryPool<uint, long>.Get(out var colorMap);
            var rgbClusters = new RGBClusters();

            foreach (var pixel in pixels)
            {
                var pixelValue = ColorToInt(pixel);
                histogram.AddPixel(pixel);
                colorMap.TryAdd(pixelValue, 0);
                ++colorMap[pixelValue];
                rgbClusters.AddColor(pixel);
            }
            histogram.Normalize(nbPixels);

            // Get the best colors
            using var _1 = ListPool<KeyValuePair<uint, long>>.Get(out var orderedColors);
            // Order in reverse order so the highest count is first
            orderedColors.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            var bestOrderedColors = orderedColors;
            bestOrderedColors.RemoveRange(5, bestOrderedColors.Count - 5);
            
            using var _2 = ListPool<ColorCluster>.Get(out var bestClusters);
            bestClusters.AddRange(rgbClusters.GetBestClusters(5));
            for (var i = bestOrderedColors.Count; i < 5; ++i)
            {
                bestOrderedColors.Add(new KeyValuePair<uint, long>(0, 0));
            }

            for (var i = 0; i < 5; ++i)
            {
                bestColors[i] = new ColorInfo { color = bestOrderedColors[i].Key, ratio = bestOrderedColors[i].Value / (double)nbPixels };
                bestShades[i] = new ColorInfo { color = ColorToInt(bestClusters[i].average), ratio = bestClusters[i].count / (double)nbPixels };
            }
        }

        public static void ComputeBestColorsAndHistogram_Parallel(Color32[] pixels, ColorInfo[] bestColors, ColorInfo[] bestShades, Histogram histogram)
        {
            var nbPixels = pixels.Length;
            using var _0 = DictionaryPool<uint, long>.Get(out var colorMap);
            var rgbClusters = new RGBClusters();

            var batchSize = ThreadUtils.GetBatchSizeByCore(pixels.Length);
            Parallel.ForEach(Partitioner.Create(0, pixels.Length, batchSize), () =>
            {
                return new LocalColorMap(new Histogram(), new Dictionary<uint, long>(), new RGBClusters());
            }, (range, state, localColorMap) =>
                {
                    for (var i = range.Item1; i < range.Item2; ++i)
                    {
                        var pixel = pixels[i];
                        var pixelValue = ColorToInt(pixel);
                        localColorMap.histogram.AddPixel(pixel);
                        localColorMap.colorMap.TryAdd(pixelValue, 0);
                        ++localColorMap.colorMap[pixelValue];
                        localColorMap.clusters.AddColor(pixel);
                    }

                    return localColorMap;
                }, localColorMap =>
                {
                    lock (histogram)
                    {
                        histogram.Combine(localColorMap.histogram);
                        rgbClusters.Combine(localColorMap.clusters);

                        foreach (var kvp in localColorMap.colorMap)
                        {
                            colorMap.TryAdd(kvp.Key, 0);
                            colorMap[kvp.Key] += kvp.Value;
                        }
                    }
                });
            histogram.Normalize(nbPixels);

            // Get the best colors
            using var _1 = ListPool<KeyValuePair<uint, long>>.Get(out var orderedColors);
            orderedColors.AddRange(colorMap);
            // Order in reverse order so the highest count is first
            orderedColors.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            var bestOrderedColors = orderedColors;
            bestOrderedColors.RemoveRange(5, bestOrderedColors.Count - 5);
            
            using var _2 = ListPool<ColorCluster>.Get(out var bestClusters);
            bestClusters.AddRange(rgbClusters.GetBestClusters(5));
            
            for (var i = bestOrderedColors.Count; i < 5; ++i)
            {
                bestOrderedColors.Add(new KeyValuePair<uint, long>(0, 0));
            }

            for (var i = 0; i < 5; ++i)
            {
                bestColors[i] = new ColorInfo { color = bestOrderedColors[i].Key, ratio = bestOrderedColors[i].Value / (double)nbPixels };
                bestShades[i] = new ColorInfo { color = ColorToInt(bestClusters[i].average), ratio = bestClusters[i].count / (double)nbPixels };
            }
        }

        struct LocalHistogramCount
        {
            public EdgeHistogram histogram;
            public int[] edgeCount;

            public LocalHistogramCount(EdgeHistogram histogram, int[] edgeCount)
            {
                this.histogram = histogram;
                this.edgeCount = edgeCount;
            }
        }

        public static void ComputeEdgesHistogramAndDensity(Color[] pixels, int width, int height, EdgeHistogram histogram, double[] edgeDensities)
        {
            var imagePixels = new ImagePixels(width, height, pixels);
            var filter = new SobelFilter(0.25f);
            var edgeImage = filter.Apply(imagePixels);
            var gradients = filter.gradients;

            var numChannels = histogram.channels;
            var edgeCount = new int[histogram.channels];

            var batchSize = ThreadUtils.GetBatchSizeByCore(edgeImage.pixels.Length);

            Parallel.ForEach(Partitioner.Create(0, edgeImage.pixels.Length, batchSize), () =>
            {
                return new LocalHistogramCount(new EdgeHistogram(), new int[numChannels]);
            }, (range, state, localData) =>
                {
                    for (var index = range.Item1; index < range.Item2; ++index)
                    {
                        var pixel = edgeImage.pixels[index];
                        for (var c = 0; c < numChannels; ++c)
                        {
                            var rad = gradients[index][c];
                            var deg = Mathf.Rad2Deg * rad;

                            // Edges have been thresholded and are either 1 or 0
                            if (pixel[c] >= 0.5f)
                            {
                                localData.histogram.AddEdge(c, deg);
                                ++localData.edgeCount[c];
                            }
                        }
                    }

                    return localData;
                }, localData =>
                {
                    lock (histogram)
                    {
                        histogram.Combine(localData.histogram);
                        for (var i = 0; i < numChannels; ++i)
                            edgeCount[i] += localData.edgeCount[i];
                    }
                });

            histogram.Normalize(edgeCount);
            for (var c = 0; c < edgeCount.Length; ++c)
            {
                edgeDensities[c] = edgeCount[c] / (double)edgeImage.pixels.Length;
            }
        }

        /// <summary>
        /// Returns the distance between two histograms. 0 is identical, 1 is completely different.
        /// </summary>
        /// <param name="histogramA">The first histogram.</param>
        /// <param name="histogramB">The second histogram.</param>
        /// <param name="model">The computation model.</param>
        /// <returns>A distance between 0 and 1.</returns>
        public static float HistogramDistance(IHistogram histogramA, IHistogram histogramB, HistogramDistance model)
        {
            switch (model)
            {
                case UnityExtensions.HistogramDistance.CityBlock: return CityBlockDistance(histogramA, histogramB);
                case UnityExtensions.HistogramDistance.Euclidean: return EuclideanDistance(histogramA, histogramB);
                case UnityExtensions.HistogramDistance.Bhattacharyya: return BhattacharyyaDistance(histogramA, histogramB);
                case UnityExtensions.HistogramDistance.MDPA: return MDPA(histogramA, histogramB);
            }

            return 1.0f;
        }

        public static float CityBlockDistance(IHistogram histogramA, IHistogram histogramB)
        {
            var distances = new float[histogramA.channels];
            for (var c = 0; c < histogramA.channels; ++c)
            {
                var binsA = histogramA.GetBins(c);
                var binsB = histogramB.GetBins(c);
                for (var i = 0; i < histogramA.bins; ++i)
                {
                    distances[c] += Mathf.Abs(binsA[i] - binsB[i]);
                }
            }

            // Values are between [0, 2], so divide by 2 to get [0, 1]
            return distances.Sum() / (2 * histogramA.channels);
        }

        public static float EuclideanDistance(IHistogram histogramA, IHistogram histogramB)
        {
            var distances = new float[histogramA.channels];
            for (var c = 0; c < histogramA.channels; ++c)
            {
                var binsA = histogramA.GetBins(c);
                var binsB = histogramB.GetBins(c);
                for (var i = 0; i < histogramA.bins; ++i)
                {
                    var diff = binsA[i] - binsB[i];
                    distances[c] += diff * diff;
                }
            }
            
            // Values are between [0, sqrt(2)], divide by sqrt(2) to get [0, 1]
            return Mathf.Sqrt(distances.Sum()) / (histogramA.channels * Mathf.Sqrt(2));
        }

        public static float BhattacharyyaDistance(IHistogram histogramA, IHistogram histogramB)
        {
            var distances = new float[histogramA.channels];
            for (var c = 0; c < histogramA.channels; ++c)
            {
                var binsA = histogramA.GetBins(c);
                var binsB = histogramB.GetBins(c);
                for (var i = 0; i < histogramA.bins; ++i)
                {
                    distances[c] += Mathf.Sqrt(binsA[i] * binsB[i]);
                }
            }

            // For the real distance, you would do D = -ln(BC), but this would give us
            // a distance between [0, INF]. Since we want a distance between [0, 1], keep the
            // values as is but invert them because they are similarity values.
            return 1 - distances.Sum() / histogramA.channels;
        }

        public static float MDPA(IHistogram histogramA, IHistogram histogramB)
        {
            var distances = new float[histogramA.channels];
            for (var c = 0; c < histogramA.channels; ++c)
            {
                var binsA = histogramA.GetBins(c);
                var binsB = histogramB.GetBins(c);

                for (var i = 0; i < histogramA.bins; ++i)
                {
                    var innerDistance = 0.0f;
                    for (var j = 0; j <= i; ++j)
                    {
                        innerDistance += binsA[j] - binsB[j];
                    }

                    distances[c] += Mathf.Abs(innerDistance);
                }
            }


            // Max distance is 255, so divide by 255 to get [0, 1]
            return distances.Sum() / (histogramA.channels * (histogramA.bins - 1));
        }

        readonly struct MomentOrder
        {
            public readonly int p;
            public readonly int q;

            public MomentOrder(int p, int q)
            {
                this.p = p;
                this.q = q;
            }
        }

        readonly struct Centroid
        {
            public readonly double x;
            public readonly double y;

            public Centroid(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
        }

        static IList<double[]> ComputeRawMoments(Color[] pixels, int width, int height, in IList<MomentOrder> momentOrders)
        {
            return ComputeCentralMoments(pixels, width, height, momentOrders, new Centroid(0, 0));
        }

        public static double[] ComputeRawMoment(Color[] pixels, int width, int height, int p, int q)
        {
            return ComputeRawMoments(pixels, width, height, new[] { new MomentOrder(p, q) })[0];
        }

        static IList<double[]> ComputeCentralMoments(Color[] pixels, int width, int height, in IList<MomentOrder> momentOrders, in Centroid centroid)
        {
            return ComputeCentralMoments_Parallel(pixels, width, height, momentOrders, new[] { centroid, centroid, centroid });
        }

        /// <exception cref="ArgumentException">Thrown if <paramref name="centroids"/> does not have a Count of 3.
        /// </exception>
        static IList<double[]> ComputeCentralMoments(Color[] pixels, int width, int height, in IList<MomentOrder> momentOrders, in IList<Centroid> centroids)
        {
            if (centroids.Count != 3)
                throw new ArgumentException("There should be 3 centroids, one for each channel", nameof(centroids));

            var allSums = new double[momentOrders.Count][];
            for (var i = 0; i < momentOrders.Count; ++i)
            {
                allSums[i] = new double[3];
            }

            for (var j = 0; j < height; ++j)
            {
                for (var i = 0; i < width; ++i)
                {
                    var currentPixel = pixels[j * width + i];
                    for (var l = 0; l < momentOrders.Count; ++l)
                    {
                        var momentOrder = momentOrders[l];
                        for (var k = 0; k < 3; ++k)
                        {
                            var diffX = 1.0;
                            var diffY = 1.0;
                            if (momentOrder.p > 0)
                            {
                                diffX = i - centroids[k].x;
                                diffX = Math.Pow(diffX, momentOrder.p);
                            }
                            if (momentOrder.q > 0)
                            {
                                diffY = j - centroids[k].y;
                                diffY = Math.Pow(diffY, momentOrder.q);
                            }

                            allSums[l][k] += diffX * diffY * currentPixel[k];
                        }
                    }
                }
            }

            return allSums;
        }

        /// <exception cref="ArgumentException">Thrown if <paramref name="centroids"/> does not have a Count of 3.
        /// </exception>
        static IList<double[]> ComputeCentralMoments_Parallel(Color[] pixels, int width, int height, IList<MomentOrder> momentOrders, IList<Centroid> centroids)
        {
            if (centroids.Count != 3)
                throw new ArgumentException("There should be 3 centroids, one for each channel", nameof(centroids));

            var allSums = new double[momentOrders.Count][];
            for (var i = 0; i < momentOrders.Count; ++i)
            {
                allSums[i] = new double[3];
            }

            var batchSize = ThreadUtils.GetBatchSizeByCore(height);
            Parallel.ForEach(Partitioner.Create(0, height, batchSize), () =>
            {
                var localSums = new double[momentOrders.Count][];
                for (var i = 0; i < momentOrders.Count; ++i)
                {
                    localSums[i] = new double[3];
                }

                return localSums;
            }, (range, state, localSums) =>
                {
                    for (var j = range.Item1; j < range.Item2; ++j)
                    {
                        for (var i = 0; i < width; ++i)
                        {
                            var currentPixel = pixels[j * width + i];
                            for (var l = 0; l < momentOrders.Count; ++l)
                            {
                                var momentOrder = momentOrders[l];
                                for (var k = 0; k < 3; ++k)
                                {
                                    var diffX = 1.0;
                                    var diffY = 1.0;
                                    if (momentOrder.p > 0)
                                    {
                                        diffX = i - centroids[k].x;
                                        diffX = Math.Pow(diffX, momentOrder.p);
                                    }

                                    if (momentOrder.q > 0)
                                    {
                                        diffY = j - centroids[k].y;
                                        diffY = Math.Pow(diffY, momentOrder.q);
                                    }

                                    localSums[l][k] += diffX * diffY * currentPixel[k];
                                }
                            }
                        }
                    }

                    return localSums;
                }, localSums =>
                {
                    lock (allSums)
                    {
                        for (var l = 0; l < momentOrders.Count; ++l)
                        {
                            for (var k = 0; k < 3; ++k)
                            {
                                allSums[l][k] += localSums[l][k];
                            }
                        }
                    }
                });

            return allSums;
        }

        static double[] ComputeCentralMoment(Color[] pixels, int width, int height, int p, int q, in List<Centroid> centroids)
        {
            return ComputeCentralMoments(pixels, width, height, new[] { new MomentOrder(p, q) }, centroids)[0];
        }

        static IList<Centroid> ComputeCentroidsAndAreas(Color[] pixels, int width, int height, out double[] areas)
        {
            var moments = ComputeRawMoments(pixels, width, height,
                new[] { new MomentOrder(0, 0), new MomentOrder(1, 0), new MomentOrder(0, 1) });

            var m00 = moments[0];
            var m10 = moments[1];
            var m01 = moments[2];
            var centroids = new Centroid[3];
            for (var i = 0; i < 3; ++i)
            {
                centroids[i] = new Centroid(m10[i] / m00[i], m01[i] / m00[i]);
            }

            areas = m00;
            return centroids;
        }

        static double[] CentralMomentToScaleInvariant(double[] centralMoment, double[] area, in MomentOrder momentOrder)
        {
            var scaleInvariant = new double[3];
            for (var i = 0; i < 3; ++i)
                scaleInvariant[i] = centralMoment[i] / Math.Pow(area[i], 1 + (momentOrder.p + momentOrder.q) / 2.0);
            return scaleInvariant;
        }

        public static void ComputeFirstOrderInvariant(Color[] pixels, int width, int height, double[] geometricMoments)
        {
            var centroids = ComputeCentroidsAndAreas(pixels, width, height, out var areas);
            var moment20 = new MomentOrder(2, 0);
            var moment02 = new MomentOrder(0, 2);
            var centralMoments =
                ComputeCentralMoments_Parallel(pixels, width, height, new[] { moment20, moment02 }, centroids);
            var u20 = centralMoments[0];
            var u02 = centralMoments[1];

            var n20 = CentralMomentToScaleInvariant(u20, areas, moment20);
            var n02 = CentralMomentToScaleInvariant(u02, areas, moment02);

            for (var i = 0; i < geometricMoments.Length; ++i)
            {
                geometricMoments[i] = n20[i] + n02[i];
            }
        }

        public static void ComputeSecondOrderInvariant(Color[] pixels, int width, int height, double[] geometricMoments)
        {
            var centroids = ComputeCentroidsAndAreas(pixels, width, height, out var areas);
            var moment20 = new MomentOrder(2, 0);
            var moment02 = new MomentOrder(0, 2);
            var moment11 = new MomentOrder(1, 1);
            var centralMoments = ComputeCentralMoments_Parallel(pixels, width, height,
                new[] { moment20, moment02, moment11 }, centroids);
            var u20 = centralMoments[0];
            var u02 = centralMoments[1];
            var u11 = centralMoments[2];

            var n20 = CentralMomentToScaleInvariant(u20, areas, moment20);
            var n02 = CentralMomentToScaleInvariant(u02, areas, moment02);
            var n11 = CentralMomentToScaleInvariant(u11, areas, moment11);

            var diff = new NativeArray<double>(n20.Length, Allocator.Temp);
            for (var i = 0; i < n20.Length; ++i)
            {
                diff[i] = n20[i] - n02[i];
            }

            for (var i = 0; i < geometricMoments.Length; ++i)
            {
                geometricMoments[i] = (diff[i] * diff[i]) + 4 * (n11[i] * n11[i]);
            }
            
            diff.Dispose();
        }

        public static MinMaxColor GetMinMax(ImagePixels image)
        {
            var min = new Color(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Color(float.MinValue, float.MinValue, float.MinValue);
            foreach (var pixel in image.pixels)
            {
                for (var c = 0; c < 3; ++c)
                {
                    if (pixel[c] > max[c])
                        max[c] = pixel[c];
                    if (pixel[c] < min[c])
                        min[c] = pixel[c];
                }
            }

            return new MinMaxColor(min, max);
        }

        public static ImagePixels StretchImage(ImagePixels image, float newMin, float newMax)
        {
            var newMinColor = new Color(newMin, newMin, newMin);
            var newColors = new Color[image.height * image.width];
            var minMax = GetMinMax(image);
            var scale = new Color();
            for (var i = 0; i < 3; ++i)
                scale[i] = (newMax - newMin) / (minMax.max[i] - minMax.min[i]);
            var index = 0;
            foreach (var pixel in image.pixels)
            {
                newColors[index] = (pixel - minMax.min) * scale + newMinColor;
                ++index;
            }

            return new ImagePixels(image.width, image.height, newColors);
        }
        #endregion // UnityEditor.Search
    }
}
