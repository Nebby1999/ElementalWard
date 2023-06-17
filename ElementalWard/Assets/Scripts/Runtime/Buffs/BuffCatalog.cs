using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public enum BuffIndex
    {
        None = -1,
    }

    public enum DotIndex
    {
        None = -1,
    }

    public static class BuffCatalog
    {
        public static int DotCount => dotDefs.Length;
        public static int BuffCount => buffDefs.Length;
        private static BuffDef[] buffDefs = Array.Empty<BuffDef>();
        private static DotBuffDef[] dotDefs = Array.Empty<DotBuffDef>();
        private static Dictionary<string, BuffIndex> buffNametoBuffIndex = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, DotIndex> dotNameToDotIndex = new(StringComparer.OrdinalIgnoreCase);

        public static DotBuffDef GetDotDef(DotIndex dotIndex)
        {
            return ArrayUtils.GetSafe(ref dotDefs, (int)dotIndex);
        }

        public static BuffDef GetBuffDef(ElementIndex index)
        {
            return ArrayUtils.GetSafe(ref buffDefs, (int)index);
        }

        public static DotIndex FindDotIndex(string dotName)
        {
            if (dotNameToDotIndex.TryGetValue(dotName, out DotIndex index))
                return index;

            return DotIndex.None;
        }

        public static BuffIndex FindBuffIndex(string buffName)
        {
            if (buffNametoBuffIndex.TryGetValue(buffName, out BuffIndex index))
                return index;

            return BuffIndex.None;
        }

        internal static IEnumerator Initialize()
        {
            yield return InitBuffs();
            yield return InitDots();
            yield break;
        }

        private static IEnumerator InitBuffs()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<BuffDef>("BuffDefs", EnsureNaming);

            while (!handle.IsDone)
                yield return new WaitForEndOfFrame();

            var results = handle.Result.OrderBy(bd => bd.name).ToArray();

            buffDefs = new BuffDef[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                BuffDef buffDef = results[i];
                BuffIndex buffIndex = (BuffIndex)i;
                buffDef.BuffIndex = buffIndex;
                buffNametoBuffIndex[buffDef.name] = buffIndex;
            }

            yield break;

            void EnsureNaming(BuffDef bd)
            {
                if (bd.name.IsNullOrWhiteSpace())
                {
                    bd.name = "BUFFDEF_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }

        private static IEnumerator InitDots()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<DotBuffDef>("DotBuffDefs", EnsureNaming);

            while (!handle.IsDone)
                yield return new WaitForEndOfFrame();

            var results = handle.Result.OrderBy(dbd => dbd.name).ToArray();

            dotDefs = new DotBuffDef[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                DotBuffDef dotBuffDef = results[i];
                DotIndex dotIndex = (DotIndex)i;
                dotBuffDef.DotIndex = dotIndex;
                dotNameToDotIndex[dotBuffDef.name] = dotIndex;
            }

            yield break;

            void EnsureNaming(DotBuffDef bd)
            {
                if (bd.name.IsNullOrWhiteSpace())
                {
                    bd.name = "DOTBUFFDEF_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }
    }
}