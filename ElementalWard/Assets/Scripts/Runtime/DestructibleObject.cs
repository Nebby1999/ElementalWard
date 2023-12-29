using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(HealthComponent))]
    public class DestructibleObject : MonoBehaviour, IHealthProvider, IOnIncomingDamage
    {
        [SerializeField] private float _baseHealth;
        [SerializeField] private float _lvlHealth;
        [SerializeField] private ElementDef requiredElement;
        public float MaxHealth => _maxHealth;
        private float _maxHealth;
        public float HealthRegen => 0;

        private HealthComponent _healthComponent;
        private void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
            ulong level = 1;
            if (DungeonManager.Instance)
                level = DungeonManager.Instance.DungeonFloor;

            _maxHealth = _baseHealth + (_lvlHealth * level - 1);
            _healthComponent.HealthProvider = this;
            _healthComponent.CurrentHealth = MaxHealth;
        }

        public void OnIncomingDamage(DamageInfo info)
        {
            if (!requiredElement)
                return;

            if(!info.attackerBody)
            {
                return;
            }

            var element = info.attackerBody.ElementDef;
            if(element != requiredElement)
            {
                info.rejected = true;
            }
        }
    }
}
