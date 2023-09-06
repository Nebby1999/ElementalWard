using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace ElementalWard
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct LookAtJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<bool> allowVerticalRotation;
        [ReadOnly]
        public NativeArray<float3> desiredLookPosition;

        public void Execute(int index, TransformAccess transform)
        {
            float3 transformPosition = transform.position;
            float3 lookAtPosition = desiredLookPosition[index];
            lookAtPosition.y = allowVerticalRotation[index] ? lookAtPosition.y : transformPosition.y;
            float3 relativePos = lookAtPosition - transformPosition;
            quaternion rot = quaternion.LookRotationSafe(relativePos, math.up());
            transform.rotation = rot;
        }
    }
}