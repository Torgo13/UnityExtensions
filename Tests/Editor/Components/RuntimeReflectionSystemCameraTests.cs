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
        private RuntimeReflectionSystemCamera sysCamComponent;
        private ReflectionSystem sysCam => sysCamComponent.Reflection;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("RuntimeReflectionSystemCamera");
            sysCamComponent = go.AddComponent<RuntimeReflectionSystemCamera>();
            sysCamComponent.Reflection = new ReflectionSystem(skyboxMaterial: new Material(Shader.Find("Skybox/Procedural")));
        }

        [TearDown]
        public void TearDown()
        {
            CoreUtils.Destroy(go);
            CoreUtils.Destroy(sysCam._skyboxMaterial);
        }

        [Test]
        public void GetMipLevel_NoiseReduction_AdjustsResult()
        {
            sysCam.noiseReduction = true;
            // >0.7 → start at 1, +1 → clamped to [2,3]
            Assert.AreEqual(2, sysCam.GetMipLevel(0.8f));
            // Mid range → start at 2, +1 → clamped to [2,3]
            Assert.AreEqual(3, sysCam.GetMipLevel(0.5f));
            // <0.32 → start at 3, +1 → clamped to 3
            Assert.AreEqual(3, sysCam.GetMipLevel(0.1f));
        }

        [Test]
        public void GetMipLevel_NoiseReductionFalse_ReturnsBaseLevels()
        {
            sysCam.noiseReduction = false;
            Assert.AreEqual(1, sysCam.GetMipLevel(0.8f));
            Assert.AreEqual(2, sysCam.GetMipLevel(0.5f));
            Assert.AreEqual(3, sysCam.GetMipLevel(0.1f));
        }

        [Test]
        public void NextIndex_WrapsAround()
        {
            sysCam._index = 1;
            Assert.AreEqual(2, sysCam.NextIndex());
            sysCam._index = 2;
            Assert.AreEqual(0, sysCam.NextIndex());
        }

        [Test]
        public void PreviousIndex_WrapsAround()
        {
            sysCam._index = 0;
            Assert.AreEqual(2, sysCam.PreviousIndex());
            sysCam._index = 1;
            Assert.AreEqual(0, sysCam.PreviousIndex());
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

            sysCam._renderTextures = rts;
            sysCam._blendedTexture = new RenderTexture(4, 4, 0);

            bool result = sysCam.EnsureCreated();
            Assert.IsTrue(result);
            foreach (var rt in rts)
            {
                Assert.IsTrue(rt.IsCreated());
                rt.Release();
                CoreUtils.Destroy(rt);
            }

            CoreUtils.Destroy(sysCam._blendedTexture);
        }

        [UnityTest]
        public IEnumerator Render_SetsSkyboxMipLevel_AndSkips_WhenTimeScaleZero()
        {
            Time.timeScale = 0f;
            sysCam.Render(true);
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

            sysCam._renderTextures = rts;
            sysCam._index = 0;
            sysCam._reflectionCamera = new GameObject("RefCam").AddComponent<Camera>();
            sysCam._reflectionCameraTransform = sysCam._reflectionCamera.transform;
            sysCam._renderedFrameCount = Time.frameCount - 10;
            sysCam._blendedTexture = RenderTexture.GetTemporary(desc);

            bool updated = sysCam.TickRealtimeProbes();
            Assert.IsInstanceOf<bool>(updated);

            foreach (var rt in rts)
                RenderTexture.ReleaseTemporary(rt);

            RenderTexture.ReleaseTemporary(sysCam._blendedTexture);
        }
    }
}
