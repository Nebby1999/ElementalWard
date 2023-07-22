using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard.Navigation
{
    [CreateAssetMenu(fileName = "New GroundNodeBaker", menuName = ElementalWardApplication.APP_NAME + "/Navigation/GroundNodeBaker")]
    public class GroundNodeBaker : NodeBaker
    {
        public override void Bake(BakeParams bakeParams, SerializedNodeGrid nodeGrid)
        {
            bakeParams.OnPreBake();
            BakeGrid(bakeParams, nodeGrid);
        }

        private void BakeGrid(BakeParams bakeParams, SerializedNodeGrid nodeGrid)
        {
            float nodeRadius = bakeParams.nodeRadius;
            Vector2 gridWorldSize = nodeGrid.gridWorldSize = bakeParams.gridWorldSize;
            Vector3 position = bakeParams.bakingPosition;

            float nodeDiameter = nodeRadius * 2;
            int gridSizeX = nodeGrid.gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            int gridSizeY = nodeGrid.gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

            var serializedNodes = new SerializedNode[gridSizeX * gridSizeY];
            Vector3 worldBottomLeft = position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    BakeNode(nodeGrid, nodeRadius, position, nodeDiameter, serializedNodes, worldBottomLeft, x, y);
                }
            }
            BlurPenaltyMap(ref serializedNodes, nodeGrid, 3);
            nodeGrid.SetSerializedNodes(serializedNodes);
        }

        private void BakeNode(SerializedNodeGrid nodeGrid, float nodeRadius, Vector3 position, float nodeDiameter, SerializedNode[] serializedNodes, Vector3 worldBottomLeft, int x, int y)
        {
            Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
            bool open = true;
            float penalty = 0;

            bool isValidGround = false;
            //Check if there's a ceiling.
            Ray ray = new Ray(worldPoint, Vector3.up);
            if (Physics.Raycast(ray, out var hit1, 1000, LayerIndex.world.Mask))
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
            ray = new Ray(worldPoint + Vector3.up * 2, Vector3.down);
            hits = Physics.SphereCastAll(ray, nodeRadius, 1024, LayerIndex.world.Mask);
            bool gotCustomModifier = false;
            foreach(var hit in hits)
            {
                if(hit.collider && hit.collider.TryGetComponent<GroundNodeModifier>(out var modifier))
                {
                    open = !modifier.isObstacle;
                    penalty = modifier.movementPenalty;
                    gotCustomModifier = true;
                    break;
                }
            }

            if(!gotCustomModifier)
            {
                penalty = 5;
            }

            if(!open || !isValidGround)
            {
                penalty += 5;
            }

            serializedNodes[nodeGrid.CalculateIndex(x, y)] = new SerializedNode
            {
                isValidPosition = isValidGround,
                isOpen = isValidGround ? open : false,
                movementPenalty = penalty,
                worldPosition = worldPoint,
            };
        }

        private void BlurPenaltyMap(ref SerializedNode[] serializedNodes, SerializedNodeGrid nodeGrid, int blurSize)
        {
            float minPenalty = float.MaxValue;
            float maxPenalty = float.MinValue;

            int gridSizeX = nodeGrid.gridSizeX;
            int gridSizeY = nodeGrid.gridSizeY;
            int kernelSize = blurSize * 2 + 1;
            int kernelExtents = (kernelSize - 1) / 2;

            float[,] penaltiesHorizontalPass = new float[gridSizeX, gridSizeY];
            float[,] penaltiesVerticalPass = new float[gridSizeX, gridSizeY];

            for(int y = 0; y < gridSizeY; y++)
            {
                for(int x = -kernelExtents; x <= kernelExtents; x++)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                    penaltiesHorizontalPass[0, y] += serializedNodes[nodeGrid.CalculateIndex(sampleX, y)].movementPenalty;
                }

                for(int x = 1; x < gridSizeX; x++)
                {
                    int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                    int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);
                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - serializedNodes[nodeGrid.CalculateIndex(removeIndex, y)].movementPenalty + serializedNodes[nodeGrid.CalculateIndex(addIndex, y)].movementPenalty;
                }
            }

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = -kernelExtents; y <= kernelExtents; y++)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                }

                float blurredPenalty = penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize);
                
                var node = serializedNodes[nodeGrid.CalculateIndex(x, 0)];
                node.movementPenalty = blurredPenalty;
                serializedNodes[nodeGrid.CalculateIndex(x, 0)] = node;

                for (int y = 1; y < gridSizeY; y++)
                {
                    int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                    int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);
                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];

                    blurredPenalty = penaltiesVerticalPass[x, y] / (kernelSize * kernelSize);
                   
                    node = serializedNodes[nodeGrid.CalculateIndex(x, y)];
                    node.movementPenalty = blurredPenalty;
                    serializedNodes[nodeGrid.CalculateIndex(x, y)] = node;

                    if(blurredPenalty > maxPenalty)
                    {
                        maxPenalty = blurredPenalty;
                    }
                    if(blurredPenalty < minPenalty)
                    {
                        minPenalty = blurredPenalty;
                    }
                }
            }
            nodeGrid.minPenalty = minPenalty;
            nodeGrid.maxPenalty = maxPenalty;
        }
    }
}