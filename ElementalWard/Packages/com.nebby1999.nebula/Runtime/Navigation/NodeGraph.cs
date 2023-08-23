using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Nebula.Navigation
{
    public class NodeGraph : MonoBehaviour
    {
        private class Mover
        {
            public GameObject obj;
            public CharacterController characterController;
            public Mover()
            {
                obj = new GameObject("NodeBakerCharacterController");
                characterController = obj.AddComponent<CharacterController>();


            }
        }
        private class NodeBaking : MonoBehaviour
        {
            public int nodeIndex;
        }
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
            ClearLinks();
            List<SerializedPathNode> nodes = GetSerializedNodes();
            var colliders = CreateCollidersForCollision(nodes);
            try
            {
                for(int i =  0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    BakeNode(node, i, nodes);
                }
            }
            finally
            {
                foreach(Collider collider in colliders)
                {
                    DestroyImmediate(collider, true);
                }
            }
        }

        public void Clear()
        {
            graphAsset.serializedNodes.Clear();
            graphAsset.serializedLinks.Clear();
        }

        private Collider[] CreateCollidersForCollision(List<SerializedPathNode> nodes)
        {
            List<Collider> result = new List<Collider>();
            for(int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                GameObject colliderObj = new GameObject("TempNodeCollider_"+i);
                colliderObj.hideFlags = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
                colliderObj.transform.position = node.worldPosition;
                var collider = colliderObj.AddComponent<BoxCollider>();
                colliderObj.AddComponent<NodeBaking>().nodeIndex = i;
                result.Add(collider);
            }
            return result.ToArray();
        }

        private void ClearLinks()
        {
            foreach (var node in GetSerializedNodes())
            {
                node.serializedPathNodeLinkIndices.Clear();
            }
            graphAsset.serializedLinks.Clear();
        }

        private void BakeNode(SerializedPathNode node, int nodeIndex, List<SerializedPathNode> nodes)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                var otherNodeIndex = i;
                if (i == nodeIndex)
                    continue;

                var otherNode = nodes[i];

                if (math.any(math.isnan(node.worldPosition)) || math.any(math.isnan(otherNode.worldPosition)))
                    continue;

                float distance = Vector3.Distance(node.worldPosition, otherNode.worldPosition);
                if (distance > SerializedPathNode.MAX_DISTANCE)
                    continue;

                Vector3 direction = otherNode.worldPosition - node.worldPosition;
                direction = direction.normalized;
                var raycastHits = Physics.RaycastAll(node.worldPosition, direction, distance, Physics.AllLayers, QueryTriggerInteraction.Collide);
                bool shouldContinue = false;
                if (raycastHits.Length > 0)
                {
                    foreach(var hit in raycastHits)
                    {
                        //A wall was hit, dont build link
                        if(!hit.collider.TryGetComponent<NodeBaking>(out var nodeBaking) && hit.collider.gameObject.layer == LayerMask.NameToLayer("World") && Vector3.Angle(hit.normal.normalized, direction) >= 90)
                        {
                            shouldContinue = true;
                            break;
                        }

                        if (nodeBaking.nodeIndex == nodeIndex)
                            continue;

                        otherNode = nodes[nodeBaking.nodeIndex];
                        otherNodeIndex = nodeBaking.nodeIndex;
                        break;
                    }
                    if (shouldContinue)
                        continue;
                }

                shouldContinue = false;
                //the current node and "otherNode" are found, see if any existing links exists, if so, just reuse it.
                for (int linkIndex = 0; linkIndex < graphAsset.serializedLinks.Count; linkIndex++)
                {
                    SerializedPathNodeLink link = graphAsset.serializedLinks[linkIndex];
                    bool nodeAValid = link.nodeAIndex == nodeIndex || link.nodeAIndex == otherNodeIndex;
                    bool nodeBValid = link.nodeBIndex == nodeIndex || link.nodeBIndex == otherNodeIndex;

                    if (nodeAValid && nodeBValid)
                    {
                        if(!node.serializedPathNodeLinkIndices.Contains(linkIndex))
                        {
                            node.serializedPathNodeLinkIndices.Add(linkIndex);
                        }
                        shouldContinue = true;
                        break;
                    }
                }
                if (shouldContinue)
                    continue;

                var newLink = new SerializedPathNodeLink
                {
                    distance = math.distance(node.worldPosition, otherNode.worldPosition),
                    normal = math.normalize(node.worldPosition - otherNode.worldPosition),
                    nodeBIndex = otherNodeIndex,
                    nodeAIndex = nodeIndex
                };

                graphAsset.serializedLinks.Add(newLink);
                var newLinkIndex = graphAsset.serializedLinks.IndexOf(newLink);
                node.serializedPathNodeLinkIndices.Add(newLinkIndex);
            }
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
                    ClearLinks();
            }
        }
    }
}