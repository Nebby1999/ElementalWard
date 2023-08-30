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
    public class NodeGraph : MonoBehaviour
    { 
        public NodeGraphAsset graphAsset;

        public void AddNewNode(Vector3 position, bool useOffset)
        {
            if (!graphAsset)
                return;

            if (useOffset)
                position += graphAsset.NodeOffset;

            var serializedNode = new SerializedPathNode
            {
                worldPosition = position,
            };

            graphAsset.serializedNodes.Add(serializedNode);
        }

        public List<SerializedPathNode> GetSerializedNodes()
        {
            return graphAsset ? graphAsset.serializedNodes : new List<SerializedPathNode>();
        }

        public List<SerializedPathNodeLink> GetSerializedPathNodeLinks()
        {
            return graphAsset ? graphAsset.serializedLinks : new List<SerializedPathNodeLink>();
        }

        public Vector3 GetNodeOffset()
        {
            return graphAsset ? graphAsset.NodeOffset : Vector3.zero;
        }

        public void Bake()
        {
            if (!graphAsset)
                return;

            var baker = graphAsset.GetBaker();
            baker.BakeNodes();
            baker.CommitBakedNodes();
        }

        public void Clear()
        {
            if (!graphAsset)
                return;

            graphAsset.Clear();
        }

        
        public void RemoveNearest(Vector3 position)
        {
            if (!graphAsset)
                return;

            var nodes = GetSerializedNodes();
            int nearest = -1;
            float nearestDistance = float.MaxValue;
            for(int i = 0; i <  nodes.Count; i++)
            {
                var node = nodes[i];
                var dist = Vector3.Distance(node.worldPosition, position);
                if(dist < nearestDistance)
                {
                    nearest = i;
                    nearestDistance = dist;
                }
            }

            if(nearest != -1)
            {
                var node = nodes[nearest];

                nodes.RemoveAt(nearest);
                if(node.serializedPathNodeLinkIndices.Count > 0)
                    graphAsset.ClearLinks();
            }
        }

        public void OnDrawGizmos()
        {
        }
    }
}