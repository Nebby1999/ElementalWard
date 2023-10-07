using System;
using Unity.Collections;
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