using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PKGE
{
    public static class RectTransformExtensions
    {
        //https://github.com/Unity-Technologies/arfoundation-samples/blob/7eaa39b7bd6fc9a540949188e6aa45a849fc7de7/Assets/Scripts/Runtime/UI/RectTransformExtensions.cs
        #region UnityEngine.XR.ARFoundation.Samples
        static readonly Vector2 s_TopCenterAnchorPosition = new Vector2(.5f, 1);

        public static void SetSize(this RectTransform rt, Vector2 size)
        {
            var oldSize = rt.rect.size;
            var deltaSize = size - oldSize;
            var pivot = rt.pivot;
            rt.offsetMin -= new Vector2(deltaSize.x * pivot.x, deltaSize.y * pivot.y);
            rt.offsetMax += new Vector2(deltaSize.x * (1f - pivot.x), deltaSize.y * (1f - pivot.y));
        }

        public static float GetWidth(this RectTransform rt)
        {
            return rt.rect.width;
        }

        public static void SetWidth(this RectTransform rt, float width)
        {
            SetSize(rt, new Vector2(width, rt.rect.size.y));
        }

        public static float GetHeight(this RectTransform rt)
        {
            return rt.rect.height;
        }

        public static void SetHeight(this RectTransform rt, float height)
        {
            SetSize(rt, new Vector2(rt.rect.size.x, height));
        }

        public static void SetTopLeftPosition(this RectTransform rt, Vector2 pos)
        {
            rt.localPosition = new Vector3(
                pos.x + (rt.pivot.x * rt.rect.width),
                pos.y - ((1f - rt.pivot.y) * rt.rect.height),
                rt.localPosition.z);
        }

        public static void SetTopCenterPosition(this RectTransform rt, Vector2 pos)
        {
            rt.localPosition = new Vector3(
                pos.x + (rt.pivot.x * rt.rect.width / 2),
                pos.y - ((1f - rt.pivot.y) * rt.rect.height),
                rt.localPosition.z);
        }

        public static void TranslateX(this RectTransform rt, float x)
        {
            Vector3 pos = rt.localPosition;
            rt.localPosition = new Vector3(
                pos.x + x,
                pos.y,
                pos.z);
        }

        public static void TranslateY(this RectTransform rt, float y)
        {
            Vector3 pos = rt.localPosition;
            rt.localPosition = new Vector3(
                pos.x,
                pos.y + y,
                pos.z);
        }

        public static void Translate(this RectTransform rt, Vector2 translation)
        {
            Vector3 pos = rt.localPosition;
            rt.localPosition = new Vector3(
                pos.x + translation.x,
                pos.y + translation.y,
                pos.z);
        }

        public static void AnchorToTopCenter(this RectTransform rt)
        {
            rt.anchorMin = s_TopCenterAnchorPosition;
            rt.anchorMax = s_TopCenterAnchorPosition;
        }
        #endregion // UnityEngine.XR.ARFoundation.Samples

        public static void GetWorldCorners(this RectTransform rectTransform, System.Span<Vector3> fourCornersArray)
        {
            if (fourCornersArray == null || fourCornersArray.Length < 4)
            {
                Debug.LogError("Calling GetWorldCorners with an array that is null or has less than 4 elements.");
                return;
            }

            GetLocalCorners(rectTransform.rect, fourCornersArray);
            Matrix4x4 matrix4x = rectTransform.localToWorldMatrix;
            for (int i = 0; i < 4; i++)
            {
                fourCornersArray[i] = matrix4x.MultiplyPoint(fourCornersArray[i]);
            }
        }

        public static void GetWorldCorners(this RectTransform rectTransform, Unity.Collections.NativeArray<Vector3> fourCornersArray)
        {
            UnityEngine.Assertions.Assert.IsTrue(fourCornersArray.IsCreated);
            UnityEngine.Assertions.Assert.IsTrue(fourCornersArray.Length >= 4);
            UnityEngine.Assertions.Assert.IsNotNull(rectTransform);

            var fourCornersJob = new FourCornersJob
            {
                fourCornersArray = fourCornersArray,
                matrix4x = rectTransform.localToWorldMatrix,
                rect = rectTransform.rect,
            };

            Unity.Jobs.IJobExtensions.RunByRef(ref fourCornersJob);
        }

        private static void GetLocalCorners(Rect rect, System.Span<Vector3> fourCornersArray)
        {
            float x = rect.x;
            float y = rect.y;
            float xMax = rect.xMax;
            float yMax = rect.yMax;
            fourCornersArray[0] = new Vector3(x, y, 0f);
            fourCornersArray[1] = new Vector3(x, yMax, 0f);
            fourCornersArray[2] = new Vector3(xMax, yMax, 0f);
            fourCornersArray[3] = new Vector3(xMax, y, 0f);
        }

        [Unity.Burst.BurstCompile]
        struct FourCornersJob : Unity.Jobs.IJob
        {
            [Unity.Collections.NativeFixedLength(4)]
            public Unity.Collections.NativeArray<Vector3> fourCornersArray;
            public Matrix4x4 matrix4x;
            public Rect rect;

            public void Execute()
            {
                GetLocalCorners(rect, fourCornersArray);
                for (int i = 0; i < 4; i++)
                {
                    fourCornersArray[i] = matrix4x.MultiplyPoint(fourCornersArray[i]);
                }
            }
        }
    }
}
