using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Unity.Collections;
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

        public bool HasValidRuntimeNodesAndLinks
        {
            get
            {
                return _nodes != null && _nodes.Length > 0 && _links != null && _links.Length > 0;
            }
        }

        public RuntimePathNode[] RuntimeNodes
        {
            get
            {
                if(!HasValidRuntimeNodesAndLinks)
                {
                    UpdateRuntimeNodesAndLinks();
                }
                return _nodes;
            }
        }
        private RuntimePathNode[] _nodes = Array.Empty<RuntimePathNode>();
        public RuntimePathNodeLink[] RuntimeLinks
        {
            get
            {
                if(!HasValidRuntimeNodesAndLinks)
                {
                    UpdateRuntimeNodesAndLinks();
                }
                return _links;
            }
        }
        private RuntimePathNodeLink[] _links = Array.Empty<RuntimePathNodeLink>();

        private void UpdateRuntimeNodesAndLinks()
        {
            _nodes = new RuntimePathNode[serializedNodes.Count];
            for(int i = 0; i < _nodes.Length; i++)
            {
                var serialized = serializedNodes[i];
                var runtimeNode = new RuntimePathNode
                {
                    nodeIndex = i,
                    worldPosition = serialized.worldPosition,
                    pathNodeLinkIndices = new FixedList4096Bytes<int>(),
                    gCost = float.MaxValue,
                    hCost = 0,
                    parentLinkIndex = -1
                };
                for(int j = 0; j < serialized.serializedPathNodeLinkIndices.Count; j++)
                {
                    runtimeNode.pathNodeLinkIndices.Add(serialized.serializedPathNodeLinkIndices[j]);
                }
                _nodes[i] = runtimeNode;
            }

            _links = new RuntimePathNodeLink[serializedLinks.Count];
            for(int i = 0; i < _links.Length; i++)
            {
                var serialized = serializedLinks[i];
                var runtime = new RuntimePathNodeLink
                {
                    linkIndex = i,
                    nodeAIndex = serialized.nodeAIndex,
                    nodeBIndex = serialized.nodeBIndex,
                    slopeAngle = serialized.slopeAngle,
                    distanceCost = serialized.distance,
                    normal = serialized.normal,
                };
                _links[i] = runtime;
            }
        }
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