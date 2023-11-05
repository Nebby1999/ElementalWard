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
        private static Dictionary<string, BuffIndex> buffNametoBuffIndex = new(StringComparer.OrdinalIgnoreCase);
        private static DotBuffDef[] dotDefs = Array.Empty<DotBuffDef>();
        private static Dictionary<string, DotIndex> dotNameToDotIndex = new(StringComparer.OrdinalIgnoreCase);
        private static HashSet<BuffIndex> dotBuffIndices = new HashSet<BuffIndex>();
        private static Type[] dotBehaviours = Array.Empty<Type>();

        //Fucking ugly hack.
        private static DotBehaviour currentlyLoadingBehaviour;
        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(BuffCatalog));

        public static DotBehaviour InitializeDotBehaviour(DotBuffDef dotBuffDef) => dotBuffDef ? InitializeDotBehaviour(dotBuffDef.DotIndex) : null;

        public static DotBehaviour InitializeDotBehaviour(DotIndex dotIndex)
        {
            if (dotIndex == DotIndex.None)
                return null;

            DotBuffDef dotBuffDef = GetDotDef(dotIndex);
            var instance = (DotBehaviour)Activator.CreateInstance(dotBehaviours[(int)dotIndex]);
            instance.TiedDotDef = dotBuffDef;
            return instance;
        }
        public static bool IsBuffADot(BuffDef buffDef) => buffDef ? dotBuffIndices.Contains(buffDef.BuffIndex) : false;
        public static bool IsBuffADot(BuffIndex index) => dotBuffIndices.Contains(index);
        public static DotBuffDef GetDotDef(DotIndex dotIndex)
        {
            return ArrayUtils.GetSafe(ref dotDefs, (int)dotIndex);
        }

        public static BuffDef GetBuffDef(BuffIndex index)
        {
            return ArrayUtils.GetSafe(ref buffDefs, (int)index);
        }

        public static DotIndex FindDotIndex(string dotName)
        {
            if (dotNameToDotIndex.TryGetValue(dotName, out DotIndex index))
                return index;
#if DEBUG
            Debug.LogWarning($"Failed to find DotIndex for DotDef with name {dotName}");
#endif
            return DotIndex.None;
        }

        public static BuffIndex FindBuffIndex(string buffName)
        {
            if (buffNametoBuffIndex.TryGetValue(buffName, out BuffIndex index))
                return index;
#if DEBUG
            Debug.LogWarning($"Failed to find BuffIndex for BuffDef with name {buffName}");
#endif
            return BuffIndex.None;
        }

        internal static IEnumerator Initialize()
        {
            yield return InitBuffs();
            yield return InitDots();
            resourceAvailability.MakeAvailable(typeof(BuffCatalog));
            yield break;
        }

        private static IEnumerator InitBuffs()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<BuffDef>("BuffDefs", EnsureNaming);

            while (!handle.IsDone)
                yield return new WaitForEndOfFrame();

            var results = handle.Result.OrderBy(bd => bd.cachedName).ToArray();

            buffDefs = new BuffDef[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                BuffDef buffDef = results[i];
                buffDefs[i] = buffDef;
                BuffIndex buffIndex = (BuffIndex)i;
                buffDef.BuffIndex = buffIndex;
                buffNametoBuffIndex[buffDef.cachedName] = buffIndex;
            }

            yield break;

            void EnsureNaming(BuffDef bd)
            {
                if (bd.cachedName.IsNullOrWhiteSpace())
                {
                    bd.cachedName = "BUFFDEF_" + invalidNameTracker;
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

            var results = handle.Result.OrderBy(dbd => dbd.cachedName).ToArray();

            dotDefs = new DotBuffDef[results.Length];
            dotBehaviours = new Type[results.Length];

            for (int i = 0; i < results.Length; i++)
            {
                currentlyLoadingBehaviour = null;
                DotBuffDef dotBuffDef = results[i];
                DotIndex dotIndex = (DotIndex)i;
                dotBuffDef.DotIndex = dotIndex;
                dotDefs[i] = dotBuffDef;
                dotNameToDotIndex[dotBuffDef.cachedName] = dotIndex;
                dotBuffIndices.Add(dotBuffDef.BuffIndex);

                Type type = dotBuffDef.dotBehaviour;
                if (type == null)
                {
                    Debug.LogWarning($"{dotBuffDef} does not implement a dot behaviour.", dotBuffDef);
                    continue;
                }
                dotBehaviours[i] = type;
                var instance = (DotBehaviour)Activator.CreateInstance(type);
                currentlyLoadingBehaviour = instance;
                yield return instance.Initialize();
            }

            yield break;

            void EnsureNaming(DotBuffDef bd)
            {
                if (bd.cachedName.IsNullOrWhiteSpace())
                {
                    bd.cachedName = "DOTBUFFDEF_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }

        [ConsoleCommand("list_buffs", "Lists all the buffs available.")]
        private static void CCListBuffs(ConsoleCommandArgs args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (buffName, buffIndex) in buffNametoBuffIndex)
            {
                sb.AppendLine($"{buffName} ({buffIndex})");
            }
            Debug.Log(sb.ToString());
        }
    }
}