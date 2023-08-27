using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.Mathematics;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// A class that bakes the nodes of a nodeGraphAsset.
    /// </summary>
    public class NodeBaker
    {
        public NodeGraphAsset NodeGraphAsset { get; private set; }

        private List<SerializedPathNode> PathNodes => NodeGraphAsset ? NodeGraphAsset.serializedNodes : new();
        private List<SerializedPathNodeLink> _nodeLinks = new List<SerializedPathNodeLink>();
        private float _maxNodeDistance;
        private Mover _mover;
        private bool HasPreBaked
        {
            get
            {
                foreach(SerializedPathNode pathNode in PathNodes)
                {
                    if (pathNode.serializedPathNodeLinkIndices.Count > 0)
                        return false;
                }
                return true;
            }
        }
        public void PreBake()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCompleteObjectUndo(NodeGraphAsset, "Clear GraphNode Links");
#endif
            foreach (SerializedPathNode pathNode in PathNodes)
                pathNode.serializedPathNodeLinkIndices.Clear();
        }
        public void BakeNodes()
        {
            if (!HasPreBaked)
                PreBake();

            try
            {
                for (int i = 0; i < PathNodes.Count; i++)
                {
                    BakeNode(PathNodes[i], i, PathNodes);
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _mover?.Dispose();
            }
        }

        public void CommitBakedNodes()
        {
            NodeGraphAsset.serializedLinks = _nodeLinks;
            NodeGraphAsset.serializedNodes = PathNodes;
        }

        private void BakeNode(SerializedPathNode nodeA, int nodeAIndex,  List<SerializedPathNode> pathNodes)
        {
            bool3 isNan = math.isnan(nodeA.worldPosition);
            if (math.any(isNan))
                return;

            _mover.StartPosition = nodeA.worldPosition;
            for(int nodeBIndex = 0; nodeBIndex < pathNodes.Count; nodeBIndex++)
            {
                if (nodeBIndex == nodeAIndex)
                    continue;

                var nodeB = pathNodes[nodeBIndex];
                isNan = math.isnan(nodeB.worldPosition);
                if (math.any(isNan))
                    continue;

                float distance = Vector3.Distance(nodeA.worldPosition, nodeB.worldPosition);
                if (distance > _maxNodeDistance)
                    continue;

                _mover.SetMoverPosition(nodeA.worldPosition, true);
                _mover.DestinationPosition = nodeB.worldPosition;

                Physics.SyncTransforms();
                _mover.Move();
               
                if(_mover.MoverFeetPosition != _mover.DestinationPosition)
                {
                    continue;
                }

                SerializedPathNodeLink link = new()
                {
                    nodeAIndex = nodeAIndex,
                    nodeBIndex = nodeBIndex,
                    normal = nodeB.worldPosition - nodeA.worldPosition,
                    slopeAngle = 0,
                    distance = distance
                };
                NodeGraphAsset.serializedLinks.Add(link);
                nodeA.serializedPathNodeLinkIndices.Add(NodeGraphAsset.serializedLinks.Count - 1);
            }
        }

        public NodeBaker(NodeGraphAsset nodeGraphAsset, float maxDistance)
        {
            NodeGraphAsset = nodeGraphAsset;
            _maxNodeDistance = maxDistance;
            _mover = new Mover();
        }

        public class Mover : IDisposable
        {
            public Vector3 CapsuleHalfHeight => new Vector3(0, (_moverCharacterController.height / 2) + _moverCharacterController.skinWidth, 0);
            public Vector3 StartPosition { get; set; }
            public Vector3 DestinationPosition { get; set; }
            public Vector3 MoverPosition { get => _transform.position; }
            public void SetMoverPosition(Vector3 value, bool shiftByCapsuleHalfHeight)
            {
                _transform.position = value;
                if(shiftByCapsuleHalfHeight)
                    _transform.position += CapsuleHalfHeight;

            }
            public Vector3 MoverFeetPosition
            {
                get
                {
                    var feetPos = MoverPosition;
                    feetPos -= CapsuleHalfHeight;
                    return feetPos;
                }
            }
            public GameObject MoverObject => _moverObject;
            private GameObject _moverObject;
            public CharacterController MoverCharacterController => _moverCharacterController;
            private CharacterController _moverCharacterController;
            private Transform _transform;

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(_moverObject);
            }

            public void Move()
            {
                Vector3 motion = DestinationPosition - MoverPosition;
                motion = _transform.TransformDirection(motion);
                _moverCharacterController.Move(motion);
            }

            public Mover()
            {
                _moverObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                _moverObject.name = "MoverTester";
                Component.DestroyImmediate(_moverObject.GetComponent<CapsuleCollider>(), true);
                _moverCharacterController = _moverObject.AddComponent<CharacterController>();
                _transform = _moverObject.transform;
            }
        }
    }
}