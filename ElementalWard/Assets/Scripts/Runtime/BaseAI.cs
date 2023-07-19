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
        public const float TIME_BETWEEN_AI_UPDATE = 0.2f;

        [Range(0, 360)]
        public float awarenessAngle;
        public float awarenessRange;
        public CharacterMaster Master { get; private set; }
        public BodyComponents CurrentBodyComponents { get; private set; }
        public CharacterInputBank BodyInputBank => CurrentBodyComponents.inputBank;
        public float BodyCapsuleHeight => CurrentBodyComponents.motorCapsule ? CurrentBodyComponents.motorCapsule.height : 1;
        public float BodyCapsuleRadius => CurrentBodyComponents.motorCapsule ? CurrentBodyComponents.motorCapsule.radius : 0.5f;
        public float BodyJumpStrength => CurrentBodyComponents.body ? CurrentBodyComponents.body.JumpStrength : 0;
        public TeamIndex BodyTeamIndex => CurrentBodyComponents.teamComponent ? CurrentBodyComponents.teamComponent.CurrentTeamIndex : TeamIndex.None;
        public KinematicCharacterMotor BodyMotor => CurrentBodyComponents.motor;
        public Vector3 BodyPosition => CurrentBodyComponents.motor ? CurrentBodyComponents.motor.InitialSimulationPosition : Vector3.zero;
        public AITarget CurrentTarget { get; private set; }
#if UNITY_EDITOR
        public bool drawPath;
