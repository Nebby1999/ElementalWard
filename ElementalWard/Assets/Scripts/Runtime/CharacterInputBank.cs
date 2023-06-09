﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalWard
{
    /// <summary>
    /// Represents inputs for a character.
    /// </summary>
    public class CharacterInputBank : MonoBehaviour
    {
        public struct Button
        {
            public InputAction TiedAction
            {
                get => _tiedAction;
                set
                {
                    if (value.type != InputActionType.Button)
                        throw new InvalidOperationException("InputAction type for TiedAction needs to be Button!");
                    _tiedAction = value;
                }
            }
            private InputAction _tiedAction;

            /// <summary>
            /// <inheritdoc cref="InputAction.WasPerformedThisFrame"/>
            /// </summary>
            public bool WasPerformedThisFrame => _tiedAction?.WasPerformedThisFrame() ?? false;
            /// <summary>
            /// <inheritdoc cref="InputAction.WasReleasedThisFrame"/>
            /// </summary>
            public bool WasReleasedThisFrame => _tiedAction?.WasReleasedThisFrame() ?? false;
            /// <summary>
            /// <inheritdoc cref="InputAction.WasPressedThisFrame"/>
            /// </summary>
            public bool WasPressedThisFrame => _tiedAction?.WasPressedThisFrame() ?? false;
            /// <summary>
            /// <inheritdoc cref="InputAction.inProgress"/>
            /// </summary>
            public bool IsPressed => _tiedAction?.inProgress ?? false;
        }
        public Vector3 moveVector;
        public Button jumpButton;

        public Vector3 AimDirection
        {
            get
            {
                if(_aimDirection == Vector3.zero)
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

        private void Update()
        {

#if DEBUG
            Debug.DrawRay(AimOrigin, AimDirection * 5, Color.yellow, 0.01f);
#endif
        }
    }
}
