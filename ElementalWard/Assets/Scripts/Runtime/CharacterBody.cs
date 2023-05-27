using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UObject = UnityEngine.Object;
using Nebula;
namespace ElementalWard
{
    public class CharacterBody : MonoBehaviour, IHealthProvider
    {
        public LocalizedString bodyName;

        [SerializeField] private float _baseHealth;
        [SerializeField] private float _baseShield;
        [SerializeField] private float _baseRegen;
        [SerializeField] private float _baseMovementSpeed;
        [SerializeField] private float _baseAttackSpeed;
        [SerializeField] private float _baseDamage;
        [SerializeField] private float _baseArmor;

        public bool autoCalculateLevelStats;

        [SerializeField] private float _lvlHealth;
        [SerializeField] private float _lvlShield;
        [SerializeField] private float _lvlRegen;
        [SerializeField] private float _lvlMovementSpeed;
        [SerializeField] private float _lvlAttackSpeed;
        [SerializeField] private float _lvlDamage;
        [SerializeField] private float _lvlArmor;
        [SerializeField] private Transform aimOriginTransform;

        public float MaxHealth { get; private set;}
        public float MaxShield { get; private set; }
        public float Regen { get; private set; }
        public float MovementSpeed { get; private set; }
        public float AttackSpeed { get; private set; }
        public float Damage { get; private set; }
        public float Armor { get; private set; }
        public uint Level => tiedMaster ? tiedMaster.Level : 1;
        public CharacterInputBank InputBank { get; private set; }
        public HealthComponent HealthComponent { get; private set; }
        public Transform AimOriginTransform => aimOriginTransform.AsValidOrNull() ?? transform;
        private CharacterMaster tiedMaster;
        private bool statsDirty;
        private void Awake()
        {
            InputBank = GetComponent<CharacterInputBank>();
            HealthComponent = GetComponent<HealthComponent>();
        }
        private void Start()
        {
            RecalculateStats();
            HealthComponent.HealthProvider = this;
            HealthComponent.CurrentHealth = MaxHealth;
        }

        void Update()
        {
        }

        public void RecalculateStats()
        {
            uint levelMinusOne = Level - 1;
            MaxHealth = _baseHealth + _lvlHealth * levelMinusOne;
            Regen = _baseRegen + _lvlRegen * levelMinusOne;
            MaxShield = _baseShield + _lvlShield * levelMinusOne;
            MovementSpeed = _baseMovementSpeed + _lvlMovementSpeed * levelMinusOne;
            AttackSpeed = _baseAttackSpeed + _lvlAttackSpeed * levelMinusOne;
            Damage = _baseDamage + _lvlDamage * levelMinusOne;
            Armor = _baseArmor + _lvlArmor * levelMinusOne;
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
    }
}
