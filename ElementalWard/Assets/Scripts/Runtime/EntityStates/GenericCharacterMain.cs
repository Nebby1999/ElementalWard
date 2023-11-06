using UnityEngine;

namespace EntityStates
{
    public class GenericCharacterMain :  BaseCharacterMain
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
            if(HasCharacterController)
            {
                CharacterController.MovementDirection = moveVector;
                CharacterController.CharacterRotation = HasCharacterInputBank ? CharacterInputBank.LookRotation : Quaternion.identity;
            }
            ProcessJump();
            if(HasCharacterBody)
            {
                bool shouldSprint = wantsToSprint;
                if(moveVector.magnitude <= 0.5f)
                {
                    shouldSprint = false;
                }
                CharacterBody.IsSprinting = shouldSprint;
            }
        }

        protected virtual void ProcessJump()
        {
            if (wantsToJump && HasCharacterController)
            {
                CharacterController.Jump();
            }
        }
    }
}