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
    public enum BodyIndex
    {
        None = -1,
    }

    public static class BodyCatalog
    {
        public const string ADDRESSABLE_LABEL = "CharacterBodies";
        public static int BodyCount => _bodyPrefabs.Length;
        private static GameObject[] _bodyPrefabs = Array.Empty<GameObject>();
        private static Dictionary<string, BodyIndex> _bodyNameToIndex = new(StringComparer.OrdinalIgnoreCase);
        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(BodyCatalog));

        public static GameObject GetBodyPrefab(BodyIndex index)
        {
            return ArrayUtils.GetSafe(ref _bodyPrefabs, (int)index);
        }

        public static BodyIndex FindBodyIndex(string bodyPrefabName)
        {
            if (_bodyNameToIndex.TryGetValue(bodyPrefabName, out BodyIndex value))
            {
                return value;
            }
#if DEBUG
            Debug.LogWarning($"Failed to find BodyIndex for BodyPrefab with name {bodyPrefabName}");
#endif
            return BodyIndex.None;
        }

        internal static IEnumerator Initialize()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<GameObject>(ADDRESSABLE_LABEL, EnsureNaming);
            while (!handle.IsDone)
                yield return new WaitForEndOfFrame();

            var results = handle.Result.OrderBy(p => p.name).ToArray();

            _bodyPrefabs = new GameObject[results.Length];

            for (int i = 0; i < results.Length; i++)
            {
                GameObject prefab = results[i];
                BodyIndex index = (BodyIndex)i;
                prefab.GetComponent<CharacterBody>().BodyIndex = index;
                _bodyNameToIndex[prefab.name] = index;
                _bodyPrefabs[i] = prefab;
            }
            resourceAvailability.MakeAvailable(typeof(BodyCatalog));
            yield break;
            void EnsureNaming(GameObject obj)
            {
                if (obj.name.IsNullOrWhiteSpace())
                {
                    obj.name = "BODYPREFAB_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }


        [ConsoleCommand("list_bodies", "Lists all the bodies available.")]
        private static void CCListBodies(ConsoleCommandArgs args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (bodyName, bodyIndex) in _bodyNameToIndex)
            {
                sb.AppendLine($"{bodyName} ({bodyIndex})");
            }
            Debug.Log(sb.ToString());
        }
    }
}