using System.Numerics;

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
                var motor = ICharacterMovementController.Motor;
                ICharacterMovementController.MovementDirection = motor.CharacterForward * moveVector.z + motor.CharacterRight * moveVector.x + motor.CharacterUp * moveVector.y;
                //ICharacterMovementController.CharacterRotation = CharacterInputBank.LookRotation;
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