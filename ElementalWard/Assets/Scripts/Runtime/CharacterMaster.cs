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

        [SerializeField] private bool _spawnOnStart;
        [SerializeField] private GameObject _defaultBodyPrefab;
        public uint Level { get => _level; set => _level = value; }
        [SerializeField] private uint _level;
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
        public GameObject CurrentCharacterPrefab { get => _currentCharacterPrefab; }
        private GameObject _currentCharacterPrefab;
        public CharacterBody CurrentBody { get; private set; }
        public Action<CharacterBody> OnBodySpawned;
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
            _currentCharacterPrefab = _defaultBodyPrefab;
            if (_defaultBodyPrefab && _spawnOnStart)
            {
                SpawnHere();
            }
        }
        public void SpawnHere() => Spawn(transform.position, Quaternion.identity);
        public void Spawn(Vector3 position, Quaternion rotation)
        {
            if(CurrentBody)
            {
                Destroy(CurrentBody);
            }
            var go = Instantiate(CurrentCharacterPrefab, position, rotation);
            CurrentBody = go.GetComponent<CharacterBody>();
            OnBodySpawned?.Invoke(CurrentBody);
        }
        public void SetCharacterPrefab(GameObject characterObject, bool forceRespawn = true)
        {
            if(characterObject.GetComponent<CharacterBody>())
            {
                _currentCharacterPrefab = characterObject;
                if(forceRespawn && CurrentBody)
                {
                    Vector3 pos = transform.position;
                    Quaternion rot = transform.rotation;
                    if(CurrentBody)
                    {
                        pos = CurrentBody.transform.position;
                        rot = CurrentBody.transform.rotation;
                    }
                    Spawn(pos, rot);
                }
            }
        }
        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
