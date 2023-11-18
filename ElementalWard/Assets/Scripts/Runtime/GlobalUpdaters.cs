using ElementalWard.Navigation;
using Nebula;
using Nebula.Navigation;
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
            if (instanceCount == 0 || !SceneNavigationSystem.HasGraphs)
                return;

            NativeArray<FindPathJob> jobs = new NativeArray<FindPathJob>(instanceCount, Allocator.Temp);
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(instanceCount, Allocator.Temp);
            SceneNavigationSystem.PathRequestResult[] requestResults = new SceneNavigationSystem.PathRequestResult[instanceCount];

            for (int i = 0; i < instanceCount; i++)
            {
                var baseAIInstance = instances[i];
                Vector3 bodyPos = baseAIInstance.BodyPosition ?? Vector3.positiveInfinity;
                Vector3 targetPos = baseAIInstance.CurrentTarget.Position ?? Vector3.positiveInfinity;
                float capsuleHeight = baseAIInstance.CurrentBodyComponents.characterMotorController.IsFlying ? 1 : baseAIInstance.BodyCapsuleHeight;
                float capsuleRadius = baseAIInstance.BodyCapsuleRadius;
                float jumpStrength = baseAIInstance.BodyJumpStrength;

                SceneNavigationSystem.PathRequest request = new SceneNavigationSystem.PathRequest
                {
                    actorHeightHalved = capsuleHeight / 2,
                    start = bodyPos,
                    end = targetPos,
                    graphProvider = baseAIInstance.CurrentBodyComponents.characterMotorController.IsFlying ? SceneNavigationSystem.AirNodeProvider : SceneNavigationSystem.GroundNodeProvider
                };

                SceneNavigationSystem.PathRequestResult requestResult = SceneNavigationSystem.RequestPath(request);

                requestResults[i] = requestResult;
                jobs[i] = requestResult.findPathJob;
                jobHandles[i] = requestResult.ScheduleFindPathJob();
            }

            JobHandle.CompleteAll(jobHandles);

            for (int i = 0; i < instanceCount; i++)
            {
                var job = jobs[i];
#if DEBUG
                try
                {
#endif
                    instances[i].UpdatePath(job.result);
#if DEBUG
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    job.result.Dispose();
                    requestResults[i].Dispose();
                }
#endif
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
            for (int i = 0; i < count; i++)
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