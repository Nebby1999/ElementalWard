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
        protected override IEnumerator LoadGameContent()
        {
            return InitializeCatalogs();
        }

        private IEnumerator InitializeCatalogs()
        {
            return ElementCatalog.Initialize();
        }
    }
}
