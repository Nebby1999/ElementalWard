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
        public float distance;
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
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
