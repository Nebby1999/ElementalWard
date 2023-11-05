using Nebula;
using Nebula.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public const string ADDRESSABLE_LABEL = "ElementDefs";
        public static int ElementCount => elementDefs.Length;
        private static ElementDef[] elementDefs = Array.Empty<ElementDef>();
        private static Dictionary<string, ElementIndex> elementNameToIndex = new(StringComparer.OrdinalIgnoreCase);

        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(ElementCatalog));
        public static ElementDef GetElementDef(ElementIndex index)
        {
            return ArrayUtils.GetSafe(ref elementDefs, (int)index);
        }

        public static ElementIndex FindElementIndex(string elementDefName)
        {
            if (elementNameToIndex.TryGetValue(elementDefName, out ElementIndex val))
            {
                return val;
            }
#if DEBUG
            Debug.LogWarning($"Failed to find ElementIndex for ElementDef with name {elementDefName}");
#endif
            return ElementIndex.None;
        }

        internal static IEnumerator Initialize()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<ElementDef>(ADDRESSABLE_LABEL, EnsureNaming);
            while (!handle.IsDone)
            {
                yield return new WaitForEndOfFrame();
            }
            var results = handle.Result.OrderBy(ed => ed.cachedName).ToArray();

            elementDefs = new ElementDef[results.Length];

            for (int i = 0; i < results.Length; i++)
            {
                ElementDef elementDef = results[i];
                ElementIndex elementIndex = (ElementIndex)i;
                elementDef.ElementIndex = elementIndex;
                elementNameToIndex[elementDef.cachedName] = elementIndex;
                elementDefs[i] = elementDef;
                Type type = elementDef.elementInteractions;
                if (type == null)
                {
                    Debug.LogWarning($"{elementDef} does not implement element interactions.", elementDef);
                    continue;
                }

                IElementInteraction elementInteraction = (IElementInteraction)Activator.CreateInstance(type);
                elementInteraction.SelfElement = elementDef;
                yield return elementInteraction.LoadAssetsAsync();
                elementDef.ElementalInteraction = elementInteraction;
            }
            resourceAvailability.MakeAvailable(typeof(ElementCatalog));
            yield break;

            void EnsureNaming(ElementDef ed)
            {
                if (ed.cachedName.IsNullOrWhiteSpace())
                {
                    ed.cachedName = "ELEMENTDEF_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }

        [ConsoleCommand("list_elements", "Lists all the elements available.")]
        private static void CCListElements(ConsoleCommandArgs args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (elementName, elementIndex) in elementNameToIndex)
            {
                sb.AppendLine($"{elementName} ({elementIndex})");
            }
            Debug.Log(sb.ToString());
        }
    }
}