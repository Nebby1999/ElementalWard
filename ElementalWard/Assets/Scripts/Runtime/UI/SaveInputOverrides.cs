using Nebula.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalWard.UI
{
    [RequireComponent(typeof(InputRebinder))]
    public class SaveInputOverrides : MonoBehaviour
    {
        public InputRebinder Rebinder { get; private set; }

        private void Awake()
        {
            Rebinder = GetComponent<InputRebinder>();
        }
        public void Save()
        {
            if(!Rebinder)
            {
                return;
            }

            var action = Rebinder.ActionReference.action;
            if (action == null)
                return;

            string overrides = action.actionMap.SaveBindingOverridesAsJson();
            if (overrides != null)
            {
                SettingsCollection.PlayerInputOverrides = overrides;
            }
        }
    }
}