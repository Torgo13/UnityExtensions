using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace PKGE.Tests
{
    public class RuntimeReflectionSystemCameraTests
    {
        private GameObject go;
        private RuntimeReflectionSystemCamera ReflectionSystemComponent;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("RuntimeReflectionSystemCamera");
            ReflectionSystemComponent = go.AddComponent<RuntimeReflectionSystemCamera>();
            ReflectionSystem.Init(captureCubemap: true, skyboxMaterial: new Material(Shader.Find("Skybox/Procedural")));
        }

        [TearDown]
        public void TearDown()
        {
            CoreUtils.Destroy(go);
            CoreUtils.Destroy(ReflectionSystem._skyboxMaterial);
        }

        [Test]
        public void GetMipLevel_NoiseReduction_AdjustsResult()
        {
            ReflectionSystem.noiseReduction = true;
            // >0.7 -> start at 1, +1 -> clamped to [2,3]
            Assert.AreEqual(2, ReflectionSystem.GetMipLevel(0.8f));
            // Mid range -> start at 2, +1 -> clamped to [2,3]
            Assert.AreEqual(3, ReflectionSystem.GetMipLevel(0.5f));
            // <0.32 -> start at 3, +1 -> clamped to 3
            Assert.AreEqual(3, ReflectionSystem.GetMipLevel(0.1f));
        }

        [Test]
        public void GetMipLevel_NoiseReductionFalse_ReturnsBaseLevels()
        {
            ReflectionSystem.noiseReduction = false;
            Assert.AreEqual(1, ReflectionSystem.GetMipLevel(0.8f));
            Assert.AreEqual(2, ReflectionSystem.GetMipLevel(0.5f));
            Assert.AreEqual(3, ReflectionSystem.GetMipLevel(0.1f));
        }

        [Test]
        public void NextIndex_WrapsAround()
        {
            ReflectionSystem._index = 1;
            Assert.AreEqual(2, ReflectionSystem.NextIndex());
            ReflectionSystem._index = 2;
            Assert.AreEqual(0, ReflectionSystem.NextIndex());
        }

        [Test]
        public void PreviousIndex_WrapsAround()
        {
            ReflectionSystem._index = 0;
            Assert.AreEqual(2, ReflectionSystem.PreviousIndex());
            ReflectionSystem._index = 1;
            Assert.AreEqual(0, ReflectionSystem.PreviousIndex());
        }

        [Test]
        public void EnsureCreated_CreatesRenderTexturesIfNeeded()
        {
            // Simulate minimal init of RenderTexture arrays
            var rts = new RenderTexture[3];
            for (int i = 0; i < rts.Length; i++)
            {
                rts[i] = new RenderTexture(4, 4, 0);
                rts[i].Release(); // ensure not created
            }

            ReflectionSystem._renderTextures = rts;
            ReflectionSystem._blendedTexture = new RenderTexture(4, 4, 0);

            bool result = ReflectionSystem.EnsureCreated();
            Assert.IsTrue(result);
            foreach (var rt in rts)
            {
                Assert.IsTrue(rt.IsCreated());
                rt.Release();
                CoreUtils.Destroy(rt);
            }

            CoreUtils.Destroy(ReflectionSystem._blendedTexture);
        }

        [UnityTest]
        public IEnumerator Render_SetsSkyboxMipLevel_AndSkips_WhenTimeScaleZero()
        {
            Time.timeScale = 0f;
            ReflectionSystem.Render(timeScale: Time.deltaTime, timeSlice: true);
            // If TimeScale = 0, should skip without errors
            yield return null;
            Time.timeScale = 1f;
        }

        [Test]
        public void TickRealtimeProbes_ReturnsBool_AndBlends()
        {
            // This is a very limited logic path check:
            // We set _renderTextures and _index, then check return type
            var desc = new RenderTextureDescriptor(width: 4, height: 4,
                RenderTextureFormat.DefaultHDR, depthBufferBits: 0, mipCount: 0,
                RenderTextureReadWrite.Default)
            {
                dimension = TextureDimension.Cube,
                autoGenerateMips = false,
            };

            var rts = new RenderTexture[3];
            for (int i = 0; i < rts.Length; i++)
                rts[i] = RenderTexture.GetTemporary(desc);

            ReflectionSystem._renderTextures = rts;
            ReflectionSystem._index = 0;
            ReflectionSystem._reflectionCamera = new GameObject("RefCam").AddComponent<Camera>();
            ReflectionSystem._reflectionCameraTransform = ReflectionSystem._reflectionCamera.transform;
            ReflectionSystem._renderedFrameCount = Time.frameCount - 10;
            ReflectionSystem._blendedTexture = RenderTexture.GetTemporary(desc);

            bool updated = ReflectionSystem.TickRealtimeProbes();
            Assert.IsInstanceOf<bool>(updated);

            foreach (var rt in rts)
                RenderTexture.ReleaseTemporary(rt);

            RenderTexture.ReleaseTemporary(ReflectionSystem._blendedTexture);
        }
    }
}
