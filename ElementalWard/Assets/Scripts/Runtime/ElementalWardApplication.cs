using KinematicCharacterController;
using Nebula;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: SearchableAttribute.OptIn]
[assembly: InternalsVisibleTo("ElementalWard.Editor", AllInternalsVisible = true)]

namespace ElementalWard
{
    public class ElementalWardApplication : MainGameBehaviour<ElementalWardApplication>
    {
        public InputActionAsset inputActionAsset;
        public const string APP_NAME = "ElementalWard";

        public static Xoroshiro128Plus rng = new Xoroshiro128Plus((ulong)DateTime.Now.Ticks);
        protected override void Awake()
        {
            base.Awake();
#if !UNITY_EDITOR
            Cursor.lockState = CursorLockMode.Confined;
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


            var map = inputActionAsset.FindActionMap("Player");
            var overrides = SettingsCollection.PlayerInputOverrides;
            if (map != null && !overrides.IsNullOrWhiteSpace())
            {
                map.LoadBindingOverridesFromJson(overrides);
            }
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
            yield return PickupCatalog.Initialize();
        }
    }
}
