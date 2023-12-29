using Nebula;
using System;
using UnityEngine;
using UnityEngine.Localization;
namespace ElementalWard
{
    public class CharacterBody : MonoBehaviour, IHealthProvider, IManaProvider, ILifeBehaviour, IOnTakeDamage, IInventoryProvider
    {
        public LocalizedString bodyName;

        [SerializeField] private float _baseHealth;
        [SerializeField] private float _baseHealthRegen;
        [SerializeField] private float _baseMana;
        [SerializeField] private float _baseManaRegen;
        [SerializeField] private float _baseMovementSpeed;
        [SerializeField] private float _baseAttackSpeed;
        [SerializeField] private float _baseDamage;
        [SerializeField] private float _jumpStrength;

        public bool autoCalculateLevelStats;

        [SerializeField] private float _lvlHealth;
        [SerializeField] private float _lvlHealthRegen;
        [SerializeField] private float _lvlMana;
        [SerializeField] private float _lvlManaRegen;
        [SerializeField] private float _lvlMovementSpeed;
        [SerializeField] private float _lvlAttackSpeed;
        [SerializeField] private float _lvlDamage;
        [SerializeField] private Transform aimOriginTransform;
        [SerializeField] private float sprintSpeedMultiplier;

        public float SprintSpeedMultiplier => sprintSpeedMultiplier;
        public float MaxHealth { get; private set; }
        public float HealthRegen { get; private set; }
        public float MaxMana { get; private set; }
        public float ManaRegen { get; private set; }
        public float MovementSpeed { get; private set; }
        public float AttackSpeed { get; private set; }
        public float Damage { get; private set; }
        public float JumpStrength { get; private set; }
        public uint Level => TiedMaster.AsValidOrNull()?.Level ?? 1;
        public CharacterInputBank InputBank { get; private set; }
        public HealthComponent HealthComponent { get; private set; }
        public ManaComponent ManaComponent { get; private set; }
        public Inventory Inventory => TiedMaster ? TiedMaster.Inventory : null;
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
        public bool IsAIControlled
        {
            get
            {
                if (!TiedMaster)
                    return false;

                return !TiedMaster.PlayableCharacterMaster && TiedMaster.CharacterMasterAI;
            }
        }
        public CharacterMaster TiedMaster { get; set; }
        public float Radius { get; internal set; }
        public BodyIndex BodyIndex { get; internal set; }
        public TeamComponent TeamComponent { get; private set; }
        public Xoroshiro128Plus CharacterRNG { get; private set; }
        public Transform AimOriginTransform => aimOriginTransform.AsValidOrNull() ?? transform;
        private bool _isSprinting;
        private bool _statsDirty;
        private IBodyStatModifier[] _bodyStatModifiers = Array.Empty<IBodyStatModifier>();
        private BuffController _buffController;
        private void Awake()
        {
            InputBank = GetComponent<CharacterInputBank>();
            HealthComponent = GetComponent<HealthComponent>();
            ManaComponent = GetComponent<ManaComponent>();
            TeamComponent = GetComponent<TeamComponent>();
            _buffController = GetComponent<BuffController>();

            var collider1 = GetComponent<CapsuleCollider>();
            Radius = collider1 ? collider1.radius : 1;
            CharacterRNG = new Xoroshiro128Plus(DungeonManager.Instance ? DungeonManager.Instance.rng.NextUlong : ElementalWardApplication.rng.NextUlong);

            _bodyStatModifiers = GetComponents<IBodyStatModifier>();
        }
        private void Start()
        {
            RecalculateStats();
            HealthComponent.HealthProvider = this;
            HealthComponent.CurrentHealth = MaxHealth;
            ManaComponent.ManaProvider = this;
            ManaComponent.CurrentMana = MaxMana;
        }

