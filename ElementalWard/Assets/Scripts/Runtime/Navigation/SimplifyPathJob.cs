using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ElementalWard.Navigation
{
    [BurstCompile]
    public struct SimplifyPathJob : IJob
    {
        public NativeList<float3> inputPath;
        public NativeList<float3> outputPath;
        public void Execute()
        {
            float3 directionOld = float3.zero;

            for(int i = 1; i < inputPath.Length; i++)
            {
                float3 directionNew = inputPath[i - 1] - inputPath[i];
                bool3 isAnyDirectionNew = directionNew != directionOld;
                if(isAnyDirectionNew.x || isAnyDirectionNew.y || isAnyDirectionNew.z)
                {
                    outputPath.Add(directionNew);
                }
                directionOld = directionNew;
            }
        }
    }
}