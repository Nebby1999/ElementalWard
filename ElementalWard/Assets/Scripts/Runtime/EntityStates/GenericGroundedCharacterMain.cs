using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates
{
    public class GenericGroundedCharacterMain : BaseGroundedCharacterMain
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            GatherInputs();
            HandleMovement();
            ProcessInputs();
        }

        protected virtual void HandleMovement()
        {
            if(HasICharacterMovementController && HasCharacterInputBank)
            {
                var motor = ICharacterMovementController.Motor;
                ICharacterMovementController.MovementDirection = motor.CharacterForward * moveVector.z + motor.CharacterRight * moveVector.x;
                ICharacterMovementController.CharacterRotation = Quaternion.Euler(0, CharacterInputBank.LookRotation.eulerAngles.y, 0);
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
            if(wantsToJump && HasGroundedCharacterMovementController)
            {
                GroundedCharacterMovementController.Jump();
            }
        }
    }
}