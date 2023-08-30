using System;
using Unity.Mathematics;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// Represents a Serialized version of a Link between two Path nodes
    /// </summary>
    [Serializable]
    public class SerializedPathNodeLink
    {
        /// <summary>
        /// The index of one of the node of this path node link, defaults to -1
        /// </summary>
        public int nodeAIndex = -1;
        /// <summary>
        /// The index of the other node of this path node link, defaults to -1
        /// </summary>
        public int nodeBIndex = -1;
        /// <summary>
        /// the distance between the Start and End node, defaults to NAN
        /// </summary>
        public float distance = float.NaN;
        /// <summary>
        /// The angle of this path node during baking calculation;
        /// </summary>
        public float slopeAngle = float.NaN;
        /// <summary>
        /// The normal direction of the link, This normal direction is calculated from <see cref="nodeAIndex"/> to <see cref="nodeBIndex"/>. defaults to a float3 with XYZ components set to NAN
        /// </summary>
        public float3 normal = new float3(math.NAN, math.NAN, math.NAN);
    }

    public struct RuntimePathNodeLink
    {
        public int linkIndex;
        public int nodeAIndex;
        public int nodeBIndex;
        public float distanceCost;
        public float slopeAngle;
        public float3 normal;
    }
}