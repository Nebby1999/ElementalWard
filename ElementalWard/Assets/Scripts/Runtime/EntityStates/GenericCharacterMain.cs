using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates
{
    public class GenericCharacterMain : BaseCharacterState
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
            if(BodyInputBank)
            {
                moveVector = BodyInputBank.moveVector;
                aimDirection = BodyInputBank.AimDirection;
                wantsToJump = BodyInputBank.jumpButton.WasPressedThisFrame;
            }
        }
        protected virtual void HandleMovement()
        {
            if(MovementController)
            {
                MovementController.movementDirection = moveVector;
            }
            ProcessJump();
            if(Body)
            {
                bool shouldSprint = wantsToSprint;
                if (moveVector.magnitude <= 0.5f)
                    shouldSprint = false;

                Body.IsSprinting = shouldSprint;
            }
        }

        protected virtual void ProcessJump()
        {
            if(!wantsToJump)
            {
                return;
            }

        }

        protected virtual void ProcessInputs()
        {
            wantsToJump = false;
            wantsToSprint = false;
        }
    }
}