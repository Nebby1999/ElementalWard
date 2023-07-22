using ElementalWard.Navigation;
using Nebula;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

namespace ElementalWard
{
    public abstract class GlobalUpdater<T> where T : MonoBehaviour
    {
        public int InstanceCount => Instances.Count;
        public List<T> Instances => InstanceTracker.GetInstances<T>();
    }

    public class GlobalBaseAIUpdater : GlobalUpdater<CharacterMasterAI>
    {
        private float stopwatch;
        public GlobalBaseAIUpdater()
        {
            ElementalWardApplication.OnFixedUpdate -= OnGlobalFixedUpdate;
            ElementalWardApplication.OnFixedUpdate += OnGlobalFixedUpdate;
        }

        ~GlobalBaseAIUpdater()
        {
            ElementalWardApplication.OnFixedUpdate -= OnGlobalFixedUpdate;
        }

        private void OnGlobalFixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if(stopwatch > CharacterMasterAI.TIME_BETWEEN_AI_UPDATE)
            {
                stopwatch = 0;
                UpdateGlobalAI();
            }
        }

        private void UpdateGlobalAI()
        {
            var instanceCount = InstanceCount;
            var instances = Instances;
            if (!PathfindingSystem.Instance || instanceCount == 0)
                return;

            NativeArray<FindPathJob> jobs = new NativeArray<FindPathJob>(instanceCount, Allocator.Temp);
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(instanceCount, Allocator.Temp);

            for (int i = 0; i < instanceCount; i++)
            {
                var baseAIInstance = instances[i];
                Vector3 bodyPos = baseAIInstance.BodyPosition;
                Vector3? targetPos = baseAIInstance.CurrentTarget.Position;
                float capsuleHeight = baseAIInstance.BodyCapsuleHeight;
                float capsuleRadius = baseAIInstance.BodyCapsuleRadius;
                float jumpStrength = baseAIInstance.BodyJumpStrength;

                if (baseAIInstance.CurrentBodyComponents.isFlying)
                {
                    jobs[i] = PathfindingSystem.Instance.RequestPath(PathfindingSystem.Instance.airNodes, bodyPos, targetPos, capsuleHeight, capsuleRadius, jumpStrength);
                    jobHandles[i] = jobs[i].Schedule();
                    continue;
                }
                if (baseAIInstance.CurrentBodyComponents.isGround)
                {
                    jobs[i] = PathfindingSystem.Instance.RequestPath(PathfindingSystem.Instance.groundNodes, bodyPos, targetPos, capsuleHeight, capsuleRadius, jumpStrength);
                    jobHandles[i] = jobs[i].Schedule();
                    continue;
                }
            }

            JobHandle.CompleteAll(jobHandles);

            for (int i = 0; i < instanceCount; i++)
            {
                var job = jobs[i];
                instances[i].UpdatePath(job.result);
                job.result.Dispose();
            }
            jobs.Dispose();
            jobHandles.Dispose();
        }
    }

    public class Global3DSpriteRendererUpdater : GlobalUpdater<SpriteRenderer3D>
    {
        private TransformAccessArray _accessArray;
        public Global3DSpriteRendererUpdater()
        {
            _accessArray = new TransformAccessArray(0);
            RenderPipelineManager.beginCameraRendering += CharacterRendererLookAt;
        }

        ~Global3DSpriteRendererUpdater()
        {
            _accessArray.Dispose();
        }

        public void UpdateTransformAccessArray()
        {
            _accessArray.Dispose();
            var instances = Instances;
            var count = instances.Count;
            _accessArray = new TransformAccessArray(count);
            for(int i = 0; i < count; i++)
            {
                _accessArray.Add(instances[i].transform);
            }
        }

        private void CharacterRendererLookAt(ScriptableRenderContext arg1, Camera arg2)
        {
            var instances = Instances;
            var count = instances.Count;

            if (count == 0)
                return;

            NativeArray<bool> allowVerticalRotation = new NativeArray<bool>(count, Allocator.TempJob);
            NativeArray<float3> lookAtPosition = new NativeArray<float3>(count, Allocator.TempJob);

            for (int i = 0; i < count; i++)
            {
                allowVerticalRotation[i] = instances[i].allowVerticalRotation;
                lookAtPosition[i] = instances[i].LookAtTransform.position;
            }

            var job = new LookAtJob()
            {
                allowVerticalRotation = allowVerticalRotation,
                desiredLookPosition = lookAtPosition
            };

            JobHandle handle = job.Schedule(_accessArray);
            handle.Complete();

            allowVerticalRotation.Dispose();
            lookAtPosition.Dispose();
        }
    }
}