using UnityEngine;

namespace ElementalWard
{
    public interface IManaProvider
    {
        public float MaxMana { get; }
        public float ManaRegen { get; }
    }
    [RequireComponent(typeof(HealthComponent))]
    public class ManaComponent : MonoBehaviour
    {
        public bool HasMana => CurrentMana > 0;
        public float CurrentMana { get; internal set; }
        public float FullMana => ManaProvider?.MaxMana ?? _defaultMaxMana;
        public IManaProvider ManaProvider
        {
            get => _manaProvider;
            set
            {
                if (_manaProvider != value)
                {
                    _manaProvider = value;
                    if (CurrentMana > _manaProvider.MaxMana)
                    {
                        CurrentMana = _manaProvider.MaxMana;
                    }
                }
            }
        }
        private IManaProvider _manaProvider;
        [SerializeField] private float _defaultMaxMana = 50;
        public HealthComponent HealthComponent { get; private set; }

        private void Awake()
        {
            HealthComponent = GetComponent<HealthComponent>();
        }
        private void Start()
        {
            if (_manaProvider == null && CurrentMana == 0)
                CurrentMana = _defaultMaxMana;
        }

        public void ConsumeMana(float amount)
        {
            CurrentMana = Mathf.Max(CurrentMana - amount, 0);
        }

        public void RestoreMana(float mana)
        {
            CurrentMana = Mathf.Min(CurrentMana + mana, FullMana);
        }

        private void FixedUpdate()
        {
            if (!HealthComponent.IsAlive || ManaProvider == null)
                return;

            CurrentMana = Mathf.Min(CurrentMana + (ManaProvider.ManaRegen * Time.fixedDeltaTime), FullMana);
        }
    }
}