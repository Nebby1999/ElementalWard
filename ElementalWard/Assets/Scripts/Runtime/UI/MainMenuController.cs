using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public class MainMenuController : MonoBehaviour
    {
        public CanvasGroup mainCanvasGroup;
        public CanvasGroup optionsCanvasGroup;
        public float fadeSpeed;

        private void Start()
        {
            if (mainCanvasGroup)
            {
                mainCanvasGroup.gameObject.SetActive(false);
                mainCanvasGroup.interactable = false;
                mainCanvasGroup.alpha = 0;
            }
            if(optionsCanvasGroup)
            {
                optionsCanvasGroup.gameObject.SetActive(false);
                optionsCanvasGroup.interactable = false;
                optionsCanvasGroup.alpha = 0;
            }

            StartCoroutine(C_FadeCanvasGroup(mainCanvasGroup, 1));
        }
        public void SwitchToCanvasGroup(CanvasGroup canvasGroup)
        {
            if(canvasGroup == mainCanvasGroup)
            {
                StartCoroutine(C_SwitchToCanvasGroup(optionsCanvasGroup, canvasGroup));
            }
            else if(canvasGroup == optionsCanvasGroup)
            {
                StartCoroutine(C_SwitchToCanvasGroup(mainCanvasGroup, canvasGroup));
            }
        }

        public void StartGame()
        {
            StartCoroutine(C_FadeOutAndStartGame());
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator C_SwitchToCanvasGroup(CanvasGroup from, CanvasGroup to)
        {
            from.interactable = false;
            to.interactable = false;

            yield return C_FadeCanvasGroup(from, 0);

            to.gameObject.SetActive(true);
            yield return C_FadeCanvasGroup(to, 1);
        }

        private IEnumerator C_FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha)
        {
            bool enabled = targetAlpha > 0;
            int num = canvasGroup.alpha > targetAlpha ? -1 : 1;
            while(canvasGroup.alpha != targetAlpha)
            {
                canvasGroup.alpha += fadeSpeed * num * Time.fixedDeltaTime;
                yield return null;
            }
            canvasGroup.gameObject.SetActive(enabled);
            canvasGroup.interactable = enabled;
            yield break;
        }

        private IEnumerator C_FadeOutAndStartGame()
        {
            yield return C_FadeCanvasGroup(mainCanvasGroup, 0);

            var op = Addressables.LoadAssetAsync<GameObject>("ElementalWard/Base/Core/Run.prefab");
            while (!op.IsDone)
                yield return null;

            Instantiate(op.Result);
        }
    }
}
