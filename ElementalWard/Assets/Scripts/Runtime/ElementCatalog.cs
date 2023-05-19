using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public enum ElementIndex
    {
        None = -1
    }
    public static class ElementCatalog
    {
        public static int ElementDefLength => elementDefs.Length;
        private static ElementDef[] elementDefs = Array.Empty<ElementDef>();
        private static Dictionary<string, ElementIndex> elementNameToIndex = new Dictionary<string, ElementIndex>();

        public static ElementDef GetElementDef(ElementIndex index)
        {
            return ArrayUtils.GetSafe(ref elementDefs, (int)index);
        }

        public static ElementIndex FindElementIndex(string elementDefName)
        {
            if(elementNameToIndex.TryGetValue(elementDefName, out ElementIndex val))
            {
                return val;
            }
            return ElementIndex.None;
        }

        internal static IEnumerator Initialize()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<ElementDef>("ElementDefs", EnsureNaming);
            while(!handle.IsDone)
            {
                yield return new WaitForEndOfFrame();
            }
            var results = handle.Result.OrderBy(ed => ed.name).ToArray();

            elementDefs = new ElementDef[results.Length];
            for(int i = 0; i < results.Length; i++)
            {
                ElementDef elementDef = results[i];
                ElementIndex elementIndex = (ElementIndex)i;
                elementDef.ElementIndex = elementIndex;
                elementNameToIndex[elementDef.name] = elementIndex;
            }

            yield break;

            void EnsureNaming(ElementDef ed)
            {
                if(ed.name.IsNullOrWhiteSpace())
                {
                    ed.name = "ELEMENTDEF_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }
    }
}