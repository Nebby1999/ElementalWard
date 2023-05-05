using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UObject = UnityEngine.Object;
namespace ElementalWard
{
    public class CharacterBody : MonoBehaviour
    {
        public LocalizedString bodyName;

        public float baseHealth;
        public float baseRegen;
        public float baseShield;
        public float baseMovementSpeed;
        public float baseAttackSpeed;
        public float baseDamage;
        public float baseArmor;

        public bool autoCalculateLevelStats;

        public float lvlHealth;
        public float lvlRegen;
        public float lvlShield;
        public float lvlMovementSpeed;
        public float lvlAttackSpeed;
        public float lvlDamage;
        public float lvlArmor;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void OnValidate()
        {
            if(autoCalculateLevelStats)
            {
                lvlHealth = baseHealth * 0.2f;
                lvlRegen = baseRegen * 0.1f;
                lvlDamage = baseDamage * 0.15f;
            }
        }
    }
}
