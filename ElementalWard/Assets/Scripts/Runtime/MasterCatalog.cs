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
    public enum MasterIndex
    {
        None = -1,
    }

    public static class MasterCatalog
    {
        public const string ADDRESSABLE_LABEL = "CharacterMasters";
        public static int MasterCount => _masterPrefabs.Length;
        private static GameObject[] _masterPrefabs = Array.Empty<GameObject>();
        private static Dictionary<string, MasterIndex> _masterNameToIndex = new(StringComparer.OrdinalIgnoreCase);
        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(MasterCatalog));

        public static GameObject GetMasterPrefab(MasterIndex index)
        {
            return ArrayUtils.GetSafe(ref _masterPrefabs, (int)index);
        }

        public static MasterIndex FindMasterIndex(string masterPrefabName)
        {
            if(_masterNameToIndex.TryGetValue(masterPrefabName, out MasterIndex value))
            {
                return value;
            }
#if DEBUG
            Debug.LogWarning($"Failed to find MasterIndex for MasterPrefab with name {masterPrefabName}");
#endif
            return MasterIndex.None;
        }

        internal static IEnumerator Initialize()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<GameObject>(ADDRESSABLE_LABEL, EnsureNaming);
            while (!handle.IsDone)
                yield return new WaitForEndOfFrame();

            var results = handle.Result.OrderBy(p => p.name).ToArray();

            _masterPrefabs = new GameObject[results.Length];

            for(int i = 0; i < results.Length; i++)
            {
                GameObject prefab = results[i];
                MasterIndex index = (MasterIndex)i;
                prefab.GetComponent<CharacterMaster>().MasterIndex = index;
                _masterNameToIndex[prefab.name] = index;
                _masterPrefabs[i] = prefab;
            }
            resourceAvailability.MakeAvailable(typeof(MasterCatalog));
            yield break;
            void EnsureNaming(GameObject obj)
            {
                if(obj.name.IsNullOrWhiteSpace())
                {
                    obj.name = "MASTERPREFAB_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }

        [ConsoleCommand("list_masters", "Lists all the masters available.")]
        private static void CCListMasters(ConsoleCommandArgs args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (masterName, masterIndex) in _masterNameToIndex)
            {
                sb.AppendLine($"{masterName} ({masterIndex})");
            }
            Debug.Log(sb.ToString());
        }
    }
}