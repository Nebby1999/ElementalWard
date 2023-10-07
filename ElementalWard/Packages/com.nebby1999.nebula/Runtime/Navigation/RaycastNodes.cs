using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// Class used to find all the reachable nodes relative to a position
    /// <br>Usually the result is fed to <see cref="FindClosestNodeIndexJob"/></br>
    /// </summary>
    public class RaycastNodes
    {
        public int worldLayerMask;
        public NativeArray<RuntimePathNode> nodes;
        public float3 originPosition;

        public int[] ExecuteRaycasts()
        {
            if(math.any(math.isinf(originPosition)))
                return Array.Empty<int>();

            int nodesLength = nodes.Length;
            if (nodesLength == 0)
                return Array.Empty<int>();

            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(nodesLength, Allocator.TempJob);
            NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(nodesLength, Allocator.TempJob);

            QueryParameters queryParams = new QueryParameters
            {
                hitBackfaces = true,
                hitMultipleFaces = true,
                layerMask = worldLayerMask,
                hitTriggers = QueryTriggerInteraction.Ignore
            };

            for(int i = 0; i < nodesLength; i++)
            {
                var node = nodes[i];
                var distance = math.distance(node.worldPosition, originPosition);
                var direction = math.normalize(originPosition - node.worldPosition);
                var command = new RaycastCommand(originPosition, direction, queryParams, distance);
                commands[i] = command;
            }

            var handle = RaycastCommand.ScheduleBatch(commands, hits, 100, 1);
            handle.Complete();

            List<int> reachableIndices = new List<int>();
            StringBuilder help = new StringBuilder();
            for(int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];

                if(!hit.collider)
                {
                    help.AppendLine($"NodeIndex{i} - {hit.collider}");
                    reachableIndices.Add(i);
                }
            }
            Debug.Log(help);

            commands.Dispose();
            hits.Dispose();

            return reachableIndices.ToArray();
        }

        public RaycastNodes(int worldLayerMask, NativeArray<RuntimePathNode> nodes, float3 originPosition)
        {
            this.worldLayerMask = worldLayerMask;
            this.nodes = nodes;
            this.originPosition = originPosition;
        }
    }
}