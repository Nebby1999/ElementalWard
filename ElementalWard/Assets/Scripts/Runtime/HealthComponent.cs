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
        public float CurrentHealth { get => _currentHealth; internal set => _currentHealth = value; }
        private float _currentHealth;
        public IHealthProvider HealthProvider
        {
            get => _healthProvider;
            set
            {
                if(_healthProvider != value)
                {
                    _healthProvider = value;
                    if(_currentHealth > _healthProvider.MaxHealth)
                    {
                        _currentHealth = _healthProvider.MaxHealth;
                    }
                }
            }
        }
        private IHealthProvider _healthProvider;

        private void Awake()
        {
            HealthProvider = GetComponent<IHealthProvider>();
        }

        public void FixedUpdate()
        {
            if(_currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
