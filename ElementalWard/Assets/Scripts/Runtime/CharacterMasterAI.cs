using System;
using System.Collections.Generic;
using ElementalWard.Navigation;
using EntityStates;
using KinematicCharacterController;
using Nebula;
using Nebula.Serialization;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace ElementalWard
{
    [RequireComponent(typeof(CharacterMaster))]
    public class CharacterMasterAI : MonoBehaviour
    {
        public const float TIME_BETWEEN_AI_UPDATE = 0.2f;

        public EntityStateMachine aiStateMachine;

        [Tooltip("When a target is obtained, the aiStateMachine is set to this state.")]
        [SerializableSystemType.RequiredBaseType(typeof(BaseAIState))]
        public SerializableSystemType targetFoundState;
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
        public AITarget CurrentTarget
        {
            get
            {
                return _currentTarget;
            }
            private set
            {
                if (_currentTarget == value)
                    return;

                if (_currentTarget.healthComponent)
                    _currentTarget.healthComponent.OnDeath -= OnTargetDeath;
                _currentTarget = value;
                if (_currentTarget.healthComponent)
                    _currentTarget.healthComponent.OnDeath += OnTargetDeath; 
                OnTargetChanged();
            }
        }
        private AITarget _currentTarget;
#if UNITY_EDITOR
        public bool drawPath;
#endif
        [Nebula.ReadOnly, SerializeField]
        private List<Vector3> _path = new List<Vector3>();
        
        private int _pathIndex;
        private Vector3 _currentWaypoint;
        private float _distanceFromCurrentWaypoint;
        private float _updateStopwatch;
        private Vector3 _pathfindingMovementVector;
        private Quaternion _pathfindingLookRotation;

        private static GlobalBaseAIUpdater globalUpdater;
        [SystemInitializer]
        private static void SystemInitialzer()
        {
            globalUpdater = new GlobalBaseAIUpdater();
        }
        private void OnTargetDeath(HealthComponent hc)
        {
            if (CurrentTarget.healthComponent && _currentTarget.healthComponent == hc)
                CurrentTarget = AITarget.Invalid;
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
            _currentTarget = AITarget.Invalid;
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

        private void Update()
        {
            if(aiStateMachine.CurrentState is not BaseAIState)
            {
                BodyInputBank.LookRotation = _pathfindingLookRotation;
                BodyInputBank.moveVector = _pathfindingMovementVector;
                BodyInputBank.jumpButton.PushState(false);
                BodyInputBank.sprintButton.PushState(false);
                BodyInputBank.skill1Button.PushState(false);
                return;
            }
            BaseAIState aiState = aiStateMachine.CurrentState as BaseAIState;
            AIInputs inputs = aiState.GenerateInputs();
            BodyInputBank.LookRotation = _pathfindingLookRotation;
            BodyInputBank.moveVector = _pathfindingMovementVector;
            BodyInputBank.AimDirection = inputs.aimDir;
            BodyInputBank.jumpButton.PushState(inputs.jumpPressed);
            BodyInputBank.sprintButton.PushState(inputs.sprintPressed);
            BodyInputBank.skill1Button.PushState(inputs.skill1Pressed);
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
            if (!CurrentTarget.IsValid && CurrentTarget.wasAlive)
                CurrentTarget = AITarget.Invalid;

            if(!CurrentTarget.IsValid)
                ScanForTargetNearby();

            if (_path.Count == 0)
            {
                _pathfindingMovementVector = Vector3.zero;
                _pathfindingLookRotation = CurrentBodyComponents.transform.rotation;
                return;
            }

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

                if (!healthComponent.IsAlive)
                    continue;

                if(healthComponent.hurtBoxGroup && !encounteredMainHurtboxes.Contains(healthComponent.hurtBoxGroup.MainHurtBox))
                {
                    encounteredMainHurtboxes.Add(healthComponent.hurtBoxGroup.MainHurtBox);
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
                    var aiPos = CurrentBodyComponents.transform.position;
                    var toHurtbox = hurtBox.transform.position - aiPos;
                    toHurtbox.y = 0;

                    var dot = Vector3.Dot(toHurtbox.normalized, CurrentBodyComponents.transform.forward);
                    if(!(dot > Mathf.Cos(awarenessAngle * 0.5f * Mathf.Deg2Rad)))
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
            }
        }

        public void SetTargetFromDamageReport(DamageReport damageReport)
        {
            if (!damageReport.attackerBody)
                return;

            var attackerBody = damageReport.attackerBody;
            
            //Do not set the target if the attacker is the target.
            if (CurrentTarget.IsValid && CurrentTarget.body == attackerBody)
                return;

            CurrentTarget = new AITarget(damageReport.attackerBody.gameObject);
            return;
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
            _pathfindingMovementVector = movementDir;
            _pathfindingLookRotation = Quaternion.LookRotation(movementDir, BodyMotor ? BodyMotor.CharacterUp : Vector3.up);
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        private void GetBodyComponents(CharacterBody obj)
        {
            CurrentBodyComponents = new BodyComponents(obj.gameObject);
            _pathfindingLookRotation = CurrentBodyComponents.transform.rotation;
            enabled = true;
        }

        private void OnDestroy()
        {
            Master.OnBodySpawned -= GetBodyComponents;
            Master.OnBodyLost -= Master_OnBodyLost;
        }

        private void OnTargetChanged()
        {
            if (!aiStateMachine)
                return;

            aiStateMachine.SetNextState(CurrentTarget.IsValid ? EntityStateCatalog.InstantiateState(targetFoundState) : new Idle());
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Color c = Color.red;
            UnityEditor.Handles.color = c;

            Vector3 rotatedForward = Quaternion.Euler(0, -awarenessAngle * 0.5f, 0) * transform.forward;

            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, rotatedForward, awarenessAngle, awarenessRange);

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
        public struct AIInputs
        {
            public Vector2 movementInput;
            public Vector3 aimDir;
            public Vector2 scrollInput;
            public bool jumpPressed;
            public bool sprintPressed;
            public bool skill1Pressed;
        }

        public readonly struct BodyComponents
        {
            public bool IsValid => body;
            public static readonly BodyComponents Invalid = default;
            public readonly GameObject gameObject;
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
                gameObject = obj;
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
        public class AITarget
        {
            public static readonly AITarget Invalid = new AITarget(null);
            public readonly CharacterBody body;
            public readonly Transform bodyTransform;
            public bool IsValid
            {
                get
                {
                    return body;
                }
            }
            public Vector3? Position
            {
                get
                {
                    if (!IsValid)
                        return null;
                    if (!IsAlive)
                        return null;

                    return _motor ? _motor.TransientPosition : bodyTransform.position;
                }
            }
            public readonly HealthComponent healthComponent;
            public bool IsAlive
            {
                get
                {
                    if (!healthComponent)
                        return false;

                    if(!healthComponent.IsAlive)
                    {
                        if (!wasAlive)
                            wasAlive = true;
                        return false;
                    }
                    return true;
                }
            }
            public bool wasAlive;
            public readonly HurtBox mainHurtBox;
            private readonly KinematicCharacterMotor _motor;

            public bool HasLOS(CharacterBody aiBody, out Vector3 aimDirection, out RaycastHit raycastHit)
            {
                var origin = aiBody.transform.position;
                var hurtboxPos = mainHurtBox ? mainHurtBox.transform.position : body.transform.position;

                aimDirection = hurtboxPos - origin;
                if (Physics.Raycast(origin, aimDirection, out raycastHit, 1024, LayerIndex.CommonMasks.Bullet))
                {
                    return mainHurtBox ? mainHurtBox.ColliderID == raycastHit.colliderInstanceID : false;
                }
                return false;
            }

            public AITarget(GameObject targetGameObject)
            {
                if (!targetGameObject)
                    return;

                body = targetGameObject.GetComponent<CharacterBody>();
                bodyTransform = body.transform;
                healthComponent = body.HealthComponent;
                wasAlive = false;
                _motor = targetGameObject.GetComponent<KinematicCharacterMotor>();

                var hurtBoxGroup = healthComponent.hurtBoxGroup;
                mainHurtBox = hurtBoxGroup ? hurtBoxGroup.MainHurtBox : null;
            }

            public static bool operator== (AITarget lhs, AITarget rhs)
            {
                if (lhs is null || rhs is null)
                    return false;

                if ((!lhs.IsValid && rhs.IsValid) || (lhs.IsValid && !rhs.IsValid))
                    return false;

                return lhs.body == rhs.body;
            }

            public static bool operator!= (AITarget lhs, AITarget rhs)
            {
                return !(lhs == rhs);
            }

            public override bool Equals(object obj)
            {
                if(obj is AITarget aiTarget)
                {
                    return this == aiTarget;
                }
                return false;
            }

            public override int GetHashCode()
            {
                if (!IsValid)
                    return -1;
                return body.GetHashCode();
            }
        }
    }
}