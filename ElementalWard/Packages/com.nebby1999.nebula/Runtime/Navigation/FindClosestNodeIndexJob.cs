using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Nebula.Navigation
{
    public struct FindClosestNodeIndexJob : IJob
    {
        public float3 position;
        [Unity.Collections.ReadOnly]
        public NativeArray<RuntimePathNode> nodes;
        public NativeReference<int> result;

        private float _distance;
        public void Execute()
        {
            _distance = float.MaxValue;
            for(int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var distanceBetween = math.distance(node.worldPosition, position);
                if(distanceBetween < _distance)
                {
                    _distance = distanceBetween;
                    result.Value = i;
                }
            }
        }
    }
}