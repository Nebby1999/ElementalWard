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

        /*/// <summary>
        /// Represents a basic CharacterController that moves to nodes to check if they can be traversed or not.
        /// </summary>
        protected class GroundMover : IDisposable
        {
            public Vector3 StartPosition => startPositionNode.worldPosition;
            public Vector3 StartPositionXZ => new(StartPosition.x, 0, StartPosition.z);
            public SerializedPathNode StartPositionNode => startPositionNode;
            public int StartPositionNodeIndex => startPositionNodeIndex;
            protected SerializedPathNode startPositionNode;
            protected int startPositionNodeIndex;

            public Vector3 DestinationPosition => destinationNode.worldPosition;
            public Vector3 DestinationPositionXZ => new(DestinationPosition.x, 0, DestinationPosition.z);
            public SerializedPathNode DestinationPositionNode => destinationNode;
            public int DestinationNodeIndex => destinationNodeIndex;
            protected SerializedPathNode destinationNode;
            protected int destinationNodeIndex;

            public Vector3 MoverPosition => gameObject.transform.position;
            public Vector3 MoverPositionXZ => new(MoverPosition.x, 0, MoverPosition.z);
            protected GameObject gameObject;
            protected CharacterController characterController;
            protected List<SerializedPathNode> nodeCollection;

            public virtual void SetNodeCollection(List<SerializedPathNode> nodeCollection)
            {
                this.nodeCollection = nodeCollection;
            }
            public virtual void SetPositionAndDestination(SerializedPathNode positionNode, int positionNodeIndex, SerializedPathNode destinationNode, int destinationNodeIndex)
            {
                this.startPositionNode = positionNode;
                this.startPositionNodeIndex = positionNodeIndex;
                this.destinationNode = destinationNode;
                this.destinationNodeIndex = destinationNodeIndex;
            }

            /// <summary>
            /// Returns true if the Mover has reached its destination."/>
            /// </summary>
            /// <returns></returns>
            public virtual bool MoveToDestination()
            {
                return false;
            }

            /// <summary>
            /// Wether the hit is a wall or not, returns true if the angle is considered a wall, false otherwise.
            /// </summary>
            public bool IsWall(RaycastHit hit, out float angle)
            {
                angle = Vector3.Angle(hit.normal, Vector3.up);
                return Mathf.Approximately(angle, 90);
            }

            public void DoMove(Vector3 motion)
            {
                characterController.Move(motion);
            }

            public void SetIgnoreCollision(Collider[] colliders)
            {
                foreach (var collider in colliders)
                {
                    Physics.IgnoreCollision(characterController, collider, true);
                }
            }
            public GroundMover()
            {
                gameObject = new GameObject("CharacterController");
                characterController = gameObject.AddComponent<CharacterController>();
                gameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
            }

            public void Dispose()
            {
                if (gameObject)
                    DestroyImmediate(gameObject, true);
            }
        }
        protected class HalfBakedNode : MonoBehaviour
        {
            public int nodeIndex;
        }*/
    }
}