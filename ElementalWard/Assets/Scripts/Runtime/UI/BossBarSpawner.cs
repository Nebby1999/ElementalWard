using UnityEngine;
using UnityEngine.UI;

namespace ElementalWard.UI
{
	[RequireComponent(typeof(CharacterMaster))]
	public class BossBarSpawner : MonoBehaviour
	{
		public GameObject prefab;

        private CharacterMaster _master;
        private Slider _barInstance;
        private void Awake()
        {
            _master = GetComponent<CharacterMaster>();
        }

        public void SpawnBar()
        {
            if (_barInstance)
                return;

            _barInstance = Instantiate(prefab).GetComponentInChildren<Slider>();
        }

        private void LateUpdate()
        {
            if (!_barInstance)
                return;

            var body = _master.CurrentBody;
            if (!body)
                return;

            _barInstance.maxValue = body.MaxHealth;
            _barInstance.minValue = 0;
            _barInstance.value = body.HealthComponent.CurrentHealth;
        }
    }
}