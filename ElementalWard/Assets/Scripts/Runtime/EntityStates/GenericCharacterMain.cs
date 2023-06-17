using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates
{
    public class GenericCharacterMain : BaseCharacterMain
    {
        private Vector3 moveVector;
        private Vector3 aimDirection;
        private bool wantsToJump;
        private bool wantsToSprint;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            GatherInputs();
            HandleMovement();
            ProcessInputs();
        }

        protected virtual void GatherInputs()
        {
            if(HasCharacterInputBank)
            {
                moveVector = CharacterInputBank.moveVector;
                aimDirection = CharacterInputBank.AimDirection;
                wantsToJump = CharacterInputBank.jumpButton.WasPressedThisFrame;
                wantsToSprint |= CharacterInputBank.sprintButton.IsPressed;
            }
        }
        protected virtual void HandleMovement()
        {
            if(HasCharacterMovementController)
            {
                CharacterMovementController.MovementDirection = CharacterInputBank.LookRotation * moveVector;
                CharacterMovementController.CharacterRotation = Quaternion.Euler(0, CharacterInputBank.LookRotation.eulerAngles.y, 0);
            }
            ProcessJump();
            if(HasCharacterBody)
            {
                bool shouldSprint = wantsToSprint;
                if (moveVector.magnitude <= 0.5f)
                    shouldSprint = false;

                CharacterBody.IsSprinting = shouldSprint;
            }
        }

        protected virtual void ProcessJump()
        {
            if(wantsToJump && HasCharacterMovementController)
            {
                CharacterMovementController.Jump();
            }

        }

        protected virtual void ProcessInputs()
        {
            wantsToJump = false;
            wantsToSprint = false;
        }
    }
}