        public void RecalculateStats()
        {
            var args = new StatModifierArgs();
            for(int i = 0; i < _bodyStatModifiers.Length; i++)
            {
                var modifier = _bodyStatModifiers[i];
                modifier.PreStatRecalculation(this);
                modifier.GetStatCoefficients(args, this);
            }
            uint levelMinusOne = Level - 1;

            int waterloggedCount = _buffController.AsValidOrNull()?.GetBuffCount(StaticElementReferences.Waterlogged) ?? 0;

            float baseStat = _baseHealth + args.baseHealthAdd;
            float levelStat = _lvlHealth * levelMinusOne;
            float finalStat = (baseStat + levelStat) * args.healthMultAdd;
            MaxHealth = finalStat;

            baseStat = _baseHealthRegen + args.baseHealthRegenAdd;
            levelStat = _lvlHealthRegen * levelMinusOne;
            finalStat = (baseStat + levelStat) * args.healthRegenMultAdd;
            HealthRegen = finalStat;

            baseStat = _baseMana + args.baseManaAdd;
            levelStat = _lvlMana * levelMinusOne;
            finalStat = (baseStat + levelStat) * args.manaMultAdd;
            MaxMana = finalStat;

            baseStat = _baseManaRegen + args.baseManaRegenAdd;
            levelStat = _lvlManaRegen * levelMinusOne;
            finalStat = (baseStat + levelStat) * args.manaRegenMultAdd;
            ManaRegen = finalStat;

            baseStat = _baseMovementSpeed + args.baseMovementSpeedAdd;
            levelStat = _lvlMovementSpeed * levelMinusOne;
            finalStat = (baseStat + levelStat) * args.movementSpeedMultAdd;
            if (IsSprinting)
                finalStat *= sprintSpeedMultiplier;

            var movementSpeedReduction = 1f;
            if(waterloggedCount > 0)
            {
                movementSpeedReduction *= NebulaMath.InverseHyperbolicScaling(9f, 0.1f, 1f, waterloggedCount) / 10;
            }
            finalStat *= movementSpeedReduction;
            MovementSpeed = finalStat;

            baseStat = _baseAttackSpeed + args.baseAttackSpeedAdd;
            levelStat = _lvlAttackSpeed * levelMinusOne;
            finalStat = (baseStat + levelStat) * args.attackSpeedMultAdd;
            var attackSpeedReduction = 1f;
            if(waterloggedCount > 0)
            {
                attackSpeedReduction *= NebulaMath.InverseHyperbolicScaling(5f, 0.1f, 5f, waterloggedCount) / 10;
            }
            finalStat *= attackSpeedReduction;
            AttackSpeed = finalStat;

            baseStat = _baseDamage + args.baseDamageAdd;
            levelStat = _lvlDamage * levelMinusOne;
            finalStat = (baseStat + levelStat) * args.damageMultAdd;
            Damage = finalStat;

            baseStat = _jumpStrength;
            finalStat = baseStat + args.baseJumpStrengthAdd;
            JumpStrength = finalStat;

            for (int i = 0; i < _bodyStatModifiers.Length; i++)
            {
                var modifier = _bodyStatModifiers[i];
                modifier.PostStatRecalculation(this);
            }
        }
        [ContextMenu("Recalculate Stats")]
        public void SetStatsDirty() => _statsDirty = true;
        private void FixedUpdate()
        {
            if (_statsDirty)
            {
                _statsDirty = false;
                RecalculateStats();
            }
        }

        private void OnValidate()
        {
            if (autoCalculateLevelStats)
            {
                _lvlHealth = _baseHealth * 0.2f;
                _lvlHealthRegen = _baseHealthRegen * 0.1f;
                _lvlMana = _baseMana * 0.1f;
                _lvlManaRegen = _baseManaRegen * 0.05f;
                _lvlDamage = _baseDamage * 0.15f;
            }
        }

        public void OnDeathStart(DamageReport killingDamageInfo)
        {
            if (TiedMaster)
            {
                TiedMaster.BodyKilled(this, killingDamageInfo);
            }
        }

        public void OnTakeDamage(DamageReport report)
        {
            if (!TiedMaster)
                return;

            if (!IsAIControlled)
                return;

            TiedMaster.CharacterMasterAI.SetTargetFromDamageReport(report);
        }

        private void OnDestroy()
        {
            TiedMaster.BodyKilled(this, null);
        }
    }

    public interface IBodyStatModifier
    {
        public void PreStatRecalculation(CharacterBody body);

        public void PostStatRecalculation(CharacterBody body);

        public void GetStatCoefficients(StatModifierArgs args, CharacterBody body);
    }

    public class StatModifierArgs
    {
        public float baseHealthAdd = 0;
        public float healthMultAdd = 1;

        public float baseHealthRegenAdd = 0;
        public float healthRegenMultAdd = 1;

        public float baseManaAdd = 0;
        public float manaMultAdd = 1;

        public float baseManaRegenAdd = 0;
        public float manaRegenMultAdd = 1;

        public float baseMovementSpeedAdd = 0;
        public float movementSpeedMultAdd = 1;

        public float baseAttackSpeedAdd = 0;
        public float attackSpeedMultAdd = 1;

        public float baseDamageAdd = 0;
        public float damageMultAdd = 1;

        public float baseArmorAdd = 0;

        public float baseJumpStrengthAdd = 0;
    }
}
