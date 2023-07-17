using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard.Navigation
{
    [CreateAssetMenu(fileName = "New AirNodeBaker", menuName = ElementalWardApplication.APP_NAME + "/Navigation/AirNodeBaker")]
    public class AirNodeBaker : NodeBaker
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
            nodeGrid.SetSerializedNodes(serializedNodes);
        }

        private void BakeNode(SerializedNodeGrid nodeGrid, float nodeRadius, Vector3 position, float nodeDiameter, SerializedNode[] serializedNodes, Vector3 worldBottomLeft, int x, int y)
        {
            Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
            //Shift world point by 2 so its node is not on the ground
            bool open = true;
            int penalty = 0;

            //Check if there's a ceiling.
            Ray ray = new Ray(worldPoint, Vector3.up);
            if (Physics.Raycast(ray, out var hit1, 1024, LayerIndex.world.Mask))
            {
                //If so, raycast from it to find the node's y pos.
                Vector3 point = hit1.point;
                ray = new Ray(point, Vector3.down);
                if (Physics.Raycast(ray, out var hit2, 1024, LayerIndex.world.Mask))
                {
                    worldPoint.y = hit2.point.y;
                }
            }
            else
            {
                //There's no ceiling, shift by 100 units up and try to find ground
                Vector3 point = new Vector3(worldPoint.x, position.y, worldPoint.z);
                ray = new Ray(point, Vector3.down);
                if (Physics.Raycast(ray, out var hit2, 1024, LayerIndex.world.Mask))
                {
                    worldPoint.y = hit2.point.y;
                }
            }

            worldPoint.y += 2;
            serializedNodes[nodeGrid.CalculateIndex(x, y)] = new SerializedNode
            {
                isValidPosition = true,
                isOpen = open,
                movementPenalty = penalty,
                worldPosition = worldPoint,
            };
        }
    }
}