using Nebula;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: SearchableAttribute.OptIn]
[assembly: InternalsVisibleTo("ElementalWard.Editor", AllInternalsVisible = true)]

namespace ElementalWard
{
    public class ElementalWardApplication : MainGameBehaviour<ElementalWardApplication>
    {
        public const string APP_NAME = "ElementalWard";

        public static Xoroshiro128Plus rng = new Xoroshiro128Plus((ulong)DateTime.Now.Ticks);
        public static readonly LoadGameAsyncArgs loadGameAsyncArgs = new LoadGameAsyncArgs();
        protected override void Awake()
        {
            base.Awake();
#if !UNITY_EDITOR
            Cursor.lockState = CursorLockMode.Locked;
#endif
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
#if !UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
#endif
        }

        protected override IEnumerator LoadGameContent()
        {
            yield return InitializeCatalogs(loadGameAsyncArgs);
            yield return SystemInitializerAttribute.ExecuteAsync(loadGameAsyncArgs);
            yield break;
        }

        private IEnumerator InitializeCatalogs(LoadGameAsyncArgs loadGameAsyncArgs)
        {
            yield return MasterCatalog.Initialize(loadGameAsyncArgs);
            yield return BodyCatalog.Initialize(loadGameAsyncArgs);
            yield return EntityStateCatalog.Initialize();
            yield return TeamCatalog.Initialize(loadGameAsyncArgs);
            yield return ProcCatalog.Initialize(loadGameAsyncArgs);
            yield return BuffCatalog.Initialize(loadGameAsyncArgs);
            yield return ElementCatalog.Initialize(loadGameAsyncArgs);
        }
    }
}
