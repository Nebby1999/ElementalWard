using Nebula;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ElementalWard.Navigation
{
    public class PathfindingSystem : SingletonBehaviour<PathfindingSystem>
    {
        public const float MOVE_DIAGONAL_COST = 14;
        public const float MOVE_STRAIGHT_COST = 10;
        public AStarNodeGrid groundNodes;
        public AStarNodeGrid airNodes;

        private List<Vector3> path = new();
        private NativeArray<int2> _neighbourOffsets;
        private float stopwatch;

        private void Awake()
        {
            _neighbourOffsets = new NativeArray<int2>(new int2[8]
            {
                new int2(-1, 0), //Left
                new int2(+1, 0), //Right
                new int2(0, +1), //Up
                new int2(0, -1), //Down
                new int2(-1, -1),//Left Down
                new int2(-1, +1),//Left Up
                new int2(+1, -1),//Right Down
                new int2(+1, +1),//Right Up
            }, Allocator.Persistent);
        }

        public FindPathJob RequestPath(AStarNodeGrid aStarNodeGrid, Vector3 start, Vector3 end, float actorHeight, float actorRadius, float actorJumpStrength)
        {
            SerializedNodeGrid grid = aStarNodeGrid.grid;
            if (!grid)
                return default;

            FindPathJob job = new FindPathJob
            {
                neighbourOffsets = _neighbourOffsets,
                actorHeight = actorHeight,
                actorRadius = actorRadius,
                actorJumpStrength = actorJumpStrength,
                endPos = end,
                startPos = start,
                result = new NativeList<float3>(512, Allocator.TempJob),
                gridSizeX = grid.gridSizeX,
                gridSizeY = grid.gridSizeY,
                gridWorldSize = grid.gridWorldSize,
                gridWorldPosition = aStarNodeGrid.transform.position,
                nodes = new NativeArray<PathNode>(grid.RuntimeNodes, Allocator.TempJob)
            };
            return job;
        }

        private void OnDestroy()
        {
            _neighbourOffsets.Dispose();
        }
    }
}