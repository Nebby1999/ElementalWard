using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ElementalWard
{
    public class CharacterMaster : MonoBehaviour
    {
        public const int LEVEL_TO_XP_COEF = 420;
        public const float LEVEL_TO_XP_DIVISOR = 7.69f;

        [SerializeField] private bool spawnOnStart;
        [SerializeField] private GameObject _defaultBodyPrefab;
        public uint Level { get => _level; set => _level = value; }
        [SerializeField] private uint _level;

        public Vector2 nextDesiredPosition;
        public float CurrentXP
        {
            get => _currentXP;
            set
            {
                var newCurrentXP = _currentXP + value;
                if (newCurrentXP > _neededXPForNextLevel)
                {
                    _currentXP = newCurrentXP - _neededXPForNextLevel;
                    LevelUp();
                    return;
                }
                _currentXP = newCurrentXP;
            }
        }
        [SerializeField] private float _currentXP;
        [SerializeField] private float _neededXPForNextLevel;
        public static event Action<CharacterMaster> OnLevelUpGlobal;

		private void OnValidate()
		{
            _currentXP = 0;
            _neededXPForNextLevel = Level * LEVEL_TO_XP_COEF / LEVEL_TO_XP_DIVISOR;
		}
        private void LevelUp()
        {
            OnLevelUpGlobal?.Invoke(this);
        }
		void Start()
        {
            if (_defaultBodyPrefab && spawnOnStart)
            {
                SpawnHere();
            }
        }
        public void SpawnHere() => Spawn(transform.position, Quaternion.identity);
        public void Spawn(Vector3 position, Quaternion rotation)
        {

        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
