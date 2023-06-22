using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nebula
{
    [RequireComponent(typeof(Collider2D))]
    public class SceneRestarter : MonoBehaviour
    {
        public static bool canRestart = true;
        public TagObject requiredTag;
        private string sceneName;
        protected virtual void Awake()
        {
            sceneName = SceneManager.GetActiveScene().name;       
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            var go = collision.gameObject.GetRootGameObject();
            var tagContainer = go.GetComponent<TagContainer>();
            if(tagContainer)
            {
                if(tagContainer.ObjectHasTag(requiredTag) && canRestart)
                {
                    RestartScene();
                }
            }
        }

        private void OnValidate()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        protected virtual void RestartScene()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}