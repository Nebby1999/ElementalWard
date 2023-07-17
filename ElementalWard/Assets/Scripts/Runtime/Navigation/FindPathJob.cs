using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ElementalWard.Navigation
{
#if !UNITY_EDITOR
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
#endif
    public struct FindPathJob : IJob
    {
        [Unity.Collections.ReadOnly]
        public NativeArray<int2> neighbourOffsets;
        public float3 startPos;
        public float3 endPos;
        public float actorHeight;
        public float actorRadius;
        public float actorJumpStrength;
        public NativeList<float3> result;

        public float3 gridWorldPosition;
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

            endPos.z += 1;
            endPos.x -= 1;
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

                for (int i = 0; i < neighbourOffsets.Length; i++)
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
                    float newMovementCostToNeighbour = currentNode.gCost + hCost + neighbourNode.movementPenalty;
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

        //Note to self, modifying the node's pos by adding the actor height divided by 2 is a good way to make each goal position centered to the actor's capsule.
        private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.parentIndex == -1)
            {
                //Couldnt find path;
                return;
            }

            float actorHeightHalved = actorHeight / 2;
            float3 directionOld = float3.zero;
            PathNode currentNode = endNode;
            float3 modifiedCurrentNodePos = currentNode.worldPosition;
            modifiedCurrentNodePos.y += actorHeightHalved;

            while (currentNode.parentIndex != -1)
            {
                PathNode parent = pathNodeArray[currentNode.parentIndex];
                float3 modifiedParentNodePos = parent.worldPosition;
                modifiedParentNodePos.y += actorHeightHalved;
                float3 directionNew = modifiedCurrentNodePos - modifiedParentNodePos;
                if (!math.all(directionNew == directionOld) || parent.parentIndex == -1)
                {
                    result.Add(modifiedParentNodePos);
                }
                directionOld = directionNew;
                currentNode = parent;
                modifiedCurrentNodePos = modifiedParentNodePos;
            }

            var length = result.Length;
            var index1 = 0;
            for(var index2 = length - 1; index1 < index2; index2--)
            {
                (result[index2], result[index1]) = (result[index1], result[index2]);
                index1++;
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

            return PathfindingSystem.MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + PathfindingSystem.MOVE_STRAIGHT_COST * remaining;
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
            var percentX = (worldPos.x - gridWorldPosition.x + gridSizeX / 2) / gridSizeX;
            var percentY = (worldPos.z - gridWorldPosition.z + gridSizeY / 2) / gridSizeY;

            int x = (int)math.floor(math.clamp((gridSizeX) * percentX, 0, gridSizeX - 1));
            int y = (int)math.floor(math.clamp((gridSizeY) * percentY, 0, gridSizeY - 1));

            return CalculateIndex(x, y);
        }
    }
}