#endif
        [Nebula.ReadOnly, SerializeField]
        private List<Vector3> _path = new List<Vector3>();
        
        private int _pathIndex;
        private Vector3 _currentWaypoint;
        private float _distanceFromCurrentWaypoint;
        private float _updateStopwatch;

        private static GlobalBaseAIUpdater globalUpdater;
        [SystemInitializer]
        private static void SystemInitialzer()
        {
            globalUpdater = new GlobalBaseAIUpdater();
        }

        public void UpdatePath(NativeList<float3> result)
        {
            _path.Clear();
            for(int i = 0; i < result.Length; i++)
            {
                _path.Add(result[i]);
            }
            _pathIndex = 1;
            if (_pathIndex > result.Length - 1)
                _pathIndex = result.Length - 1;
        }

        private void Awake()
        {
            Master = GetComponent<CharacterMaster>();
            Master.OnBodySpawned += GetBodyComponents;
            Master.OnBodyLost += Master_OnBodyLost;
            enabled = false;
        }

        private void Master_OnBodyLost()
        {
            CurrentBodyComponents = default;
            enabled = false;
        }

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        private void FixedUpdate()
        {
            if (!CurrentBodyComponents.IsValid)
                return;
            var deltaTime = Time.fixedDeltaTime;
            _updateStopwatch += deltaTime;
            if(_updateStopwatch > TIME_BETWEEN_AI_UPDATE)
            {
                _updateStopwatch = 0;
                Tick(deltaTime);
            }
        }

        private void Tick(float deltaTime)
        {
            if(!CurrentTarget.IsValid || !CurrentTarget.IsAlive)
            ScanForTargetNearby();

            if (_path.Count == 0)
                return;

            ProcessPath();
        }

        private void ScanForTargetNearby()
        {
            Collider[] potentialTargetHurtboxes = Physics.OverlapSphere(BodyPosition, awarenessRange, LayerIndex.entityPrecise.Mask);

            List<HurtBox> encounteredMainHurtboxes = new List<HurtBox>();
            foreach (Collider col in potentialTargetHurtboxes)
            {
                if (!col.TryGetComponent<HurtBox>(out var hurtBox))
                    continue;

                bool? isEnemy = TeamCatalog.GetTeamInteraction(BodyTeamIndex, hurtBox.TeamIndex);
                if (isEnemy == false)
                    continue;

                var healthComponent = hurtBox.HealthComponent;
                if (!healthComponent || healthComponent == CurrentBodyComponents.body.HealthComponent)
                    continue;

                if(healthComponent.HurtboxGroup && !encounteredMainHurtboxes.Contains(healthComponent.HurtboxGroup.MainHurtBox))
                {
                    encounteredMainHurtboxes.Add(healthComponent.HurtboxGroup.MainHurtBox);
                }
            }

            if (encounteredMainHurtboxes.Count == 0)
                return;

            Vector3 bodyAimPosition = CurrentBodyComponents.body.AimOriginTransform.position;
            foreach(var hurtBox in encounteredMainHurtboxes)
            {
                //Angle check should only be done if this instance has an awareness angle less than 360.
                if(awarenessAngle < 360f)
                {
                    var hurtboxPos = hurtBox.transform.position.normalized;
                    hurtboxPos.y = 0;
                    var bodyAimForward = CurrentBodyComponents.body.AimOriginTransform.forward;
                    bodyAimForward.y = 0;

                    var max = awarenessAngle / 2;
                    var min = -max;
                    var angle = Vector3.SignedAngle(hurtboxPos, bodyAimForward, Vector3.up);
                    
                    //Hurtbox is not inside vision cone, continue to next candidate
                    if(!(angle > min && angle < max))
                    {
                        continue;
                    }
                }

                var dir = hurtBox.transform.position - bodyAimPosition;
                if(Physics.Raycast(bodyAimPosition, dir, out var hit, awarenessRange * 2, LayerIndex.CommonMasks.Bullet))
                {
                    if(hit.colliderInstanceID == hurtBox.ColliderID)
                    {
                        CurrentTarget = new AITarget(hurtBox.HealthComponent.gameObject);
                        return;
                    }
                }

                /*if(Physics.Raycast(bodyAimPosition, dir.normalized, out RaycastHit hit, awarenessRange * 2, LayerIndex.CommonMasks.Bullet))
                {
                    if(hit.colliderInstanceID == hurtbox.GetInstanceID())
                        Debug.Log($"LOS to {hurtbox}", hurtbox);
                }
               */
            }
        }

        private void ProcessPath()
        {
            _currentWaypoint = _path[_pathIndex];
            _distanceFromCurrentWaypoint = Vector3.Distance(_currentWaypoint, BodyPosition);
            if (_distanceFromCurrentWaypoint < 0.7f)
            {
                _pathIndex++;
                var num = _path.Count - 1;
                if (_pathIndex >= num)
                    _pathIndex = num;
                return;
            }

            var vector = _currentWaypoint - BodyPosition;
            var movementDir = vector.normalized;
            if(BodyInputBank)
            {
                BodyInputBank.moveVector = movementDir;
                BodyInputBank.LookRotation = Quaternion.LookRotation(movementDir, BodyMotor ? BodyMotor.CharacterUp : Vector3.up);
            }
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        private void GetBodyComponents(CharacterBody obj)
        {
            CurrentBodyComponents = new BodyComponents(obj.gameObject);
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
            float radius = awarenessAngle;
            float rayRange = awarenessRange;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, rayRange);
            Gizmos.color = Color.black;

            float halfRadius = radius / 2.0f;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-halfRadius, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(halfRadius, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * transform.forward;
            Vector3 rightRayDirection = rightRayRotation * transform.forward;
            Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
            Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);


            if (!drawPath)
                return;

            var previousPos = Vector3.zero;
            int pathCount = _path.Count;
            for (int i = 0; i < pathCount; i++)
            {
                var pos = _path[i];
                if (i == 0)
                {
                    Gizmos.color = Color.red;
                }
                else if (i == pathCount - 1)
                {
                    Gizmos.color = Color.green;
                }
                else if(i == _pathIndex)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.black;
                }

                Gizmos.DrawSphere(_path[i], AStarNodeGrid.NODE_RADIUS);

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
        public readonly struct BodyComponents
        {
            public bool IsValid => body;
            public static BodyComponents Invalid = default;
            public readonly CharacterBody body;
            public readonly Transform transform;
            public readonly TeamComponent teamComponent;
            public readonly CharacterInputBank inputBank;
            public readonly ICharacterMovementController characterMovementController;
            public readonly KinematicCharacterMotor motor;
            public readonly CapsuleCollider motorCapsule;
            public readonly bool isGround;
            public readonly bool isFlying;

            public BodyComponents(GameObject obj)
            {
                body = obj.GetComponent<CharacterBody>();
                transform = body.transform;
                teamComponent = obj.GetComponent<TeamComponent>();
                inputBank = obj.GetComponent<CharacterInputBank>();
                characterMovementController = obj.GetComponent<ICharacterMovementController>();
                motor = characterMovementController == null ? null : characterMovementController.Motor;
                motorCapsule = motor ? motor.Capsule : null;
                isGround = characterMovementController is GroundedCharacterMovementController;
                isFlying = characterMovementController is FlyingCharacterMovementController;
            }
        }
        public readonly struct AITarget
        {
            public static AITarget Invalid = default;
            public readonly CharacterBody body;
            public readonly Transform bodyTransform;
            public bool IsValid => body;
            public Vector3? Position => _motor ? _motor.TransientPosition  : IsValid ? bodyTransform.position : null;
            public readonly HealthComponent healthComponent;
            public bool IsAlive => healthComponent ? healthComponent.IsAlive : false;
            public readonly HurtBox mainHurtBox;
            private readonly KinematicCharacterMotor _motor;

            public bool HasLOS(CharacterBody aiBody)
            {
                var origin = aiBody.transform.position;
                var hurtboxPos = mainHurtBox ? mainHurtBox.transform.position : body.transform.position;

                var dir = hurtboxPos - origin;
                if (Physics.Raycast(origin, dir, out var hitInfo, 1024, LayerIndex.CommonMasks.Bullet))
                {
                    return mainHurtBox ? mainHurtBox.ColliderID == hitInfo.colliderInstanceID : false;
                }
                return false;
            }

            public AITarget(GameObject targetGameObject)
            {
                body = targetGameObject.GetComponent<CharacterBody>();
                bodyTransform = body.transform;
                healthComponent = body.HealthComponent;
                _motor = targetGameObject.GetComponent<KinematicCharacterMotor>();

                var hurtBoxGroup = healthComponent.HurtboxGroup;
                if (hurtBoxGroup)
                {
                    mainHurtBox = hurtBoxGroup.MainHurtBox;
                }
                mainHurtBox = null;
            }
        }
    }
}