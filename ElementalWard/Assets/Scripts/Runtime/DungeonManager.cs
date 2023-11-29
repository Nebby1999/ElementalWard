using Nebula;
using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class DungeonManager : SingletonBehaviour<DungeonManager>
    {
        public ulong DungeonFloor => _dungeonFloor;
        [SerializeField] private ulong _dungeonFloor;
        [SerializeField] private DungeonDirector _dungeonDirector;
        [SerializeField] private CombatDirector _combatDirector;
        [SerializeField] private PlayableCharacterMaster _playableCharacterMaster;


        private void Start()
        {
            StartCoroutine(WaitForEverythingToBeSetUp());   
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

            while(!SceneNavigationSystem.HasGraphs)
                yield return null;



            if(_playableCharacterMaster)
            {
                _playableCharacterMaster.ManagedMaster.SpawnHere();
            }
        }
    }
}