using System;
using UnityEngine;

namespace ElementalWard.Navigation
{
    public class AStarNodeGrid : MonoBehaviour
    {
        public const float NODE_RADIUS = 0.5f;
        public NodeBaker baker;
        public SerializedNodeGrid grid;
        [SerializeField] private Vector2 gridWorldSize;

        [Header("Gizmo Settings")]
        public bool drawGizmos;
        public bool gizmosAsWireFrame;

        public PathNode[] RuntimeNodes
        {
            get
            {
                if (!grid)
                    return Array.Empty<PathNode>();

                if (!grid.HasValidRuntimeNodes)
                    Bake();

                return grid.RuntimeNodes;
            }
        }

        [ContextMenu("Bake to SerializedNodeGrid")]
        public void Bake()
        {
            if (!grid)
            {
                Debug.LogWarning("No Grid Selected", this);
                return;
            }

            baker.Bake(new NodeBaker.BakeParams
            {
                gridWorldSize = gridWorldSize,
                nodeRadius = NODE_RADIUS,
                bakingPosition = transform.position,
                bakingRequest = this
            }, grid);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (!grid)
                return;

            var runtimeNodes = grid.RuntimeNodes;
            for (int i = 0; i < runtimeNodes.Length; i++)
            {
                var node = runtimeNodes[i];

                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(grid.minPenalty, grid.maxPenalty, node.movementPenalty));
                Gizmos.color = node.Available ? Gizmos.color : Color.red;

                Vector3 worldPosition = node.worldPosition;
                Vector3 nodeRadius = 2f * NODE_RADIUS * Vector3.one;

                if(gizmosAsWireFrame)
                {
                    Gizmos.DrawWireCube(worldPosition, nodeRadius);
                }
                else
                {
                    Gizmos.DrawCube(worldPosition, nodeRadius);
                }
            }
        }
    }
}