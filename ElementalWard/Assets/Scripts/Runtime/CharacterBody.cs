using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UObject = UnityEngine.Object;
using Nebula;
namespace ElementalWard
{
    public class CharacterBody : MonoBehaviour
    {
        public LocalizedString bodyName;

        [SerializeField] private float _baseHealth;
        [SerializeField] private float _baseRegen;
        [SerializeField] private float _baseShield;
        [SerializeField] private float _baseMovementSpeed;
        [SerializeField] private float _baseAttackSpeed;
        [SerializeField] private float _baseDamage;
        [SerializeField] private float _baseArmor;

        public bool autoCalculateLevelStats;

        [SerializeField] private float _lvlHealth;
        [SerializeField] private float _lvlRegen;
        [SerializeField] private float _lvlShield;
        [SerializeField] private float _lvlMovementSpeed;
        [SerializeField] private float _lvlAttackSpeed;
        [SerializeField] private float _lvlDamage;
        [SerializeField] private float _lvlArmor;

        public float MaxHealth { get; private set; }
        public float Regen { get; private set; }
        public float Shield { get; private set; }
        public float MovementSpeed { get; private set; }
        public float AttackSpeed { get; private set; }
        public float Damage { get; private set; }
        public float Armor { get; private set; }
        public uint Level => tiedMaster ? tiedMaster.Level : 1;
        private CharacterMaster tiedMaster;
        private bool statsDirty;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void RecalculateStats()
        {

        }
        public void SetStatsDirty() => statsDirty = true;
		private void FixedUpdate()
		{
            if (statsDirty)
            {
                statsDirty = false;
                RecalculateStats();
            }
		}

		private void OnValidate()
        {
            if(autoCalculateLevelStats)
            {
                _lvlHealth = _baseHealth * 0.2f;
                _lvlRegen = _baseRegen * 0.1f;
                _lvlDamage = _baseDamage * 0.15f;
            }
        }
    }
}
