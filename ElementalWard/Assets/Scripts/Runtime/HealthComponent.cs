using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ElementalWard
{
    public interface IOnIncomingDamage
    {
        public void OnIncomingDamage(DamageInfo damageInfo);
    }
    public interface IOnTakeDamage
    {
        public void OnTakeDamage(DamageReport dReport);
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

        private IOnIncomingDamage[] _incomingDamageRecievers;
        private IOnTakeDamage[] _takeDamageRecievers;

        private void Awake()
        {
            HealthProvider = GetComponent<IHealthProvider>();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            CurrentHealth -= damageInfo.damage;
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
