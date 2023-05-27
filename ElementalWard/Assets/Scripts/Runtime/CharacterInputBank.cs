using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalWard
{
    public class CharacterInputBank : MonoBehaviour
    {
        public struct ButtonState
        {
            public bool Performed => Phase == InputActionPhase.Performed;
            public bool Waiting => Phase == InputActionPhase.Waiting;
            public bool Started => Phase == InputActionPhase.Started;
            public bool Disabled => Phase == InputActionPhase.Disabled;
            public bool Canceled => Phase == InputActionPhase.Canceled;
            public InputActionPhase Phase { get; private set; }

            public void SetPhase(InputActionPhase newPhase) => Phase = newPhase;
        }
        public Vector3 moveVector;
        public ButtonState fire;
        public ButtonState modifier;
        public ButtonState jump;
        public ButtonState dash;

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
        public float yRotation;

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
