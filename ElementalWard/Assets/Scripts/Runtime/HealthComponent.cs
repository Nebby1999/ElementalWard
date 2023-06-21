using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

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
        public float CurrentHealth { get; internal set; }
        public IHealthProvider HealthProvider
        {
            get => _healthProvider;
            set
            {
                if(_healthProvider != value)
                {
                    _healthProvider = value;
                    if(CurrentHealth > _healthProvider.MaxHealth)
                    {
                        CurrentHealth = _healthProvider.MaxHealth;
                    }
                }
            }
        }
        private IHealthProvider _healthProvider;
        public ElementDef CurrentElement => _elementProvider?.Element;
        private IElementProvider _elementProvider;
        private IOnTakeDamage[] _takeDamageReceivers = Array.Empty<IOnTakeDamage>();
        private IOnIncomingDamage[] _incomingDamageReceivers = Array.Empty<IOnIncomingDamage>();
        private void Awake()
        {
            HealthProvider = GetComponent<IHealthProvider>();
            _elementProvider = GetComponent<IElementProvider>();

            _takeDamageReceivers = GetComponents<IOnTakeDamage>();
            _incomingDamageReceivers = GetComponents<IOnIncomingDamage>();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            var attackerElement = damageInfo.attackerBody.element;
            IElementEvents attackerElementEvents = ElementCatalog.GetElementEventsFor(attackerElement);
            IElementEvents selfElementEvents = ElementCatalog.GetElementEventsFor(CurrentElement);

            selfElementEvents?.OnIncomingDamage(damageInfo, this);
            foreach(IOnIncomingDamage onIncomingDamage in _incomingDamageReceivers)
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

            foreach(IOnTakeDamage onTakeDamage in _takeDamageReceivers)
            {
                onTakeDamage.OnTakeDamage(report);
            }
            selfElementEvents?.OnDamageTaken(report);
            attackerElementEvents?.OnDamageDealt(report);
        }

        private IElementEvents GetInteractionFromMatrix(DamageInfo damageInfo)
        {
            //Todo, make matrix stuff
            return null;
        }

        public void FixedUpdate()
        {
            if(CurrentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
