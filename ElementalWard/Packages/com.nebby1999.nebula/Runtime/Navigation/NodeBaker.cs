using System;
using System.Collections.Generic;
using System.Globalization;
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
        private List<SerializedPathNodeLink> _nodeLinks = new();
        private List<Collider> _collidersForBaking = new();
        private float _maxNodeDistance;
        private Mover _mover;

        public void PreBake()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCompleteObjectUndo(NodeGraphAsset, "Clear GraphNode Links");
#endif

            for (int i = 0; i < PathNodes.Count; i++)
            {
                var pathNode = PathNodes[i];
                pathNode.serializedPathNodeLinkIndices.Clear();

                GameObject pathNodeObject = new("TempCollider_" + i);
                pathNodeObject.transform.position = pathNode.worldPosition;
                pathNodeObject.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
                var collider = pathNodeObject.AddComponent<SphereCollider>();
                Physics.IgnoreCollision(_mover.MoverCharacterController, collider, true);
                _collidersForBaking.Add(collider);
            }
        }
        public void BakeNodes()
        {
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
                foreach(Collider collider in _collidersForBaking)
                {
                    Collider.DestroyImmediate(collider.gameObject, true);
                }
                _mover?.Dispose();
            }
        }

        public void CommitBakedNodes()
        {
            NodeGraphAsset.serializedLinks = _nodeLinks;
            NodeGraphAsset.serializedNodes = PathNodes;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(NodeGraphAsset);
#else
            NodeGraphAsset.SetDirty();
#endif
        }

        private void BakeNode(SerializedPathNode nodeA, int nodeAIndex,  List<SerializedPathNode> pathNodes)
        {
            bool3 isNan = math.isnan(nodeA.worldPosition);
            if (math.any(isNan))
                return;

            _mover.StartPosition = nodeA.worldPosition;
            for(int i = 0; i < pathNodes.Count; i++)
            {
                int nodeBIndex = i;
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

                bool hitWall = false;
                float slopeAngle = 0;
                RaycastHit[] raycastHits = _mover.TestCapsuleCast();
                for(int j = 0; j < raycastHits.Length; j++)
                {
                    var hit = raycastHits[j];

                    var hitName = hit.collider.name;
                    //This is not a nodeCollider.
                    if(!hitName.StartsWith("TempCollider_"))
                    {
                        //Check the angle, see if its either a wall or a slope.
                        var hitAngle = Vector3.Angle(hit.normal, Vector3.up);
                        if(hitAngle > 89 && hitAngle < 91)
                        {
                            //Its a wall
                            hitWall = true;
                            break;
                        }
                        else if(hitAngle < 89)
                        {
                            //Its a slope, save the angle and continue;
                            slopeAngle = hitAngle;
                            continue;
                        }
                        else if(hitAngle > 91)
                        {
                            //TODO: see what happens on obstuse angled slopes.
                            continue;
                        }
                    }

                    if(hitName.StartsWith("TempCollider_"))
                    {
                        var colliderNodeIndexAsString = hitName.Substring("TempCollider_".Length);
                        var colliderNodeIndex = int.Parse(colliderNodeIndexAsString, CultureInfo.InvariantCulture);

                        if (colliderNodeIndex == nodeAIndex)
                            continue;

                        //We found a node between nodeAIndex and nodeBIndex, use this node for baking
                        if(colliderNodeIndex != nodeBIndex)
                        {
                            nodeBIndex = colliderNodeIndex;
                            nodeB = pathNodes[nodeBIndex];
                            break;
                        }
                    }
                }

                if (hitWall)
                    continue;
                _mover.DestinationPosition = nodeB.worldPosition;

                Physics.SyncTransforms();
                for(int j = 0; j < 5; j++)
                {
                    _mover.Move();
                }
               
                if(_mover.MoverFeetPosition != _mover.DestinationPosition)
                {
                    continue;
                }

                bool linkAlreadyExists = false;
                //Check if theres already a link that has the same nodes
                for(int j = 0; j < _nodeLinks.Count; j++)
                {
                    var link = _nodeLinks[j];

                    var nodeAValid = link.nodeAIndex == nodeAIndex || link.nodeBIndex == nodeAIndex;
                    var nodeBValid = link.nodeAIndex == nodeBIndex || link.nodeBIndex == nodeBIndex;

                    if(nodeAValid && nodeBValid)
                    {
                        if(!nodeA.serializedPathNodeLinkIndices.Contains(j))
                        {
                            nodeA.serializedPathNodeLinkIndices.Add(j);
                        }
                        linkAlreadyExists = true;
                        break;
                    }
                }
                if (linkAlreadyExists)
                    continue;

                if(nodeA.serializedPathNodeLinkIndices.Count >= 100)
                {
                    Debug.LogWarning($"PathNode of index {nodeAIndex} has reached it's max capacity of 100 links, cannot add more links.");
                    continue;
                }

                SerializedPathNodeLink newLink = new()
                {
                    nodeAIndex = nodeAIndex,
                    nodeBIndex = nodeBIndex,
                    normal = (nodeB.worldPosition - nodeA.worldPosition).normalized,
                    slopeAngle = slopeAngle,
                    distance = distance
                };
                _nodeLinks.Add(newLink);
                nodeA.serializedPathNodeLinkIndices.Add(_nodeLinks.Count - 1);
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
                var dest = DestinationPosition;
                dest.y -= 10;
                var start = MoverPosition;

                Vector3 motion = dest - start;
                _moverCharacterController.Move(motion);
            }

            public RaycastHit[] TestCapsuleCast()
            {
                var p1 = _transform.position + _moverCharacterController.center + (0.5f * _moverCharacterController.height - _moverCharacterController.radius) * Vector3.up;
                var p2 = _transform.position + _moverCharacterController.center + (0.5f * _moverCharacterController.height - _moverCharacterController.radius) * Vector3.down;

                var direction = DestinationPosition - StartPosition;
                direction = direction.normalized;
                var distance = Vector3.Distance(DestinationPosition, StartPosition);

                List<RaycastHit> result = new();
                var hits = Physics.CapsuleCastAll(p1, p2, _moverCharacterController.radius, direction, distance);
                foreach(var hit in hits)
                {
                    if (hit.collider == _moverCharacterController)
                        continue;

                    result.Add(hit);
                }

                return result.ToArray();
            }

            public Mover()
            {
                _moverObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                _moverObject.name = "MoverTester";
                Component.DestroyImmediate(_moverObject.GetComponent<CapsuleCollider>(), true);
                _moverCharacterController = _moverObject.AddComponent<CharacterController>();
                _transform = _moverObject.transform;
                //var tester = _moverObject.AddComponent<Tester>();

                //tester.mover = this;
                //tester.controller = _moverCharacterController;
            }
        }
    }
}