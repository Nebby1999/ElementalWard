using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ElementalWard.Navigation
{
    [CreateAssetMenu(fileName = "New SerializedNodeGrid", menuName = "ElementalWard/Navigation/SerializedNodeGrid")]
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

        public float minPenalty = float.MaxValue;
        public float maxPenalty = float.MinValue;

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

        public void SetSerializedNodes(SerializedNode[] nodes)
        {
            serializedNodes = nodes;
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
                        parentIndex = -1,
                        movementPenalty = serialized.movementPenalty
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
            worldPos.z -= 8;
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