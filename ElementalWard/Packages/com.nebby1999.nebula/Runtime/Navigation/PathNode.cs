using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// Represents a Serialized version of a Navigation path node
    /// </summary>
    [Serializable]
    public class SerializedPathNode
    {
        /// <summary>
        /// The max distance between any node
        /// </summary>
        public const float MAX_DISTANCE = 15f;

        /// <summary>
        /// The world position of this path node, defaults to a Vector3 with XYZ components set to NAN
        /// </summary>
        public Vector3 worldPosition = new Vector3(math.NAN, math.NAN, math.NAN);

        /// <summary>
        /// represents an array of intergers that represent node links.
        /// </summary>
        public List<int> serializedPathNodeLinkIndices = new List<int>();
    }

    /// <summary>
    /// Represents a Runtime version of a Navigation path node, usually created at runtime or from a <see cref="SerializedPathNode"/>
    /// </summary>
    public struct RuntimePathNode
    {
        /// <summary>
        /// The world position of this path node, defaults to a float3 with XYZ components set to NAN
        /// </summary>
        public float3 worldPosition;
    }
}