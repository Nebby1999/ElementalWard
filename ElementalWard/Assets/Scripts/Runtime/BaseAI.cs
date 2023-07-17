using System;
using System.Collections.Generic;
using ElementalWard.Navigation;
using KinematicCharacterController;
using Nebula;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace ElementalWard.AI
{
    [RequireComponent(typeof(CharacterMaster))]
    public class BaseAI : MonoBehaviour
    {
        public readonly struct BodyComponents
        {
            public bool IsValid => body;
            public readonly CharacterBody body;
            public readonly CharacterInputBank inputBank;
            public readonly ICharacterMovementController characterMovementController;
            public readonly KinematicCharacterMotor motor;
            public readonly CapsuleCollider motorCapsule;
            public readonly bool isGround;
            public readonly bool isFlying;

            public BodyComponents(GameObject obj)
            {
                body = obj.GetComponent<CharacterBody>();
                inputBank = obj.GetComponent<CharacterInputBank>();
                characterMovementController = obj.GetComponent<ICharacterMovementController>();
                motor = characterMovementController == null ? null : characterMovementController.Motor;
                motorCapsule = motor ? motor.Capsule : null;
                isGround = characterMovementController is GroundedCharacterMovementController;
                isFlying = characterMovementController is FlyingCharacterMovementController;
            }
        }
        private const float TIME_BETWEEN_AI_UPDATE = 0.2f;
        private static float aiUpdateStopwatch;

        public Transform target;
        public CharacterMaster Master { get; private set; }
        public CharacterInputBank BodyInputBank => bodyComponents.inputBank;
        public float CapsuleHeight => bodyComponents.motorCapsule ? bodyComponents.motorCapsule.height : 1;
        public float CapsuleRadius => bodyComponents.motorCapsule ? bodyComponents.motorCapsule.radius : 0.5f;
        public float JumpStrength => bodyComponents.body ? bodyComponents.body.JumpStrength : 0;
        public KinematicCharacterMotor Motor => bodyComponents.motor;
        public Vector3 BodyPosition => bodyComponents.motor ? bodyComponents.motor.InitialSimulationPosition : Vector3.zero;
#if UNITY_EDITOR
        public bool drawPath;
#endif
        [Nebula.ReadOnly, SerializeField]
        private List<Vector3> path = new List<Vector3>();
        private BodyComponents bodyComponents;
        
        private int pathIndex;
        private Vector3 currentWaypoint;
        private float distanceFromCurrentWaypoint;
        
        [SystemInitializer]
        private static void SystemInitialzer()
        {
            ElementalWardApplication.OnFixedUpdate += OnGlobalFixedUpdate;
        }

        private static void OnGlobalFixedUpdate()
        {
            aiUpdateStopwatch += Time.fixedDeltaTime;
            if (aiUpdateStopwatch > TIME_BETWEEN_AI_UPDATE)
            {
                aiUpdateStopwatch = 0;
                GlobalAIUpdate();
            }
        }

        private static void GlobalAIUpdate()
        {
            var instances = InstanceTracker.GetInstances<BaseAI>();
            int instanceCount = instances.Count;

            if (!PathfindingSystem.Instance || instanceCount == 0)
                return;

            NativeArray<FindPathJob> jobs = new NativeArray<FindPathJob>(instanceCount, Allocator.Temp);
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(instanceCount, Allocator.Temp);

            for (int i = 0; i < instanceCount; i++)
            {
                var baseAIInstance = instances[i];
                Vector3 bodyPos = baseAIInstance.BodyPosition;
                Vector3 targetPos = baseAIInstance.target.position;
                float capsuleHeight = baseAIInstance.CapsuleHeight;
                float capsuleRadius = baseAIInstance.CapsuleRadius;
                float jumpStrength = baseAIInstance.JumpStrength;

                if (baseAIInstance.bodyComponents.isFlying)
                {
                    jobs[i] = PathfindingSystem.Instance.RequestPath(PathfindingSystem.Instance.airNodes, bodyPos, targetPos, capsuleHeight, capsuleRadius, jumpStrength);
                    jobHandles[i] = jobs[i].Schedule();
                    continue;
                }
                if (baseAIInstance.bodyComponents.isGround)
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

        private void UpdatePath(NativeArray<float3> result)
        {
            path.Clear();
            for(int i = 0; i < result.Length; i++)
            {
                path.Add(result[i]);
            }
            pathIndex = 1;
            if (pathIndex > result.Length - 1)
                pathIndex = result.Length - 1;
        }

        private void Awake()
        {
            Master = GetComponent<CharacterMaster>();
            Master.OnBodySpawned += GetBodyComponents;
            Master.OnBodyLost += Master_OnBodyLost;
        }

        private void Master_OnBodyLost()
        {
            bodyComponents = default;
            enabled = false;
        }

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        private void FixedUpdate()
        {
            if (path.Count == 0)
                return;

            ProcessPath();
        }

        private void ProcessPath()
        {
            currentWaypoint = path[pathIndex];
            distanceFromCurrentWaypoint = Vector3.Distance(currentWaypoint, BodyPosition);
            if (distanceFromCurrentWaypoint < 0.7f)
            {
                pathIndex++;
                var num = path.Count - 1;
                if (pathIndex >= num)
                    pathIndex = num;
                return;
            }

            var vector = currentWaypoint - BodyPosition;
            var movementDir = vector.normalized;
            if(BodyInputBank)
            {
                BodyInputBank.moveVector = movementDir;
                BodyInputBank.LookRotation = Quaternion.LookRotation(movementDir, Motor ? Motor.CharacterUp : Vector3.up);
            }
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        private void GetBodyComponents(CharacterBody obj)
        {
            bodyComponents = new BodyComponents(obj.gameObject);
            enabled = true;
        }

        private void OnDestroy()
        {
            Master.OnBodySpawned -= GetBodyComponents;
            Master.OnBodyLost -= Master_OnBodyLost;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var previousPos = Vector3.zero;
            int pathCount = path.Count;
            for (int i = 0; i < pathCount; i++)
            {
                var pos = path[i];
                if (i == 0)
                {
                    Gizmos.color = Color.red;
                }
                else if (i == pathCount - 1)
                {
                    Gizmos.color = Color.green;
                }
                else if(i == pathIndex)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.black;
                }

                Gizmos.DrawSphere(path[i], AStarNodeGrid.NODE_RADIUS);

                if (i == 0)
                {
                    previousPos = pos;
                    continue;
                }

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(previousPos, pos);
                previousPos = pos;
            }
        }
#endif
    }
}