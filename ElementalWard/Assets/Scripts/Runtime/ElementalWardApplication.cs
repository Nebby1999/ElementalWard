using KinematicCharacterController;
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
            yield return InitializeCatalogs();
            SystemInitializerAttribute.Execute();
            yield break;
        }

        private IEnumerator InitializeCatalogs()
        {
            yield return MasterCatalog.Initialize();
            yield return BodyCatalog.Initialize();
            yield return EntityStateCatalog.Initialize();
            yield return TeamCatalog.Initialize();
            yield return ProcCatalog.Initialize();
            yield return BuffCatalog.Initialize();
            yield return ElementCatalog.Initialize();
        }
    }
}
