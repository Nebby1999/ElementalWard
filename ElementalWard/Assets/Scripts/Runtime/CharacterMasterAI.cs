//using ElementalWard.Navigation;
using ElementalWard.Navigation;
using EntityStates;
using KinematicCharacterController;
using Nebula;
using Nebula.Serialization;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(CharacterMaster))]
    public class CharacterMasterAI : MonoBehaviour, INavigationAgentDataProvider
    {
        public const float TIME_BETWEEN_AI_UPDATE = 0.25f;

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
        public float BodyJumpStrength => CurrentBodyComponents.body ? CurrentBodyComponents.body.JumpStrength : 0;
        public TeamIndex BodyTeamIndex => CurrentBodyComponents.teamComponent ? CurrentBodyComponents.teamComponent.CurrentTeamIndex : TeamIndex.None;
        public KinematicCharacterMotor BodyMotor => CurrentBodyComponents.characterMotorController.Motor;
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
        public float AgentHeight => BodyMotor ? BodyMotor.Capsule.height : 2;
        public float AgentRadius => BodyMotor ? BodyMotor.Capsule.radius : 0.5f;
        public Vector3 Target => CurrentTarget?.Position ?? Vector3.positiveInfinity;
        public Vector3 Start => BodyMotor ? BodyMotor.TransientPosition : Vector3.positiveInfinity;

        public bool IsAgentFlying => CurrentBodyComponents.characterMotorController ? CurrentBodyComponents.characterMotorController.IsFlying : false;

        private float _updateStopwatch;

        private void OnTargetDeath(HealthComponent hc)
        {
            if (CurrentTarget.healthComponent && _currentTarget.healthComponent == hc)
                CurrentTarget = AITarget.Invalid;
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
            /*if (aiStateMachine.CurrentState is not BaseAIState)
            {
                AskForPath = false;
                BodyInputBank.LookRotation = _pathfindingLookRotation;
                BodyInputBank.moveVector = _pathfindingMovementVector;
                BodyInputBank.jumpButton.PushState(false);
                BodyInputBank.sprintButton.PushState(false);
                BodyInputBank.primaryButton.PushState(false);
                return;
            }
            BaseAIState aiState = aiStateMachine.CurrentState as BaseAIState;
            AskForPath = aiState.AskForNewPath;
            AIInputs inputs = aiState.GenerateInputs();
            BodyInputBank.LookRotation = _pathfindingLookRotation;
            BodyInputBank.moveVector = _pathfindingMovementVector;
            BodyInputBank.AimDirection = inputs.aimDir;
            BodyInputBank.jumpButton.PushState(inputs.jumpPressed);
            BodyInputBank.sprintButton.PushState(inputs.sprintPressed);
            BodyInputBank.primaryButton.PushState(inputs.primaryPressed);
            BodyInputBank.secondaryButton.PushState(inputs.secondaryPressed);
            BodyInputBank.utilityButton.PushState(inputs.utilityPressed);
            BodyInputBank.specialButton.PushState(inputs.specialPressed);*/
        }

        private void FixedUpdate()
        {
            if (!CurrentBodyComponents.IsValid)
                return;
            var deltaTime = Time.fixedDeltaTime;
            _updateStopwatch += deltaTime;
            if (_updateStopwatch > TIME_BETWEEN_AI_UPDATE)
            {
                _updateStopwatch = 0;
                Tick(deltaTime);
            }
        }

        private void Tick(float deltaTime)
        {
            if (!CurrentTarget.IsValid && CurrentTarget.wasAlive)
                CurrentTarget = AITarget.Invalid;

            if (!CurrentTarget.IsValid)
                ScanForTargetNearby();

            /*if (_path.Count == 0)
            {
                /*_pathfindingMovementVector = Vector3.zero;
                _pathfindingLookRotation = CurrentBodyComponents.transform.rotation;
                return;
            }*/

            ProcessPath();
        }

        private void ScanForTargetNearby()
        {
            Collider[] potentialTargetHurtboxes = Physics.OverlapSphere(Start, awarenessRange, LayerIndex.entityPrecise.Mask);

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

                if (healthComponent.hurtBoxGroup && !encounteredMainHurtboxes.Contains(healthComponent.hurtBoxGroup.MainHurtBox))
                {
                    encounteredMainHurtboxes.Add(healthComponent.hurtBoxGroup.MainHurtBox);
                }
            }

            if (encounteredMainHurtboxes.Count == 0)
                return;

            Vector3 bodyAimPosition = CurrentBodyComponents.body.AimOriginTransform.position;
            foreach (var hurtBox in encounteredMainHurtboxes)
            {
                //Angle check should only be done if this instance has an awareness angle less than 360.
                if (awarenessAngle < 360f)
                {
                    var aiPos = CurrentBodyComponents.transform.position;
                    var toHurtbox = hurtBox.transform.position - aiPos;
                    toHurtbox.y = 0;

                    var dot = Vector3.Dot(toHurtbox.normalized, CurrentBodyComponents.transform.forward);
                    if (!(dot > Mathf.Cos(awarenessAngle * 0.5f * Mathf.Deg2Rad)))
                    {
                        continue;
                    }
                }

                var dir = hurtBox.transform.position - bodyAimPosition;
                if (Physics.Raycast(bodyAimPosition, dir, out var hit, awarenessRange * 2, LayerIndex.CommonMasks.Bullet))
                {
                    if (hit.colliderInstanceID == hurtBox.ColliderID)
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
            /*_currentWaypoint = _path[_pathIndex];
            _distanceFromCurrentWaypoint = Vector3.Distance(_currentWaypoint, BodyPosition.Value);
            if (_distanceFromCurrentWaypoint < 0.7f)
            {
                _pathIndex++;
                var num = _path.Count - 1;
                if (_pathIndex >= num)
                    _pathIndex = num;
                return;
            }

            var vector = _currentWaypoint - BodyPosition;
            var movementDir = vector.Value.normalized;
            /*_pathfindingMovementVector = movementDir;
            _pathfindingLookRotation = Quaternion.LookRotation(movementDir, BodyMotor ? BodyMotor.CharacterUp : Vector3.up);*/
        }


        private void GetBodyComponents(CharacterBody obj)
        {
            CurrentBodyComponents = new BodyComponents(obj.gameObject);
            //_pathfindingLookRotation = CurrentBodyComponents.transform.rotation;
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
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.red;

            Vector3 rotatedForward = Quaternion.Euler(0, -awarenessAngle * 0.5f, 0) * transform.forward;

            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, rotatedForward, awarenessAngle, awarenessRange);
        }
#endif
        public struct AIInputs
        {
            public Vector2 movementInput;
            public Vector3 aimDir;
            public Vector2 scrollInput;
            public bool jumpPressed;
            public bool sprintPressed;
            public bool primaryPressed;
            public bool secondaryPressed;
            public bool utilityPressed;
            public bool specialPressed;
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
            public readonly CharacterMotorController characterMotorController;
            public readonly SkillManager skillManager;

            public BodyComponents(GameObject obj)
            {
                gameObject = obj;
                body = obj.GetComponent<CharacterBody>();
                transform = body.transform;
                teamComponent = obj.GetComponent<TeamComponent>();
                inputBank = obj.GetComponent<CharacterInputBank>();
                skillManager = obj.GetComponent<SkillManager>();
                characterMotorController = obj.GetComponent<CharacterMotorController>();
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

                    if (!healthComponent.IsAlive)
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

            public static bool operator ==(AITarget lhs, AITarget rhs)
            {
                if (lhs is null || rhs is null)
                    return false;

                if ((!lhs.IsValid && rhs.IsValid) || (lhs.IsValid && !rhs.IsValid))
                    return false;

                return lhs.body == rhs.body;
            }

            public static bool operator !=(AITarget lhs, AITarget rhs)
            {
                return !(lhs == rhs);
            }

            public override bool Equals(object obj)
            {
                if (obj is AITarget aiTarget)
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