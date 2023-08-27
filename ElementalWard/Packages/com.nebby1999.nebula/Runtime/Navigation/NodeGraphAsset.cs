using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Nebula.Navigation
{
    /// <summary>
    /// Represents a NodeGraphAsset for Grounded travel, you can inherit this asset for different types of travel (IE: an "Air" Graph)
    /// </summary>
    [CreateAssetMenu(menuName = "Nebula/Navigation/NodeGraphAsset")]
    public class NodeGraphAsset : NebulaScriptableObject
    {
        /// <summary>
        /// An offset thats used by the node placer/painter when adding new nodes to this graph asset.
        /// </summary>
        public virtual Vector3 NodeOffset => Vector3.zero;
        public List<SerializedPathNode> serializedNodes = new List<SerializedPathNode>();
        public List<SerializedPathNodeLink> serializedLinks = new List<SerializedPathNodeLink>();

        /// <summary>
        /// Clears all nodes and links
        /// </summary>
        public virtual void Clear()
        {
            serializedNodes.Clear();
            serializedLinks.Clear();
        }

        /// <summary>
        /// Clears all the serializedLinks from <see cref="serializedLinks"/> and <see cref="serializedNodes"/>'s serializedLinkIndices.
        /// </summary>
        public virtual void ClearLinks()
        {
            foreach(var node in serializedNodes)
            {
                node.serializedPathNodeLinkIndices.Clear();
            }
            serializedLinks.Clear();
        }

        public virtual NodeBaker GetBaker()
        {
            return new NodeBaker(this, SerializedPathNode.MAX_DISTANCE);
        }
    }
}