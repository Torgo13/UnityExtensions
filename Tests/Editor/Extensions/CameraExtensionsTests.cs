using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace PKGE.Tests
{
    public class CameraExtensionsTests
    {
        Camera cam;

        [SetUp]
        public void SetUp()
        {
            cam = new GameObject("TestCam").AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.aspect = 16f / 9f;
        }

        [TearDown]
        public void TearDown()
        {
            if (cam != null)
                UnityEngine.Object.DestroyImmediate(cam.gameObject);

            // Cleanup any other cameras created during the test
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Camera>())
            {
                if (c != Camera.main)
                    UnityEngine.Object.DestroyImmediate(c.gameObject);
            }
        }

        [Test]
        public void GetVerticalFieldOfView_MatchesExpected()
        {
            float diagFov = 90f;
            float vFov = cam.GetVerticalFieldOfView(diagFov);

            // Vertical FoV should be less than diagonal FoV
            Assert.Less(vFov, diagFov);
        }

        [Test]
        public void GetVerticalFieldOfViewRad_MatchesFloatVersion()
        {
            double diagFovRad = Mathf.Deg2Rad * 90f;
            double vfovRad = cam.GetVerticalFieldOfViewRad(diagFovRad);
            float vfovDegFromFloat = cam.GetVerticalFieldOfView(90f) * Mathf.Deg2Rad;

            Assert.That(vfovRad, Is.EqualTo(vfovDegFromFloat).Within(1e-5));
        }

        [Test]
        public void GetHorizontalFieldOfView_And_Rad_Agree()
        {
            float hFovDeg = cam.GetHorizontalFieldOfView();
            double hFovRad = cam.GetHorizontalFieldOfViewRad();

            Assert.That(Mathf.Deg2Rad * hFovDeg, Is.EqualTo(hFovRad).Within(1e-5));
        }

        [Test]
        public void GetVerticalOrthographicSize_ComputesExpected()
        {
            cam.orthographic = true;
            float size = cam.GetVerticalOrthographicSize(10f);

            // Should be proportional to aspect ratio
            Assert.AreEqual(10f * 0.707106781f / Mathf.Sqrt(cam.aspect), size, 1e-6f);
        }

        [Test]
        public void HorizontalToVerticalFOVRad_MatchesManualCalc()
        {
            double hFov = Mathf.Deg2Rad * 90f;
            double aspect = 16.0 / 9.0;

            double vFovRad = CameraExtensions.HorizontalToVerticalFOVRad(hFov, aspect);
            double manual = 2.0 * System.Math.Atan(System.Math.Tan(hFov / 2.0) / aspect);

            Assert.AreEqual(manual, vFovRad, 1e-10);
        }

        [Test]
        public void GetTopCamera_ReturnsCameraWithHighestDepth()
        {
            var camA = cam;
            camA.depth = 0;

            var camB = new GameObject("HigherDepth").AddComponent<Camera>();
            camB.depth = 5;

            var topCam = CameraExtensions.GetTopCamera();
            Assert.AreSame(camB, topCam);
        }

        [Test]
        public void GetAllCameras_PopulatesList()
        {
            var list = new List<Camera>();
            bool found = CameraExtensions.GetAllCameras(list);

            Assert.IsTrue(found);
            Assert.IsTrue(list.Count >= 1);
        }

        [Test]
        public void GetFirstCamera_ReturnsFirstInList()
        {
            var first = CameraExtensions.GetFirstCamera();
            Assert.IsNotNull(first);
        }

        [Test]
        public void GetCamera_ByName_ReturnsMatch()
        {
            cam.name = "SpecialCam";
            var result = CameraExtensions.GetCamera("SpecialCam");
            Assert.AreSame(cam, result);
        }

        [Test]
        public void GetCamera_ByTag_ReturnsMatch()
        {
            cam.tag = "MainCamera";
            var result = CameraExtensions.GetCamera("MainCamera", compareTag: true);
            Assert.IsTrue(result.CompareTag(cam.tag));
        }

        [Test]
        public void GetCamera_NoMatch_ReturnsNull()
        {
            var result = CameraExtensions.GetCamera("DoesNotExist");
            Assert.IsNull(result);
        }
    }
}
