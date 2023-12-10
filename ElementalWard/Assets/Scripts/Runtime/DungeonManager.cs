using Nebula;
using System;
using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class DungeonManager : SingletonBehaviour<DungeonManager>
    {
        public ulong DungeonFloor => _dungeonFloor;
        public float DifficultyCoefficient
        {
            get
            {
                return 1 + (_dungeonFloor / 10);
            }
        }
        public DungeonDirector DungeonDirector => _dungeonDirector;
        public CombatDirector CombatDirector => _combatDirector;
        public ulong Seed { get; private set; }
        public Xoroshiro128Plus rng;
        [SerializeField] private ulong _dungeonFloor;
        [SerializeField] private DungeonDirector _dungeonDirector;
        [SerializeField] private CombatDirector _combatDirector;
        [SerializeField] private PlayableCharacterMaster _playableCharacterMaster;

        [SerializeField] private bool _useCustomSeed;
        [SerializeField] private ulong _customSeed;

        private void Awake()
        {
            Seed = _useCustomSeed ? _customSeed : ElementalWardApplication.rng.NextUlong;
            Debug.Log("Dungeon RNG Seed: " + Seed);
            rng = new Xoroshiro128Plus(Seed);
        }

        private void Start()
        {
            _combatDirector.enabled = false;
            StartCoroutine(WaitForEverythingToBeSetUp());
        }

        public GameObject TrySpawnObject(SpawnRequest spawnRequest)
        {
            PlacementRule placementRule = spawnRequest.placementRule;
            try
            {
                return placementRule.placement.Invoke(spawnRequest);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        protected override void DestroySelf()
        {
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#else
            Destroy(gameObject);
#endif
        }

        private IEnumerator WaitForEverythingToBeSetUp()
        {
            while(_dungeonDirector ? _dungeonDirector.GenerationComplete : false)
            {
                yield return null;
            }

            while(!SceneNavigationSystem.HasBakedGraphs)
                yield return null;

            if(_combatDirector)
            {
                _combatDirector.enabled = true;
                yield return null;
                _combatDirector.SpendAllCreditsOnMapSpawn();
                _combatDirector.enabled = false;
            }

            if(_playableCharacterMaster)
            {
                _playableCharacterMaster.ManagedMaster.Spawn(transform.position + Vector3.up, transform.rotation);
                _combatDirector.enabled = true;
            }
        }
    }
}