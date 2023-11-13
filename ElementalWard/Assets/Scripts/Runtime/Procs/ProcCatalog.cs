using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public enum ProcType
    {
        None = -1,
    }
    public static class ProcCatalog
    {
        public const string ADDRESSABLE_LABEL = "ProcAssets";
        public static int ProcCount => _procAssets.Length;
        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(ProcCatalog));
        private static ProcAsset[] _procAssets;
        private static Dictionary<string, ProcType> _procNameToIndex = new Dictionary<string, ProcType>(StringComparer.OrdinalIgnoreCase);

        public static ProcAsset GetProcAsset(ProcType type)
        {
            return ArrayUtils.GetSafe(ref _procAssets, (int)type);
        }

        public static ProcType FindProcType(string procName)
        {
            if (_procNameToIndex.TryGetValue(procName, out ProcType procType))
                return procType;

#if DEBUG
            Debug.LogWarning($"Failed to find ProcType for ProcAsset with name {procName}");
#endif

            return ProcType.None;
        }

        internal static IEnumerator Initialize()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<ProcAsset>(ADDRESSABLE_LABEL, EnsureNaming);
            while (!handle.IsDone)
                yield return new WaitForEndOfFrame();

            var results = handle.Result.OrderBy(td => td.cachedName).ToArray();

            _procAssets = new ProcAsset[results.Length];

            for(int i = 0; i < results.Length; i++)
            {
                ProcAsset asset = results[i];
                ProcType procType = (ProcType)i;
                asset.ProcType = procType;
                _procNameToIndex[asset.cachedName] = procType;
                _procAssets[i] = asset;
            }

            resourceAvailability.MakeAvailable(typeof(ProcCatalog));
            yield break;

            void EnsureNaming(ProcAsset def)
            {
                if (def.cachedName.IsNullOrWhiteSpace())
                {
                    def.cachedName = $"PROCASSET" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }
    }
}
