using ElementalWard.Navigation;
using Nebula;
using Nebula.Navigation;
using System.Collections;
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
        public int Count => Instances.Count;
        public List<T> Instances => InstanceTracker.GetInstances<T>();
    }

    public class GlobalNavigationAgentUpdater : GlobalUpdater<NavigationAgent>
    {
        private float stopwatch;

        public GlobalNavigationAgentUpdater()
        {
            ElementalWardApplication.OnFixedUpdate -= OnGlobalFixedUpdate;
            ElementalWardApplication.OnFixedUpdate += OnGlobalFixedUpdate;
        }

        ~GlobalNavigationAgentUpdater()
        {
            ElementalWardApplication.OnFixedUpdate -= OnGlobalFixedUpdate;
        }

        private void OnGlobalFixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if(stopwatch > NavigationAgent.TIME_BETWEEN_NAVIGATION_UPDATE)
            {
                stopwatch -= NavigationAgent.TIME_BETWEEN_NAVIGATION_UPDATE;
                UpdateNavigation();
            }
        }

        private void UpdateNavigation()
        {
            var instanceCount = Count;
            var instances = Instances;

            if (instanceCount == 0 || !SceneNavigationSystem.HasGraphs)
                return;

            for(int i = 0; i < Count; i++)
            {
                UpdatePathIndividual(instances[i]);
            }
        }

        private void UpdatePathIndividual(NavigationAgent agent)
        {
            if (!agent.AskForPath)
                return;

            var dataProvider = agent.NavigationDataProvider;
            SceneNavigationSystem.PathRequest request = new SceneNavigationSystem.PathRequest
            {
                actorHeightHalved = dataProvider.AgentHeight / 2,
                start = dataProvider.StartPosition,
                end = agent.TargetPos,
                graphProvider = dataProvider.IsFlying ? SceneNavigationSystem.AirNodeProvider : SceneNavigationSystem.GroundNodeProvider
            };
            agent.StartNavigationCoroutine(request);
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