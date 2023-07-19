using Nebula;
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
        [SerializeField, ForcePrefab] private GameObject _defaultBodyPrefab;
        [SerializeField] private TeamDef _defaultTeam;
        [SerializeField] private uint _level;
        [SerializeField] private float _currentXP;
        [SerializeField] private float _neededXPForNextLevel;
        [SerializeField] private bool _autoCalculateNextLevelRequirement = true;
        public uint Level => _level;
        public float CurrentXP => _currentXP;
        public float NeededXPForNextLevel => _neededXPForNextLevel;
        public GameObject CurrentCharacterPrefab { get => _currentCharacterPrefab; }
        private GameObject _currentCharacterPrefab;
        public CharacterBody CurrentBody { get; private set; }
        public event Action<CharacterBody> OnBodySpawned;
        public event Action OnBodyLost;
        public static event Action<CharacterMaster> OnLevelUpGlobal;
        private void Start()
        {
            _currentCharacterPrefab = _defaultBodyPrefab;
            if (_defaultBodyPrefab && _spawnOnStart)
            {
                SpawnHere();
            }
        }
        private void FixedUpdate()
        {
            if (_currentXP > _neededXPForNextLevel)
            {
                _level++;
                _currentXP -= _neededXPForNextLevel;
                LevelUp();
            }
        }

        private void OnValidate()
        {
            if (_autoCalculateNextLevelRequirement)
            {
                _neededXPForNextLevel = CalculateExperienceForLevel(_level + 1);
            }
        }
        private void LevelUp()
        {
            _neededXPForNextLevel = CalculateExperienceForLevel(_level + 1);
            OnLevelUpGlobal?.Invoke(this);
        }
        public void SpawnHere() => Spawn(transform.position, Quaternion.identity);
        public void Spawn(Vector3 position, Quaternion rotation)
        {
            if (CurrentBody)
            {
                Destroy(CurrentBody);
            }
            var go = Instantiate(CurrentCharacterPrefab, position, rotation);
            CurrentBody = go.GetComponent<CharacterBody>();
            CurrentBody.TiedMaster = this;

            TeamComponent teamComponent = go.GetComponent<TeamComponent>();
            if(teamComponent)
                teamComponent.CurrentTeamIndex = _defaultTeam ? _defaultTeam.TeamIndex : TeamIndex.None;

            OnBodySpawned?.Invoke(CurrentBody);
        }
        public void SetCharacterPrefab(GameObject characterObject, bool forceRespawn = true)
        {
            if (characterObject.GetComponent<CharacterBody>())
            {
                _currentCharacterPrefab = characterObject;
                if (forceRespawn && CurrentBody)
                {
                    Vector3 pos = transform.position;
                    Quaternion rot = transform.rotation;
                    if (CurrentBody)
                    {
                        pos = CurrentBody.transform.position;
                        rot = CurrentBody.transform.rotation;
                    }
                    Spawn(pos, rot);
                }
            }
        }
        private void AddXP(uint xpAmount)
        {
            _currentXP += xpAmount;
        }

        private void RemoveXP(uint xpAmount)
        {
            _currentXP -= xpAmount;
        }
        private float CalculateExperienceForLevel(uint levelToCalculate)
        {
            return levelToCalculate * LEVEL_TO_XP_COEF / LEVEL_TO_XP_DIVISOR;
        }

        public void BodyKilled(CharacterBody body)
        {
            if(CurrentBody == body)
            {
                OnBodyLost?.Invoke();
            }
        }
    }
}
