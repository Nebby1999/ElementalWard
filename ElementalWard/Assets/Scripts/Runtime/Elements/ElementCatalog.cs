using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElementalWard;
using Nebula;
using Nebula.Serialization;
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
        public static int ElementCount => elementDefs.Length;
        private static ElementDef[] elementDefs = Array.Empty<ElementDef>();
        private static Dictionary<string, ElementIndex> elementNameToIndex = new(StringComparer.OrdinalIgnoreCase);

        private static IElementEvents[] elementEvents = Array.Empty<IElementEvents>();
        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(ElementCatalog));
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
#if DEBUG
            Debug.LogWarning($"Failed to find ElementIndex for ElementDef with name {elementDefName}");
#endif
            return ElementIndex.None;
        }

        public static IElementEvents GetElementEventsFor(ElementDef elementDef)
        {
            if (elementDef)
                return GetElementEventsFor(elementDef.ElementIndex);
            return null;
        }

        public static IElementEvents GetElementEventsFor(ElementIndex elementIndex)
        {
            return ArrayUtils.GetSafe(ref elementEvents, (int)elementIndex);
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
            elementEvents = new IElementEvents[results.Length];

            for(int i = 0; i < results.Length; i++)
            {
                ElementDef elementDef = results[i];
                ElementIndex elementIndex = (ElementIndex)i;
                elementDef.ElementIndex = elementIndex;
                elementNameToIndex[elementDef.name] = elementIndex;
                elementDefs[i] = elementDef;
                Type type = elementDef.elementEvents;
                if(type == null)
                {
                    Debug.LogWarning($"{elementDef} does not implement element events.", elementDef);
                    continue;
                }

                IElementEvents elementEvent = (IElementEvents)Activator.CreateInstance(type);
                elementEvent.TiedElement = elementDef;
                yield return elementEvent.LoadAssets();
                elementEvents[i] = elementEvent;
            }
            resourceAvailability.MakeAvailable(typeof(ElementCatalog));
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