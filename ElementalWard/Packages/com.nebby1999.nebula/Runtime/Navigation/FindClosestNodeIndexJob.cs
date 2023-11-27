using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// Job used to find the closest node relative to a position.
    /// <br>For immediate return with a -1 value, position should be an infinite value on either axis.</br>
    /// </summary>
    public struct FindClosestNodeIndexJob : IJob
    {
        public float3 position;
        [Unity.Collections.ReadOnly]
        public NativeArray<RuntimePathNode> nodes;
        [Unity.Collections.ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> reachableIndices;
        public NativeReference<int> result;

        private float _distance;
        public void Execute()
        {
            if(math.any(math.isinf(position)))
            {
                result.Value = -1;
                return;
            }
            _distance = float.MaxValue;
            if (!reachableIndices.IsCreated)
            {
                for(int i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    var distanceBetween = math.distancesq(node.worldPosition, position);
                    if (distanceBetween > _distance)
                        continue;

                    _distance = distanceBetween;
                    result.Value = i;
                }
                return;
            }
            foreach(int index in reachableIndices)
            {
                var node = nodes[index];
                var distanceBetween = math.distancesq(node.worldPosition, position);
                if (distanceBetween > _distance)
                    continue;

                _distance = distanceBetween;
                result.Value = index;
            }
        }
    }
}