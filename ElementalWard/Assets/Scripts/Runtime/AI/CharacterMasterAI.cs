using ElementalWard.AI;
using ElementalWard.Navigation;
using EntityStates;
using EntityStates.CharacterAI;
using KinematicCharacterController;
using Nebula;
using Nebula.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalWard
{
    [RequireComponent(typeof(CharacterMaster))]
    public class CharacterMasterAI : MonoBehaviour, INavigationAgentDataProvider
    {
        public const float TIME_BETWEEN_AI_UPDATE = 0.25f;

        [SerializeField] private AIDriverData[] _aiDriverData = Array.Empty<AIDriverData>();
        public EntityStateMachine aiStateMachine;
        [SerializableSystemType.RequiredBaseType(typeof(BaseAIState))]
        public SerializableSystemType aiState;

        [Range(0, 360)]
        public float awarenessAngle;
        public float awarenessRange;
        public UnityEvent OnFirstTargetObtained;
        public CharacterMaster Master { get; private set; }
        public CharacterBody Body { get; private set; }
        public CharacterInputBank BodyInputBank { get; private set; }
        public TeamIndex CurrentTeam => Body.TeamComponent.CurrentTeamIndex;
        public KinematicCharacterMotor BodyCharacterMotor { get; private set; }
        public CharacterMotorController BodyMotorController { get; private set; }
        public SkillManager BodySkillManager { get; private set; }
        public AITarget CurrentTarget => DriverEvaluation.target;
        public float AgentHeight => BodyCharacterMotor ? BodyCharacterMotor.Capsule.height : 2;
        public float AgentRadius => BodyCharacterMotor ? BodyCharacterMotor.Capsule.radius : 0.5f;
        public Vector3 Target => CurrentTarget?.Position ?? Vector3.positiveInfinity;
        public Vector3 StartPosition => Body ? Body.transform.position : Vector3.positiveInfinity;
        public Transform AgentTransform => Body ? Body.transform : null;
        public bool IsFlying => BodyMotorController ? BodyMotorController.IsFlying : false;
        public AIDriver[] AIDrivers { get; private set; }
        public AIDriverEvaluation DriverEvaluation { get; private set; }
        public string CurrentDriverName => _currentDriverName;
#if UNITY_EDITOR
        [SerializeField, Nebula.ReadOnly]
#endif
        private string _currentDriverName;
        private float _updateStopwatch;
        private BullseyeSearch _enemySearch;
        private AITarget _enemyTarget;
        private AITarget _friendlyTarget;
        private AIInputs _aiInputs;

        private void Awake()
        {
            _enemyTarget = AITarget.Invalid;
            Master = GetComponent<CharacterMaster>();
            Master.OnBodySpawned += Master_OnBodySpawned;
            Master.OnBodyLost += Master_OnBodyLost;
            _enemySearch = new BullseyeSearch();
            enabled = false;
        }

        private void Start()
        {
            AIDrivers = _aiDriverData.Select(x => x.CreateDriver()).ToArray();
            DriverEvaluation = new AIDriverEvaluation
            {
                aimTarget = AITarget.Invalid,
                target = AITarget.Invalid,
                dominantDriver = null,
            };
        }

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        private void FixedUpdate()
        {
            if (!Body)
                return;

            var deltaTime = Time.fixedDeltaTime;
            _updateStopwatch += deltaTime;
            if (_updateStopwatch > TIME_BETWEEN_AI_UPDATE)
            {
                _updateStopwatch = 0;
                Tick(deltaTime);
            }
            UpdateInputs();
        }

        private void Tick(float deltaTime)
        {
            EnsureEnemyTarget();

            BeginAIDriver(EvaluateDrivers(deltaTime));
        }

        private void EnsureEnemyTarget()
        {
            if (!_enemyTarget.IsValid && _enemyTarget.wasAlive)
                _enemyTarget = AITarget.Invalid;
            var newEnemy = ScanForTargetNearby(0, awarenessRange, true);
            if(newEnemy)
            {
                _enemyTarget = new AITarget(newEnemy.HealthComponent.gameObject);
                OnFirstTargetObtained?.Invoke();
            }
        }

        private void EnsureFriendlyTarget(AIDriver driver = null)
        {
            float minDistance = driver?.MinDistance ?? 0;
            float maxDistance = driver?.MaxDistance ?? awarenessRange;

            if (_friendlyTarget.IsValid && _friendlyTarget.wasAlive)
                _friendlyTarget = AITarget.Invalid;

            var newFriendly = ScanForTargetNearby(minDistance, maxDistance, true);
            if(newFriendly)
            {
                _friendlyTarget = new AITarget(newFriendly.HealthComponent.gameObject);
            }
        }

        private void BeginAIDriver(AIDriverEvaluation newDriver)
        {
            DriverEvaluation = newDriver;
            DriverEvaluation.dominantDriver?.OnSelected();
            _currentDriverName = DriverEvaluation.dominantDriver?.DriverName ?? "None";
        }

        private AIDriverEvaluation EvaluateDrivers(float deltaTime)
        {
            var driverEvaluation = DriverEvaluation;
            for(int i = 0; i < AIDrivers.Length; i++)
            {
                var aiDriver = AIDrivers[i];
                if(!aiDriver.IsAvailable)
                {
                    continue;
                }

                AIDriverEvaluation? newDriver = EvaluateDriverSingle(in driverEvaluation, aiDriver);
                if (!newDriver.HasValue)
                    continue;

                return newDriver.Value;
            }
            return driverEvaluation;
        }
        private AIDriverEvaluation? EvaluateDriverSingle(in AIDriverEvaluation currentEvaluation, AIDriver driver)
        {
            if (!Body || !BodySkillManager)
                return null;

            if(currentEvaluation.dominantDriver?.TransitionOnSkillEnd ?? false)
            {
                var currentEvaluationSkill = BodySkillManager.GetSkillBySkillSlot(currentEvaluation.dominantDriver.RequiredSkillSlot);
                if (!currentEvaluationSkill)
                    return null;

                if (currentEvaluationSkill.IsInSkillState())
                    return null;
            }

            if (driver.NoRepeat && currentEvaluation.dominantDriver == driver)
                return null;

            if (driver.MaxTimesSelected >= 0 && driver.TimesSelected >= driver.MaxTimesSelected)
                return null;

            if(driver.RequiredSkillSlot != SkillSlot.None)
            {
                GenericSkill skill = BodySkillManager.GetSkillBySkillSlot(driver.RequiredSkillSlot);
                if (!skill)
                    return null;

                if (driver.RequireSkillReady && !skill.IsReady())
                    return null;
            }
            if (!driver.SelectionRequiresGrounded && BodyMotorController && BodyMotorController.IsGrounded)
                return null;

            AITarget movementTarget = null;
            switch(driver.TargetType)
            {
                case AIDriverData.TargetType.CurrentEnemy:
                    movementTarget = TargetPassesDriverFilters(_enemyTarget, driver);
                    break;
                case AIDriverData.TargetType.NearestFriendly:
                    EnsureFriendlyTarget(driver);
                    movementTarget = TargetPassesDriverFilters(_friendlyTarget, driver);
                    break;
            }
            if (!movementTarget.IsValid)
                return null;

            AITarget aimTarget = null;
            if(driver.AimType != AIDriverData.AimType.None)
            {
                bool requiresAimTarget = driver.SelectionRequiresAimTarget;
                switch(driver.AimType)
                {
                    case AIDriverData.AimType.AtMoveTarget:
                        aimTarget = movementTarget;
                        break;
                    case AIDriverData.AimType.AtCurrentEnemy:
                        aimTarget = _enemyTarget;
                        break;
                    default:
                        requiresAimTarget = false;
                        break;
                }
                if(requiresAimTarget && (!aimTarget.IsValid))
                {
                    return null;
                }
            }
            return new AIDriverEvaluation
            {
                dominantDriver = driver,
                target = movementTarget,
                aimTarget = aimTarget,
            };
        }

        private AITarget TargetPassesDriverFilters(AITarget targetToCheck, AIDriver driver)
        {
            if (!targetToCheck.IsValid)
                return AITarget.Invalid;

            Vector3 aimPoint = BodyInputBank ? BodyInputBank.AimOrigin : AgentTransform.position;
            Vector3? targetPos = targetToCheck.Position;
            if (!targetPos.HasValue)
                return AITarget.Invalid;
            var distance = Vector3.Distance(aimPoint, targetPos.Value);
            if (distance < driver.MinDistance || distance > driver.MaxDistance)
                return AITarget.Invalid;

            if(driver.SelectionRequiresLOS && !targetToCheck.HasLOS(Body, out var _))
            {
                return AITarget.Invalid;
            }
            return targetToCheck;
        }

        private void UpdateInputs()
        {
            BaseAIState state;
            if((state = aiStateMachine.CurrentState as BaseAIState) != null)
            {
                _aiInputs = state.GenerateAIInputs(in _aiInputs);
            }
            if(BodyInputBank)
            {
                BodyInputBank.primaryButton.PushState(_aiInputs.primaryPressed);
                BodyInputBank.secondaryButton.PushState(_aiInputs.secondaryPressed);
                BodyInputBank.utilityButton.PushState(_aiInputs.utilityPressed);
                BodyInputBank.specialButton.PushState(_aiInputs.specialPressed);
                BodyInputBank.jumpButton.PushState(_aiInputs.jumpPressed);
                BodyInputBank.sprintButton.PushState(_aiInputs.sprintPressed);
                BodyInputBank.moveVector = _aiInputs.movementInput;
                BodyInputBank.elementalScroll = _aiInputs.scrollInput;
                BodyInputBank.AimDirection = _aiInputs.aimDir;
                BodyInputBank.LookRotation = _aiInputs.lookRotation ?? UnityUtil.SafeLookRotation(BodyInputBank.AimDirection);
            }
        }
        private HurtBox ScanForTargetNearby(float minDistance, float maxDistance, bool filterByLos)
        {
            _enemySearch.viewer = Body.gameObject;
            _enemySearch.searchOrigin = Body.transform.position;
            _enemySearch.teamMaskFilter = TeamMask.allButNeutral;
            _enemySearch.teamMaskFilter.RemoveTeam(CurrentTeam);
            _enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
            _enemySearch.minDistanceFilter = minDistance;
            _enemySearch.maxDistanceFilter = maxDistance;
            _enemySearch.searchDirection = BodyInputBank.AimDirection;
            _enemySearch.MaxAngleFilter = awarenessAngle / 2;
            _enemySearch.filterByLoS = filterByLos;
            _enemySearch.RefreshCandidates();
            HurtBox enemy = _enemySearch.GetResults().FirstOrDefault();
            return enemy;
        }

        public void SetTargetFromDamageReport(DamageReport damageReport)
        {
            if (!damageReport.attackerBody)
                return;

            var attackerBody = damageReport.attackerBody;

            //Do not set the target if the attacker is the target.
            if (CurrentTarget.IsValid && CurrentTarget.body == attackerBody)
                return;

            _enemyTarget = new AITarget(damageReport.attackerBody.gameObject);
            OnFirstTargetObtained?.Invoke();
            return;
        }

        public void SetTarget(AITarget other)
        {
            _enemyTarget = new AITarget(other.gameObject);
            OnFirstTargetObtained?.Invoke();
        }


        private void Master_OnBodySpawned(CharacterBody obj)
        {
            Body = obj;
            BodyInputBank = obj.InputBank;
            BodyCharacterMotor = obj.GetComponent<KinematicCharacterMotor>();
            BodyMotorController = obj.GetComponent<CharacterMotorController>();
            BodySkillManager = obj.GetComponent<SkillManager>();
            enabled = true;
            Type type = (Type)aiState;
            if(aiStateMachine && type != null)
            {
                aiStateMachine.SetNextState(EntityStateCatalog.InstantiateState(type));
            }
        }

        private void Master_OnBodyLost()
        {
            Body = null;
            BodyInputBank = null;
            BodyCharacterMotor = null;
            BodyMotorController = null;
            BodySkillManager = null;
            enabled = false;
            if(aiStateMachine)
            {
                aiStateMachine.SetNextStateToMain();
            }
        }

        private void OnDestroy()
        {
            Master.OnBodySpawned -= Master_OnBodySpawned;
            Master.OnBodyLost -= Master_OnBodyLost;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.red;

            Vector3 rotatedForward = Quaternion.Euler(0, -awarenessAngle * 0.5f, 0) * transform.forward;

            UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, rotatedForward, awarenessAngle, awarenessRange);
        }
#endif
        public struct AIInputs
        {
            public Vector3 movementInput;
            public Vector3 aimDir;
            public Quaternion? lookRotation;
            public int scrollInput;
            public bool jumpPressed;
            public bool sprintPressed;
            public bool primaryPressed;
            public bool secondaryPressed;
            public bool utilityPressed;
            public bool specialPressed;
        }

        public struct AIDriverEvaluation
        {
            public AIDriver dominantDriver;
            public AITarget target;
            public AITarget aimTarget;
        }

        public class AITarget
        {
            public static AITarget Invalid => new AITarget(null);
            public readonly CharacterBody body;
            public readonly Transform bodyTransform;
            public readonly GameObject gameObject;
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

                    return bodyTransform.position;
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

            public bool HasLOS(CharacterBody aiBody, out RaycastHit raycastHit)
            {
                var origin = aiBody.AimOriginTransform.position;
                var aimDirection = Position.Value - origin;
                aimDirection.Normalize();

                Debug.DrawLine(origin, aimDirection * 10, Color.magenta, 1);
                if (Physics.Raycast(origin, aimDirection, out raycastHit, 1024, LayerIndex.CommonMasks.Bullet))
                {
                    return mainHurtBox ? mainHurtBox.ColliderID == raycastHit.colliderInstanceID : false;
                }
                return false;
            }

            public bool HasAimLOS(CharacterBody aiBody, out Vector3 aimDirection, out RaycastHit hit)
            {
                var origin = aiBody.AimOriginTransform.position;
                aimDirection = (Position.Value - origin).normalized;

                if (Physics.Raycast(origin, aimDirection, out hit, 1024, LayerIndex.CommonMasks.Bullet))
                {
                    return mainHurtBox ? mainHurtBox.ColliderID == hit.colliderInstanceID : false;
                }
                return false;
            }



            public AITarget(GameObject targetGameObject)
            {
                if (!targetGameObject)
                    return;

                gameObject = targetGameObject;
                body = targetGameObject.GetComponent<CharacterBody>();
                bodyTransform = body.transform;
                healthComponent = body.HealthComponent;
                wasAlive = false;

                var hurtBoxGroup = healthComponent.hurtBoxGroup;
                mainHurtBox = hurtBoxGroup ? hurtBoxGroup.MainHurtBox : null;
            }

            public static bool operator ==(AITarget lhs, AITarget rhs)
            {
                if (lhs is null && rhs is null)
                    return true;

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