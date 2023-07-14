using Nebula;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Unity.Burst;
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
        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private struct FindPathJob : IJob
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<int2> neighbourOffsets;
            public float3 startPos;
            public float3 endPos;
            public float actorHeight;
            public float actorRadius;
            public NativeList<float3> result;

            public float2 gridWorldSize;
            public int gridSizeX;
            public int gridSizeY;
            [DeallocateOnJobCompletion]
            public NativeArray<PathNode> nodes;

            public void Execute()
            {

                var startNodeIndex = GetNodeIndexFromPos(startPos);
                PathNode startNode = nodes[startNodeIndex];
                startNode.gCost = 0;
                nodes[startNodeIndex] = startNode;

                var endNodeIndex = GetNodeIndexFromPos(endPos);
                PathNode endNode = nodes[endNodeIndex];

                NativeList<int> openList = new NativeList<int>(1024, Allocator.Temp);
                NativeHashSet<int> closedSet = new NativeHashSet<int>(4096, Allocator.Temp);

                openList.Add(startNode.nodeIndex);
                while (openList.Length > 0)
                {
                    var currentNodeIndex = GetLowestCostFNodeIndex(openList, nodes);
                    PathNode currentNode = nodes[currentNodeIndex];

                    //Destination reached
                    if (currentNode == endNode)
                    {
                        break;
                    }

                    //Remove current node from open list
                    for (int i = 0; i < openList.Length; i++)
                    {
                        if (openList[i] == currentNode.nodeIndex)
                        {
                            openList.RemoveAtSwapBack(i);
                            break;
                        }
                    }

                    closedSet.Add(currentNodeIndex);

                    for(int i = 0; i < neighbourOffsets.Length; i++)
                    {
                        var neighbourIndex = GetNeighbourIndex(currentNode, neighbourOffsets[i]);

                        //Already searched or invalid index.
                        if (closedSet.Contains(neighbourIndex) || neighbourIndex == -1)
                            continue;

                        PathNode neighbourNode = nodes[neighbourIndex];
                        if (!neighbourNode.Available)
                        {
                            continue;
                        }

                        var currentXY = new float2(currentNode.x, currentNode.y);
                        var neighbourXY = new float2(neighbourNode.x, neighbourNode.y);

                        float hCost = CalculateDistanceCost(currentXY, neighbourXY);
                        float newMovementCostToNeighbour = currentNode.gCost + hCost;
                        if (newMovementCostToNeighbour < neighbourNode.gCost)
                        {
                            neighbourNode.gCost = newMovementCostToNeighbour;
                            neighbourNode.hCost = hCost;
                            neighbourNode.parentIndex = currentNode.nodeIndex;
                            nodes[neighbourNode.nodeIndex] = neighbourNode;

                            if (!openList.Contains(neighbourNode.nodeIndex))
                                openList.Add(neighbourNode.nodeIndex);
                        }
                    }
                }
                //Update to ensure we have the updated nodes.
                endNode = nodes[endNodeIndex];

                if (endNode.parentIndex != -1)
                {
                    CalculatePath(nodes, endNode);
                }

                openList.Dispose();
                closedSet.Dispose();
            }

            private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
            {
                if (endNode.parentIndex == -1)
                {
                    //Couldnt find path;
                    return;
                }

                result.Add(endNode.worldPosition);

                PathNode currentNode = endNode;
                while (currentNode.parentIndex != -1)
                {
                    PathNode parent = pathNodeArray[currentNode.parentIndex];
                    result.Add(parent.worldPosition);
                    currentNode = parent;
                }
            }

            //Use a heap later
            private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
            {
                PathNode lowestCostPathNode = pathNodeArray[openList[0]];
                for (int i = 1; i < openList.Length; i++)
                {
                    PathNode testPathNode = pathNodeArray[openList[i]];
                    if (testPathNode.FCost < lowestCostPathNode.FCost)
                    {
                        lowestCostPathNode = testPathNode;
                    }
                }
                return lowestCostPathNode.nodeIndex;
            }

            private float CalculateDistanceCost(float2 aPos, float2 bPos)
            {
                float xDistance = math.abs(aPos.x - bPos.x);
                float yDistance = math.abs(aPos.y - bPos.y);
                float remaining = math.abs(xDistance - yDistance);

                return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            }

            public int GetNeighbourIndex(PathNode currentNode, int2 offset)
            {
                int checkX = currentNode.x + offset.x;
                int checkY = currentNode.y + offset.y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    return CalculateIndex(checkX, checkY);
                }
                return -1;
            }

            public int CalculateIndex(int x, int y)
            {
                return x + y * gridSizeX;
            }

            public int GetNodeIndexFromPos(Vector3 worldPos)
            {
                //Absolute Fucking Cancer.
                worldPos.z -= 6;
                float percentX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
                float percentY = (worldPos.z + gridWorldSize.y / 2) / gridWorldSize.y;

                percentX = math.clamp(percentX, 0, 1);
                percentY = math.clamp(percentY, 0, 1);

                int x = (int)math.round((gridSizeX - 1) * percentX);
                int y = (int)math.round((gridSizeY - 1) * percentY);
                return CalculateIndex(x, y);
            }
        }
        public const float MOVE_DIAGONAL_COST = 14;
        public const float MOVE_STRAIGHT_COST = 10;
        public AStarNodeGrid groundNodes;

        public Transform start;
        public Transform end;

        private List<Vector3> path = new();
        private NativeArray<int2> _neighbourOffsets;

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

        private void Start()
        {
            int count = 1000;

            NativeArray<FindPathJob> jobs = new NativeArray<FindPathJob>(count, Allocator.Temp);
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(count, Allocator.Temp);

            for (int i = 0; i < count; i++)
            {
                jobs[i] = CreateJob(groundNodes, start.position, end.position, 1, 1);
                jobHandles[i] = jobs[i].Schedule();
            }

            JobHandle.CompleteAll(jobHandles);
            for(int i = 0; i < count; i++)
            {
                jobs[i].result.Dispose();
            }
            jobs.Dispose();
            jobHandles.Dispose();
        }

        private FindPathJob CreateJob(AStarNodeGrid aStarNodeGrid, Vector3 start, Vector3 end, float actorHeight, float actorRadius)
        {
            SerializedNodeGrid grid = aStarNodeGrid.grid;
            if (!grid)
                return default;

            FindPathJob job = new FindPathJob
            {
                neighbourOffsets = _neighbourOffsets,
                actorHeight = actorHeight,
                actorRadius = actorRadius,
                endPos = end,
                startPos = start,
                result = new NativeList<float3>(512, Allocator.TempJob),
                gridSizeX = grid.gridSizeX,
                gridSizeY = grid.gridSizeY,
                gridWorldSize = grid.gridWorldSize,
                nodes = new NativeArray<PathNode>(grid.RuntimeNodes, Allocator.TempJob)
            };
            return job;
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (i == 0)
                {
                    Gizmos.color = Color.green;
                }
                else if (i == path.Count - 1)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.black;
                }

                Gizmos.DrawSphere(path[i], AStarNodeGrid.NODE_RADIUS);
            }
        }

        private void OnDestroy()
        {
            _neighbourOffsets.Dispose();
        }
    }
}