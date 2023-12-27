using Nebula;
using Nebula.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

namespace ElementalWard.Navigation
{
    public class AirNodeBaker : IElementalWardBaker
    {
        public float ProviderScale { get; set; } = 1f;
        public Vector3 ProviderPosition { get; set; } = Vector3.zero;
        public INodeGraph NodeGraph => _nodeGraph;
        private AirNodeGraph _nodeGraph;
        public List<SerializedPathNode> PathNodes => NodeGraph?.SerializedNodes ?? new List<SerializedPathNode>();
        public List<SerializedPathNodeLink> BakedLinks => _bakedLinks;
        private List<SerializedPathNodeLink> _bakedLinks = new List<SerializedPathNodeLink>();
        private List<Collider> _collidersForBaking = new List<Collider>();
        private Mover _mover;

        public void BakeNode(SerializedPathNode nodeA, int nodeAIndex)
        {
            Vector3 nodeAPosition = nodeA.position + ProviderPosition;
            bool3 isNan = math.isnan(nodeAPosition);
            if (math.any(isNan))
                return;

            _mover.StartPosition = nodeAPosition;
            for (int i = 0; i < PathNodes.Count; i++)
            {
                int nodeBIndex = i;
                if (nodeBIndex == nodeAIndex)
                    continue;

                var nodeB = PathNodes[nodeBIndex];
                Vector3 nodeBPosition = nodeB.position + ProviderPosition;
                isNan = math.isnan(nodeBPosition);
                if (math.any(isNan))
                    continue;

                float distance = Vector3.Distance(nodeAPosition, nodeBPosition);
                if (distance > SerializedPathNode.MAX_DISTANCE * ProviderScale)
                    continue;

                _mover.SetMoverPosition(nodeAPosition, false);
                _mover.DestinationPosition = nodeBPosition;

                bool hitWall = false;
                float slopeAngle = 0;
                RaycastHit[] raycastHits = _mover.TestCapsuleCast();
                for(int j = 0; j < raycastHits.Length; j++)
                {
                    var hit = raycastHits[j];

                    var hitName = hit.collider.name;
                    if(!hitName.StartsWith("TempCollider_"))
                    {
                        //Check the Angle, see if it's either a wall or a slope
                        var hitAngle = Vector3.Angle(hit.normal, Vector3.up);
                        if(hitAngle > 89 && hitAngle < 91)
                        {
                            hitWall = true;
                            break;
                        }
                        else if(hitAngle < 89)
                        {
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
                        if (colliderNodeIndex < 0 || colliderNodeIndex > PathNodes.Count - 1)
                        {
                            Debug.Log($"{hit.collider}'s TempCollider ID is out of range (0 - {PathNodes.Count - 1}), Collider ID={colliderNodeIndex}");
                            UnityEngine.Object.Destroy(hit.collider.gameObject);
                            continue;
                        }

                        if (colliderNodeIndex == nodeAIndex)
                            continue;

                        if(colliderNodeIndex != nodeBIndex)
                        {
                            nodeBIndex = colliderNodeIndex;
                            nodeB = PathNodes[nodeBIndex];
                            nodeBPosition = nodeB.position;
                            break;
                        }
                    }
                }

                if (hitWall)
                    continue;

                _mover.DestinationPosition = nodeBPosition;
                Physics.SyncTransforms();
                for(int j = 0; j < 5; j++)
                {
                    _mover.Move();
                }

                if (!_mover.ReachedDestination())
                {
                    continue;
                }

                bool linkAlreadyExists = false;
                for(int j = 0; j < BakedLinks.Count; j++)
                {
                    var link = BakedLinks[j];

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
                    normal = (nodeBPosition - nodeAPosition).normalized,
                    slopeAngle = slopeAngle,
                    distance = distance
                };
                BakedLinks.Add(newLink);
                nodeA.serializedPathNodeLinkIndices.Add(BakedLinks.Count - 1);
            }
        }

        public void Dispose()
        {
            foreach(var collider in _collidersForBaking)
            {
                if (collider)
                    GameObject.Destroy(collider.gameObject);
            }
            _mover?.Dispose();
        }

        public void PreBake()
        {
#if UNITY_EDITOR
            if(NodeGraph is UnityEngine.Object obj)
            {
                UnityEditor.Undo.RegisterCompleteObjectUndo(obj, "Clear GraphNode Links");
            }
#endif

            for(int i = 0; i < PathNodes.Count; i++)
            {
                var pathNode = PathNodes[i];
                pathNode.serializedPathNodeLinkIndices.Clear();

                GameObject pathNodeObject = new("TempCollider_" + i);
                pathNodeObject.transform.position = pathNode.position;
                pathNodeObject.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
                var collider = pathNodeObject.AddComponent<CapsuleCollider>();
                collider.radius = 0.5f * ProviderScale;
                collider.height = 2 * ProviderScale;
                pathNodeObject.AddComponent<KYS>();
                Physics.IgnoreCollision(_mover.MoverCharacterController, collider, true);
                _collidersForBaking.Add(collider);
            }
            Physics.SyncTransforms();

            Debug.Log("Air Prebake Complete");
        }

        public AirNodeBaker(AirNodeGraph graph)
        {
            _nodeGraph = graph;
            _mover = new Mover();
        }

        private class Mover : IDisposable
        {
            public Vector3 CapsuleHalfHeight => new Vector3(0, (_moverCharacterController.height / 2) + _moverCharacterController.skinWidth, 0);
            public Vector3 StartPosition { get; set; }
            public Vector3 DestinationPosition { get; set; }
            public Vector3 MoverPosition => _transform.position;
            public void SetMoverPosition(Vector3 value, bool shiftByCapsuleHalfHeight)
            {
                value += shiftByCapsuleHalfHeight ? CapsuleHalfHeight : Vector3.zero;
                _transform.position = value;
            }
            public Vector3 MoverFeetPosition => MoverPosition - CapsuleHalfHeight;
            public GameObject MoverObject => _moverObject;
            private GameObject _moverObject;

            public CharacterController MoverCharacterController => _moverCharacterController;
            private CharacterController _moverCharacterController;
            private Transform _transform;
            public void Dispose()
            {
                if(_moverObject)
                {
                    GameObject.DestroyImmediate(_moverObject, true);
                }
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

                    if (hit.collider.isTrigger)
                        continue;

                    result.Add(hit);
                }

                return result.ToArray();
            }

            public void Move()
            {
                var dest = DestinationPosition;
                var start = MoverPosition;

                Vector3 motion = dest - start;
                _moverCharacterController.Move(motion);
            }

            public bool ReachedDestination()
            {
                float distance = Vector3.Distance(MoverPosition, DestinationPosition);
                return distance < 0.05f;
            }

            public Mover()
            {
                _moverObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                _moverObject.name = "MoverTester";
                Component.DestroyImmediate(_moverObject.GetComponent<CapsuleCollider>(), true);
                _moverCharacterController = _moverObject.AddComponent<CharacterController>();
                _moverCharacterController.height = 1f;
                _transform = _moverObject.transform;
            }

        }
    }
}