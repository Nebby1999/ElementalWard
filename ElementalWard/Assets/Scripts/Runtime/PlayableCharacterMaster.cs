using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.InputSystem;

namespace ElementalWard
{
    [RequireComponent(typeof(CharacterMaster))]
    public class PlayableCharacterMaster : MonoBehaviour, ElementalWardInput.IPlayerActions
    {
        public CharacterMaster ManagedMaster { get; private set; }
        public ElementalWardInput Input { get; private set; }

        private void Awake()
        {
            ManagedMaster = GetComponent<CharacterMaster>();
            Input = new ElementalWardInput();
        }

        private void OnEnable()
        {
            if(Input != null)
            {
                Input.Player.Enable();
                Input.Player.SetCallbacks(this);
            }
        }

        private void OnDisable()
        {
            if(Input != null)
            {
                Input.Player.Disable();
            }
        }

        public void OnTrackMouse(InputAction.CallbackContext context)
        {
            //ManagedMaster.NextDesiredPosition = context.ReadValue<Vector2>();
        }

        public void OnPathfindToMouse(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                //ManagedMaster.UpdatePosition();
            }
        }
    }
}
