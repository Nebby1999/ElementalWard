using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalWard
{
    /// <summary>
    /// Represents inputs for a character.
    /// </summary>
    public class CharacterInputBank : MonoBehaviour
    {
        public Vector3 moveVector;
        /// <summary>
        /// if this value is > 0, then go to the next element, otherwise, go back to the previous element
        /// </summary>
        public float elementAxis;
        public Button jumpButton;
        public Button sprintButton;
        public Button inventoryButton;
        public Button interactButton;
        public Button openMapButton;
        public Button primaryButton;
        public Button secondaryButton;
        public Button utilityButton;
        public Button specialButton;
        public Button weaponWheelButton;
        public Button weaponSlot1;
        public Button weaponSlot2;
        public Button weaponSlot3;
        public Button weaponSlot4;

        public Vector3 AimDirection
        {
            get
            {
                if (_aimDirection == Vector3.zero)
                {
                    return transform.forward;
                }
                return _aimDirection;
            }
            set
            {
                _aimDirection = value.normalized;
            }
        }
        private Vector3 _aimDirection;
        public Quaternion LookRotation { get; set; }
        public Vector3 AimOrigin => characterBody ? characterBody.AimOriginTransform.position : transform.position;
        private CharacterBody characterBody;
        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }
        private void Start()
        {
            AimDirection = transform.forward;
        }

#if DEBUG
        private void Update()
        {
            //Debug.DrawRay(AimOrigin, AimDirection * 5, Color.yellow, 0.01f);
            Debug.DrawRay(transform.position, moveVector * 10, Color.blue, 0.01f);
        }
#endif

        public struct Button
        {
            public bool down;
            public bool wasDown;
            public bool hasPressBeenClaimed;
            public bool JustReleased
            {
                get
                {
                    if (!down)
                        return wasDown;
                    return false;
                }
            }

            public bool JustPressed
            {
                get
                {
                    if (down)
                        return !down;
                    return false;
                }
            }

            public void PushState(bool newState)
            {
                hasPressBeenClaimed &= newState;
                wasDown = down;
                down = newState;
            }
        }
    }
}
