using Nebula;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard.UI
{
    public class PauseMenu : MonoBehaviour
    {
        public CanvasGroup mainGroup;
        public CanvasGroup optionsGroup;

        private static GameObject _prefab;
        private static PauseMenu _instance;

        private void OnEnable()
        {
            if(!PauseManager.IsPaused)
            {
                PauseManager.PauseGame();
            }
            _instance = this;
        }
        private void OnDisable()
        {
            if(PauseManager.IsPaused)
            {
                PauseManager.UnpauseGame();
            }
            _instance = null;
        }

        public void Unpause()
        {
            PauseManager.UnpauseGame();
        }

        public void SwitchTo(CanvasGroup _group)
        {
            if (_group == mainGroup)
            {
                optionsGroup.interactable = false;
                optionsGroup.alpha = 0;
                optionsGroup.gameObject.SetActive(false);

                _group.interactable = true;
                _group.alpha = 1;
                _group.gameObject.SetActive(true);
            }
            else if(_group == optionsGroup)
            {
                mainGroup.interactable = false;
                mainGroup.alpha = 0;
                mainGroup.gameObject.SetActive(false);

                _group.interactable = true;
                _group.alpha = 1;
                _group.gameObject.SetActive(true);
            }
        }

        public void ReturnToMenu()
        {
            StartCoroutine(C_ReturnToMenu());
        }

        private IEnumerator C_ReturnToMenu()
        {
            mainGroup.interactable = false;

            var loadRequest = Addressables.LoadSceneAsync("MainMenu.unity");
            while (!loadRequest.IsDone)
                yield return null;
        }

        [SystemInitializer]
        private static void Initialize()
        {
            _prefab = Addressables.LoadAssetAsync<GameObject>("ElementalWard/Base/UI/Prefabs/PauseMenu.prefab").WaitForCompletion();

            PauseManager.OnPauseChange += SpawnOrDestroy;
        }

        private static void SpawnOrDestroy(bool shouldPause)
        {
            if (shouldPause)
            {
                if (!_instance)
                    _instance = Instantiate(_prefab).GetComponent<PauseMenu>();
            }
            else if (_instance)
                Destroy(_instance.gameObject);
        }
    }
}