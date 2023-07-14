using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UObject = UnityEngine.Object;
using Nebula;
namespace ElementalWard
{
    public class CharacterBody : MonoBehaviour, IHealthProvider, ILifeBehaviour
    {
        public LocalizedString bodyName;

        [SerializeField] private float _baseHealth;
        [SerializeField] private float _baseShield;
        [SerializeField] private float _baseRegen;
        [SerializeField] private float _baseMovementSpeed;
        [SerializeField] private float _baseAttackSpeed;
        [SerializeField] private float _baseDamage;
        [SerializeField] private float _baseArmor;
        [SerializeField] private float _jumpStrength;

        public bool autoCalculateLevelStats;

        [SerializeField] private float _lvlHealth;
        [SerializeField] private float _lvlShield;
        [SerializeField] private float _lvlRegen;
        [SerializeField] private float _lvlMovementSpeed;
        [SerializeField] private float _lvlAttackSpeed;
        [SerializeField] private float _lvlDamage;
        [SerializeField] private float _lvlArmor;
        [SerializeField] private Transform aimOriginTransform;
        [SerializeField] private float sprintSpeedMultiplier;

        public float MaxHealth { get; private set;}
        public float MaxShield { get; private set; }
        public float Regen { get; private set; }
        public float MovementSpeed { get; private set; }
        public float AttackSpeed { get; private set; }
        public float Damage { get; private set; }
        public float Armor { get; private set; }
        public float JumpStrength { get; private set; }
        public uint Level => TiedMaster.AsValidOrNull()?.Level ?? 1;
        public CharacterInputBank InputBank { get; private set; }
        public HealthComponent HealthComponent { get; private set; }
        public bool IsSprinting
        {
            get => _isSprinting;
            set
            {
                if (_isSprinting == value)
                    return;
                _isSprinting = value;
                RecalculateStats();
            }
        }
        public CharacterMaster TiedMaster { get; set; }
        public float Radius { get; internal set; }
        private bool _isSprinting;
        public Transform AimOriginTransform => aimOriginTransform.AsValidOrNull() ?? transform;
        private bool statsDirty;
        private void Awake()
        {
            InputBank = GetComponent<CharacterInputBank>();
            HealthComponent = GetComponent<HealthComponent>();

            var collider1 = GetComponent<CapsuleCollider>();
            Radius = collider1 ? collider1.radius : 1;
        }
        private void Start()
        {
            RecalculateStats();
            HealthComponent.HealthProvider = this;
            HealthComponent.CurrentHealth = MaxHealth;
        }

        public void RecalculateStats()
        {
            uint levelMinusOne = Level - 1;
            MaxHealth = _baseHealth + _lvlHealth * levelMinusOne;
            Regen = _baseRegen + _lvlRegen * levelMinusOne;
            MaxShield = _baseShield + _lvlShield * levelMinusOne;

            var movementSpeed = _baseMovementSpeed;
            var lvlMovementSpeed = _lvlMovementSpeed * levelMinusOne;
            var finalMovementSpeed = movementSpeed + lvlMovementSpeed;
            if (IsSprinting)
                finalMovementSpeed *= sprintSpeedMultiplier;
            MovementSpeed = finalMovementSpeed;

            AttackSpeed = _baseAttackSpeed + _lvlAttackSpeed * levelMinusOne;
            Damage = _baseDamage + _lvlDamage * levelMinusOne;
            Armor = _baseArmor + _lvlArmor * levelMinusOne;
            JumpStrength = _jumpStrength;
        }
        [ContextMenu("Recalculate Stats")]
        public void SetStatsDirty() => statsDirty = true;
		private void FixedUpdate()
		{
            if (statsDirty)
            {
                statsDirty = false;
                RecalculateStats();
            }
		}

		private void OnValidate()
        {
            if(autoCalculateLevelStats)
            {
                _lvlHealth = _baseHealth * 0.2f;
                _lvlRegen = _baseRegen * 0.1f;
                _lvlDamage = _baseDamage * 0.15f;
            }
        }

        public void OnDeathStart(DamageReport killingDamageInfo)
        {
            if(TiedMaster)
            {
                TiedMaster.BodyKilled(this);
            }
        }
    }
}
