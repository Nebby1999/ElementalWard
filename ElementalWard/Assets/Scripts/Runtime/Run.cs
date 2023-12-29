using Nebula;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ElementalWard
{
    public class Run : SingletonBehaviour<Run>
    {
        public GameObject characterPrefab;
        public AssetReferenceScene sampleScene;
        public ulong currentFloor;

        private SceneInstance instance;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            currentFloor = 0;
            StartCoroutine(C_BeginRun());
        }

        private IEnumerator C_BeginRun()
        {
            var op = sampleScene.LoadSceneAsync();
            while (op.IsDone)
                yield return null;

            instance = op.Result;
        }

        [ContextMenu("Force Completion")]
        public void StageComplete()
        {
            StartCoroutine(C_TransitionToNextFloor());
        }

        public void SpawnPlayers(Transform pos)
        {
            PlayableCharacterMaster player = null;
            if(!InstanceTracker.Any<PlayableCharacterMaster>())
            {
                Instantiate(characterPrefab);
                player = InstanceTracker.FirstOrDefault<PlayableCharacterMaster>();
                DontDestroyOnLoad(player.gameObject);
                player.ManagedMaster.OnBodyLost += CheckDeathSource;
            }
            player = InstanceTracker.FirstOrDefault<PlayableCharacterMaster>();
            player.ManagedMaster.Spawn(pos.position + Vector3.up, transform.rotation);
        }

        private void CheckDeathSource(DamageReport obj)
        {
            //No death source, its just the game loading the scene again, return.
            if (obj == null)
                return;

            if(obj.victimBody.characterMaster && obj.victimBody.characterMaster.PlayableCharacterMaster)
                //Death by enemy, back to menu.
                StartCoroutine(C_EndRun());
        }

        public IEnumerator C_EndRun()
        {
            yield return new WaitForSeconds(3);

            var op = Addressables.LoadSceneAsync("MainMenu.unity");
            while (!op.IsDone)
                yield return null;

            Destroy(gameObject);
        }

        private IEnumerator C_TransitionToNextFloor()
        {
            var op = Addressables.LoadSceneAsync("InbetweenScenesLoading.unity");
            while(!op.IsDone)
                yield return null;

            yield return C_BeginRun();
            currentFloor++;
        }
    }
}