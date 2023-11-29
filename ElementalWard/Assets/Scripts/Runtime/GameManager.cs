using Nebula;
using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] DungeonDirector _dungeonDirector;
        [SerializeField] private PlayableCharacterMaster _playableCharacterMaster;


        private void Start()
        {
            StartCoroutine(WaitForEverythingToBeSetUp());   
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