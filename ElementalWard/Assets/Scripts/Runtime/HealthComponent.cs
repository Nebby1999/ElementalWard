using Nebula;
using System;
using UnityEngine;

namespace ElementalWard
{
    public interface IOnIncomingDamage
    {
        public void OnIncomingDamage(DamageInfo info);
    }
    public interface IOnTakeDamage
    {
        public void OnTakeDamage(DamageReport report);
    }
    public interface IHealthProvider
    {
        public float MaxHealth { get; }
        public float MaxShield { get; }
    }
    public class HealthComponent : MonoBehaviour
    {
        public bool IsAlive => CurrentHealth > 0;
        public float CurrentHealth { get; internal set; }
        public IHealthProvider HealthProvider
        {
            get => _healthProvider;
            set
            {
                if (_healthProvider != value)
                {
                    _healthProvider = value;
                    if (CurrentHealth > _healthProvider.MaxHealth)
                    {
                        CurrentHealth = _healthProvider.MaxHealth;
                    }
                }
            }
        }
        private IHealthProvider _healthProvider;
        public ElementDef CurrentElement => _elementProvider?.ElementDef;
        [Tooltip("If the game object that has this health component doesnt have a component that implements IHealthProvider, use this value for health.")]
        [SerializeField] private float _defaultMaxHealth = 100;

        public HurtBoxGroup hurtBoxGroup;
        public event Action<HealthComponent> OnDeath;

        private DamageReport _lastDamageSource;
        private CharacterDeathBehaviour _deathBehaviour;
        private IElementProvider _elementProvider;
        private TeamComponent _teamComponent;
        private IOnTakeDamage[] _takeDamageReceivers = Array.Empty<IOnTakeDamage>();
        private IOnIncomingDamage[] _incomingDamageReceivers = Array.Empty<IOnIncomingDamage>();
        private bool _wasAlive;

        internal bool IsImmune { get; set; }
        private void Awake()
        {
            _elementProvider = GetComponent<IElementProvider>();

            _takeDamageReceivers = GetComponents<IOnTakeDamage>();
            _incomingDamageReceivers = GetComponents<IOnIncomingDamage>();
            _deathBehaviour = GetComponent<CharacterDeathBehaviour>();
            _teamComponent = GetComponent<TeamComponent>();
        }

        private void Start()
        {
            if (_healthProvider == null && CurrentHealth == 0)
                CurrentHealth = _defaultMaxHealth;
            _wasAlive = true;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsImmune)
                return;

            if(!damageInfo.damageType.HasFlag(DamageType.FriendlyFire))
            {
                var selfTeamIndex = _teamComponent ? _teamComponent.CurrentTeamIndex : TeamIndex.None;
                if (damageInfo.attackerBody.team != TeamIndex.None || selfTeamIndex != TeamIndex.None)
                {
                    bool? isEnemy = TeamCatalog.GetTeamInteraction(damageInfo.attackerBody.team, selfTeamIndex);
                    if (isEnemy == false)
                    {
                        return;
                    }
                }
            }

            foreach (IOnIncomingDamage onIncomingDamage in _incomingDamageReceivers)
            {
                onIncomingDamage.OnIncomingDamage(damageInfo);
            }

            if (damageInfo.rejected)
                return;

#if DEBUG
            Debug.Log($"{this}: Taken {damageInfo.damage} damage.");
#endif
            CurrentHealth -= damageInfo.damage;
            DamageReport report = new DamageReport
            {
                damageType = damageInfo.damageType,
                attackerBody = damageInfo.attackerBody,
                victimBody = new BodyInfo(gameObject),
                damage = damageInfo.damage,
            };
            report.damageInfo = damageInfo;

            foreach (IOnTakeDamage onTakeDamage in _takeDamageReceivers)
            {
                onTakeDamage.OnTakeDamage(report);
            }
            if(damageInfo.attackerBody.TryGetComponents<IOnDamageDealt>(out var damageDealts))
            {
                foreach(IOnDamageDealt onDamageDealt in damageDealts)
                {
                    onDamageDealt?.OnDamageDealt(report);
                }
            }
            _lastDamageSource = report;

            ParticleTextSystem.SpawnParticle(transform.position, ParticleTextSystem.FormatDamage(damageInfo.damage, false), damageInfo.attackerBody.ElementDef?.elementColor ?? Color.white);
        }

        public void Heal(float amount)
        {
            CurrentHealth += amount;
            if (CurrentHealth > HealthProvider.MaxHealth)
                CurrentHealth = HealthProvider.MaxHealth;
        }

        private void FixedUpdate()
        {
            if (!IsAlive && _wasAlive)
            {
                _wasAlive = false;
                _deathBehaviour.AsValidOrNull()?.OnDeath(_lastDamageSource);
                OnDeath?.Invoke(this);
            }
        }
    }
}
