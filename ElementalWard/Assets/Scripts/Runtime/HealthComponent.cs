using Nebula;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ElementalWard
{
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
        private void Awake()
        {
            HealthProvider = GetComponent<IHealthProvider>();
            _elementProvider = GetComponent<IElementProvider>();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            var attackerElement = damageInfo.attackerBody.element;
            IElementEvents attackerElementEvents = ElementCatalog.GetElementEventsFor(attackerElement);
            IElementEvents selfElementEvents = ElementCatalog.GetElementEventsFor(CurrentElement);

            selfElementEvents?.OnIncomingDamage(damageInfo, this);

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
