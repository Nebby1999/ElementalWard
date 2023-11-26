using Nebula;
using UnityEngine;
using static ElementalWard.AI.AIDriverData;

namespace ElementalWard.AI
{
    [CreateAssetMenu(fileName = "New AIDriverData", menuName = ElementalWardApplication.APP_NAME + "/AIDriverData")]
    public class AIDriverData : NebulaScriptableObject
    {
        [Header("Conditions")]
        [SerializeField] private SkillSlot _requiredSkillSlot;
        [SerializeField] private bool _requireSkillReady;
        [SerializeField] private float _minDistance;
        [SerializeField] private float _maxDistance = float.PositiveInfinity;
        [SerializeField] private bool _selectionRequiresLOS;
        [SerializeField] private bool _selectionRequiresGrounded;
        [SerializeField] private bool _selectionRequiresAimTarget;
        [SerializeField] private int _maxTimesSelected = -1;

        [Header("Behaviour")]
        [SerializeField] private TargetType _targetType;
        [SerializeField] private MovementType _movementType;
        [SerializeField] private AimType _aimType;
        [SerializeField] private ButtonPressType _buttonPressType;
        [SerializeField] private bool _useNavigationAgentForDirections;
        [SerializeField] private bool _shouldSprint;
        [SerializeField] private bool _activationRequiresTargetLOS;
        [SerializeField] private bool _activationRequiresAimTargetLOS;

        [Header("Transition Behaviour")]
        [SerializeField] private bool _noRepeat;

        public AIDriver CreateDriver()
        {
            return new AIDriver
            {
                DriverName = cachedName,
                ActivationRequiresTargetLOS = _activationRequiresTargetLOS,
                ActivationRequiresAimTargetLOS = _activationRequiresAimTargetLOS,
                AimType = _aimType,
                UseNodeGraph = _useNavigationAgentForDirections,
                ButtonPressType = _buttonPressType,
                MaxDistance = _maxDistance,
                MinDistance = _minDistance,
                MaxTimesSelected = _maxTimesSelected,
                MovementType = _movementType,
                RequiredSkillSlot = _requiredSkillSlot,
                RequireSkillReady = _requireSkillReady,
                SelectionRequiresGrounded = _selectionRequiresGrounded,
                SelectionRequiresLOS = _selectionRequiresLOS,
                SelectionRequiresAimTarget = _selectionRequiresAimTarget,
                ShouldSprint = _shouldSprint,
                TargetType = _targetType,
                IsAvailable = true
            };
        }
        public enum TargetType
        {
            CurrentEnemy = 0,
            NearestFriendly = 1,
        }

        public enum AimType
        {
            None = 0,
            AtMoveTarget = 1,
            AtCurrentEnemy = 2,
            MoveDirection = 3,
        }

        public enum MovementType
        {
            Stop = 0,
            ChaseEnemy = 1,
        }

        public enum ButtonPressType
        {
            Custom = -1,
            Hold = 0,
            Abstain = 1,
            TapContinuous = 2,
        }
    }

    public class AIDriver
    {
        public string DriverName { get; init; }
        public SkillSlot RequiredSkillSlot { get; init; }
        public bool RequireSkillReady { get; init; }
        public float MinDistance { get; init; }
        public float MaxDistance { get; init; }
        public bool SelectionRequiresLOS { get; init; }
        public bool SelectionRequiresGrounded { get; init; }
        public bool SelectionRequiresAimTarget { get; init; }
        public int MaxTimesSelected { get; init; }
        public TargetType TargetType { get; init; }
        public MovementType MovementType { get; init; }
        public AimType AimType { get; init; }
        public ButtonPressType ButtonPressType { get; init; }
        public bool UseNodeGraph { get; init; }
        public bool ShouldSprint { get; init; }
        public bool ActivationRequiresTargetLOS { get; init; }
        public bool ActivationRequiresAimTargetLOS { get; init; }
        public bool NoRepeat { get; init; }
        public bool IsAvailable { get; set; }

        public float MinDistanceSqr => MinDistance * MinDistance;
        public float MaxDistanceSqr => MaxDistance * MaxDistance;
        public int TimesSelected { get; private set; }

        public void OnSelected()
        {
            TimesSelected++;
        }
    }
}