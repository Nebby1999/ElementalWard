using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Nebula.Navigation
{
#if !UNITY_EDITOR
    using Unity.Burst;
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
#endif
    public struct FindPathJob : IJob
    {
        public float actorHeight;
        public float3 startPos;
        public float3 endPos;
        public NativeReference<int> startIndex;
        public NativeReference<int> endIndex;
        public NativeArray<RuntimePathNode> nodes;
        public NativeArray<RuntimePathNodeLink> links;
        public NativeList<float3> result;
        public void Execute()
        {
            if ((startIndex.Value == -1 || endIndex.Value == -1) || (startIndex.Value == endIndex.Value))
            {
                if(math.any(math.isinf(startPos)) && math.any(math.isinf(endPos)))
                {
                    result.Add(endPos);
                    result.Add(startPos);
                }
                return;
            }

            RuntimePathNode startNode = nodes[startIndex.Value];
            startNode.gCost = 0;
            nodes[startIndex.Value] = startNode;

            RuntimePathNode endNode = nodes[endIndex.Value];

            NativeList<int> openList = new NativeList<int>(1024, Allocator.Temp);
            NativeHashSet<int> closedSet = new NativeHashSet<int>(4096, Allocator.Temp);

            openList.Add(startNode.nodeIndex);
            while(openList.Length > 0)
            {
                var currentNodeIndex = GetLowestCostFNodeIndex(openList);
                RuntimePathNode currentNode = nodes[currentNodeIndex];

                //Destination Reached
                if(currentNode == endNode)
                {
                    break;
                }

                //Remove current noded from open list
                for(int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNode.nodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedSet.Add(currentNodeIndex);
                
                for(int i = 0; i < currentNode.pathNodeLinkIndices.Length; i++)
                {
                    var link = links[currentNode.pathNodeLinkIndices[i]];

                    bool isCurrentNodeNodeAOfLink = IsNodeAOfLink(link, currentNode);
                    int neighbourIndex = isCurrentNodeNodeAOfLink ? link.nodeBIndex : link.nodeAIndex;
                    RuntimePathNode neighbourNode = nodes[neighbourIndex];

                    float hCost = link.distanceCost;
                    float newMovementCostToNeighbour = currentNode.gCost + hCost;
                    if(newMovementCostToNeighbour < neighbourNode.gCost)
                    {
                        neighbourNode.gCost = newMovementCostToNeighbour;
                        neighbourNode.hCost = hCost;
                        neighbourNode.parentLinkIndex = link.linkIndex;
                        nodes[neighbourIndex] = neighbourNode;

                        if (!openList.Contains(neighbourNode.nodeIndex))
                        {
                            openList.Add(neighbourNode.nodeIndex);
                        }
                    }
                }
            }
            endNode = nodes[endNode.nodeIndex];

            if(endNode.parentLinkIndex != -1)
            {
                CalculatePath(endNode);
            }

            openList.Dispose();
            closedSet.Dispose();
        }

        private void CalculatePath(RuntimePathNode endNode)
        {
            if(endNode.parentLinkIndex == -1)
            {
                return;
            }

            float actorHeightHalved = actorHeight / 2;
            RuntimePathNode currentNode = endNode;
            float3 modifiedCurrentNodePos = currentNode.worldPosition;
            modifiedCurrentNodePos.y += actorHeightHalved;

            result.Add(endPos);
            while (currentNode.parentLinkIndex != -1)
            {
                var link = links[currentNode.parentLinkIndex];
                bool isCurrentNodeNodeAOfLink = IsNodeAOfLink(link, currentNode);

                var parentNode = isCurrentNodeNodeAOfLink ? nodes[link.nodeBIndex] : nodes[link.nodeAIndex];
                result.Add(currentNode.worldPosition);
                currentNode = parentNode;
            }
            result.Add(currentNode.worldPosition);
            //result.Add(startPos);

            var length = result.Length;
            var index1 = 0;
            for(var index2 = length - 1; index1 < index2; index2--)
            {
                (result[index2], result[index1]) = (result[index1], result[index2]); ;
                index1++;
            }
        }
        private int GetLowestCostFNodeIndex(NativeList<int> openList)
        {
            RuntimePathNode lowestCostPathNode = nodes[openList[0]];
            for(int i = 1; i < openList.Length; i++)
            {
                RuntimePathNode testPathNode = nodes[openList[i]];
                if(testPathNode.FCost < lowestCostPathNode.FCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }
            return lowestCostPathNode.nodeIndex;
        }

        public bool IsNodeAOfLink(RuntimePathNodeLink link, RuntimePathNode node)
        {
            return link.nodeAIndex == node.nodeIndex;
        }
    }
}