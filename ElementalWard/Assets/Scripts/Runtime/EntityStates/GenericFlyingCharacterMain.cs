using UnityEngine;

namespace EntityStates
{
    public class GenericFlyingCharacterMain : BaseFlyingCharacterMain
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
            if(HasICharacterMovementController)
            {
                ICharacterMovementController.CharacterRotation = CharacterInputBank.LookRotation;
                ICharacterMovementController.MovementDirection = moveVector;
            }

            if(HasCharacterBody)
            {
                bool shouldSprint = wantsToSprint;
                if (moveVector.magnitude <= 0.5f)
                    shouldSprint = false;
                CharacterBody.IsSprinting = shouldSprint;
            }
        }
    }
}