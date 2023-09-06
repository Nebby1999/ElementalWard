using UnityEngine;

namespace ElementalWard
{
    public class DestructibleObject : MonoBehaviour, IHealthProvider
    {
        [SerializeField] private float _baseHealth;
        [SerializeField] private float _lvlHealth;
        public uint level;
        public bool useAmbientLevel;
        public float MaxHealth => _maxHealth;
        private float _maxHealth;
        public float MaxShield => 0;

        private HealthComponent _healthComponent;
        private void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
            _maxHealth = _baseHealth + (_lvlHealth * level - 1);
            _healthComponent.HealthProvider = this;
            _healthComponent.CurrentHealth = MaxHealth;
        }
    }
}
