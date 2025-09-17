using System.Xml;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
#else
using float3 = UnityEngine.Vector3;
using quaternion = UnityEngine.Quaternion;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Packages
{
    public static class XmlUtility
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Runtime/Supplementary/Utility/XmlUtility.cs
        #region Unity.Kinematica
        public static void CreateAttribute(this XmlNode node, string name, string value)
        {
            var document = node.OwnerDocument;
            var attribute = document.CreateAttribute(name);
            attribute.Value = value;
            _ = node.Attributes.Append(attribute);
        }

        public static void CreateAttribute(this XmlNode node, string name, int value)
        {
            node.CreateAttribute(name, value.ToString());
        }

        public static void CreateAttribute(this XmlNode node, string name, float value)
        {
            node.CreateAttribute(name, value.ToString());
        }

        public static void CreateVector3Node(this XmlNode parentNode, string name, float3 position)
        {
            var document = parentNode.OwnerDocument;
            var node = document.CreateElement(name);
            node.CreateAttribute("x", position.x);
            node.CreateAttribute("y", position.y);
            node.CreateAttribute("z", position.z);
            _ = parentNode.AppendChild(node);
        }

        public static void CreateQuaternionNode(this XmlNode parentNode, string name, quaternion rotation)
        {
            var document = parentNode.OwnerDocument;
            var node = document.CreateElement(name);
#if INCLUDE_MATHEMATICS
            node.CreateAttribute("x", rotation.value.x);
            node.CreateAttribute("y", rotation.value.y);
            node.CreateAttribute("z", rotation.value.z);
            node.CreateAttribute("w", rotation.value.w);
#else
            node.CreateAttribute("x", rotation.x);
            node.CreateAttribute("y", rotation.y);
            node.CreateAttribute("z", rotation.z);
            node.CreateAttribute("w", rotation.w);
#endif // INCLUDE_MATHEMATICS
            _ = parentNode.AppendChild(node);
        }
        #endregion // Unity.Kinematica
    }
}
