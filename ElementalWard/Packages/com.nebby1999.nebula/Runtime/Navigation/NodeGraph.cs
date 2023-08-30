using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// Represents a graph of nodes on a scene, can also add and remove nodes from its nodeGraphAsset
    /// </summary>
    public class NodeGraph : MonoBehaviour, IGraphProvider
    { 
        public NodeGraphAsset NodeGraphAsset => _graphAsset;
        [SerializeField] private NodeGraphAsset _graphAsset;


        public void AddNewNode(Vector3 position)
        {
            if (!NodeGraphAsset)
                return;

                position += NodeGraphAsset.NodeOffset;

            var serializedNode = new SerializedPathNode
            {
                worldPosition = position,
            };

            NodeGraphAsset.serializedNodes.Add(serializedNode);
        }

        public void Bake()
        {
            if (!NodeGraphAsset)
                return;

            var baker = NodeGraphAsset.GetBaker();
            baker.BakeNodes();
            baker.CommitBakedNodes();
        }

        public void Clear()
        {
            if (!NodeGraphAsset)
                return;

            NodeGraphAsset.Clear();
        }

        public RuntimePathNodeLink[] GetRuntimePathNodeLinks()
        {
            if (!NodeGraphAsset)
                return Array.Empty<RuntimePathNodeLink>();

            return NodeGraphAsset.RuntimeLinks;
        }

        public RuntimePathNode[] GetRuntimePathNodes()
        {
            if(!NodeGraphAsset)
                return Array.Empty<RuntimePathNode>();

            return NodeGraphAsset.RuntimeNodes;
        }

        public List<SerializedPathNodeLink> GetSerializedPathLinks()
        {
            if (!NodeGraphAsset)
                return new List<SerializedPathNodeLink>();

            return NodeGraphAsset.serializedLinks;
        }

        public List<SerializedPathNode> GetSerializedPathNodes()
        {
            if (!NodeGraphAsset)
                return new List<SerializedPathNode>();

            return NodeGraphAsset.serializedNodes;
        }

        public void RemoveNearestNode(Vector3 position)
        {
            if (!NodeGraphAsset)
                return;

            var nodes = GetSerializedPathNodes();
            int nearest = -1;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var dist = Vector3.Distance(node.worldPosition, position);
                if (dist < nearestDistance)
                {
                    nearest = i;
                    nearestDistance = dist;
                }
            }

            if (nearest != -1)
            {
                var node = nodes[nearest];

                nodes.RemoveAt(nearest);
                if (node.serializedPathNodeLinkIndices.Count > 0)
                    NodeGraphAsset.ClearLinks();
            }
        }
    }
}