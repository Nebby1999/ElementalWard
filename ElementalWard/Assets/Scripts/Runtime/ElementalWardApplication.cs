using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nebula;
using UObject = UnityEngine.Object;
using System.Threading.Tasks;

namespace ElementalWard
{
    public class ElementalWardApplication : MainGameBehaviour<ElementalWardApplication>
    {
        public const string APP_NAME = "ElementalWard";
        protected override void Awake()
        {
            base.Awake();
#if !UNITY_EDITOR
            Cursor.lockState = CursorLockMode.Locked;
#endif
        }
        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
#if !UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
#endif
        }
        protected override IEnumerator LoadGameContent()
        {
            return InitializeCatalogs();
        }

        private IEnumerator InitializeCatalogs()
        {
            yield return EntityStateCatalog.Initialize();
            yield return BuffCatalog.Initialize();
            yield return ElementCatalog.Initialize();
        }
    }
}
