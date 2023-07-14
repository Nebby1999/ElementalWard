using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ElementalWard.Navigation
{
    [CreateAssetMenu(fileName = "New SerializedNodeGrid", menuName = "AStarTest/SerializedNodeGrid")]
    public class SerializedNodeGrid : ScriptableObject
    {
        public struct BakeParams
        {
            public Vector3 bakingPosition;
            public Vector2 gridWorldSize;
            public float nodeRadius;
        }

        public Vector2 gridWorldSize;
        public int gridSizeX;
        public int gridSizeY;

        [SerializeField]
        private SerializedNode[] serializedNodes = Array.Empty<SerializedNode>();

        public bool HasValidRuntimeNodes => _nodes != null && _nodes.Length > 0;
        public PathNode[] RuntimeNodes
        {
            get
            {
                if (!HasValidRuntimeNodes)
                {
                    UpdateRuntimeNodes();
                }
                return _nodes;
            }
        }
        private PathNode[] _nodes = Array.Empty<PathNode>();

        public void Bake(BakeParams bakeParams)
        {
            float nodeRadius = bakeParams.nodeRadius;
            gridWorldSize = bakeParams.gridWorldSize;
            Vector3 position = bakeParams.bakingPosition;

            float nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

            serializedNodes = new SerializedNode[gridSizeX * gridSizeY];
            Vector3 worldBottomLeft = position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    bool open = true;
                    int penalty = 0;

                    bool isValidGround = false;
                    //Check if there's a ceiling.
                    Ray ray = new Ray(worldPoint, Vector3.up);
                    if (Physics.Raycast(ray, out var hit1, 100, LayerIndex.world.Mask))
                    {
                        //If so, raycast from it to find the node's y pos.
                        Vector3 point = hit1.point;

                        ray = new Ray(point, Vector3.down);
                        if (Physics.Raycast(ray, out var hit2, 1024, LayerIndex.world.Mask))
                        {
                            worldPoint.y = hit2.point.y;
                            isValidGround = true;
                        }
                    }
                    else //There's no ceiling, shift by 100 units up and raycast down.
                    {
                        Vector3 point = new Vector3(worldPoint.x, position.y + 100, worldPoint.z);
                        ray = new Ray(point, Vector3.down);
                        if (Physics.Raycast(ray, out var hit2, 1024, LayerIndex.world.Mask))
                        {
                            worldPoint.y = hit2.point.y;
                            isValidGround = true;
                        }
                    }


                    RaycastHit[] hits;
                    ray = new Ray(worldPoint, Vector3.down);
                    hits = Physics.SphereCastAll(ray, nodeRadius, 1024, LayerIndex.world.Mask);

                    serializedNodes[CalculateIndex(x, y)] = new SerializedNode
                    {
                        isValidPosition = isValidGround,
                        isOpen = isValidGround ? open : false,
                        movementPenalty = penalty,
                        worldPosition = worldPoint,
                    };
                }
            }
            UpdateRuntimeNodes();
            SetDirty();
        }


        private void UpdateRuntimeNodes()
        {
            _nodes = new PathNode[gridSizeX * gridSizeY];
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    int index = CalculateIndex(x, y);
                    var serialized = serializedNodes[index];
                    PathNode pathNode = new PathNode
                    {
                        worldPosition = serialized.worldPosition,
                        x = x,
                        y = y,
                        nodeIndex = index,
                        gCost = float.MaxValue,
                        hCost = 0,
                        isOpen = serialized.isOpen,
                        isValidPosition = serialized.isValidPosition,
                        parentIndex = -1
                    };

                    _nodes[pathNode.nodeIndex] = pathNode;
                }
            }
        }


        public NativeList<int> GetNeighbouringNodeIndices(PathNode node)
        {
            NativeList<int> result = new NativeList<int>(Allocator.Temp);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    //This is the node specified in the parameter
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.x + x;
                    int checkY = node.y + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        result.Add(CalculateIndex(checkX, checkY));
                    }
                }
            }
            return result;
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

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
            return CalculateIndex(x, y);
        }
    }
}