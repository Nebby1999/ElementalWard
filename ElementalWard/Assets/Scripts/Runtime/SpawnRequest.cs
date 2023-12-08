using Nebula;
using Nebula.Navigation;
using System;
using UnityEngine;

namespace ElementalWard
{
    public class SpawnRequest
    {
        public SpawnCard spawnCard;
        public PlacementRule placementRule;
        public Xoroshiro128Plus rng;
        public Action<SpawnCard.SpawnResult> onSpawned;

        public SpawnRequest(SpawnCard spawnCard, PlacementRule placementRule, Xoroshiro128Plus rng)
        {
            this.spawnCard = spawnCard;
            this.placementRule = placementRule;
            this.rng = rng;
        }
    }

    public class PlacementRule
    {
        public PlacementDelegate placement;
        public Vector3 TargetPosition => _targetTransform ? _targetTransform.position : _position;
        public float minDistance;
        public float maxDistance;

        private Transform _targetTransform;
        private Vector3 _position;

        public PlacementRule(Transform transform)
        {
            _targetTransform = transform;
        }

        public PlacementRule(Vector3 position)
        {
            _position = position;
        }

        public static GameObject DirectPlacement(SpawnRequest request)
        {
            var placement = request.placementRule;
            Quaternion quaternion = Quaternion.Euler(0f, request.rng.NextNormalizedFloat * 360f, 0f);
            return request.spawnCard.DoSpawn(placement.TargetPosition, placement._targetTransform ? placement._targetTransform.rotation : quaternion, request).spawnedInstance;
        }

        public static GameObject NearestNodePlacement(SpawnRequest request)
        {
            if (!SceneNavigationSystem.HasGraphs)
                throw new InvalidOperationException("Cannot use Approximate Placement when no graphs exists");

            IGraphProvider graphProider = null;
            switch(request.spawnCard.graphType)
            {
                case NodeGraphType.Air:
                    graphProider = SceneNavigationSystem.AirNodeProvider;
                    break;
                case NodeGraphType.Ground:
                    graphProider = SceneNavigationSystem.GroundNodeProvider;
                    break;
                default:
                    throw new Exception("Invalid graph type.");
            }

            var targetPosition = request.placementRule.TargetPosition;
            var distanceRand = request.rng.RangeFloat(request.placementRule.minDistance, request.placementRule.maxDistance);
            Vector2 insideUnitCircle = request.rng.InsideUnitCircle() * distanceRand;
            var position = new Vector3(targetPosition.x + insideUnitCircle.x, targetPosition.y, targetPosition.z + insideUnitCircle.y);
            position = SceneNavigationSystem.FindClosestPositionUsingNodeGraph(position, graphProider);
            var pos2 = position;
            pos2.y = request.placementRule.TargetPosition.y;
            Quaternion rotation = UnityUtil.SafeLookRotation(request.placementRule.TargetPosition - pos2);

            return request.spawnCard.DoSpawn(position, rotation, request).spawnedInstance;
        }

        public static GameObject RandomNodePlacement(SpawnRequest request)
        {
            if (!SceneNavigationSystem.HasGraphs)
                throw new InvalidOperationException("Cannot use Approximate Placement when no graphs exists");

            IGraphProvider graphProider = null;
            switch (request.spawnCard.graphType)
            {
                case NodeGraphType.Air:
                    graphProider = SceneNavigationSystem.AirNodeProvider;
                    break;
                case NodeGraphType.Ground:
                    graphProider = SceneNavigationSystem.GroundNodeProvider;
                    break;
                default:
                    throw new Exception("Invalid graph type.");
            }
            var position = SceneNavigationSystem.GetRandomPositionFromNodeGraph(graphProider, request.rng);
            Quaternion rotation = Quaternion.Euler(0f, request.rng.NextNormalizedFloat * 360f, 0f);

            return request.spawnCard.DoSpawn(position, rotation, request).spawnedInstance;
        }

        public delegate GameObject PlacementDelegate(SpawnRequest spawnRequest);
    }
}