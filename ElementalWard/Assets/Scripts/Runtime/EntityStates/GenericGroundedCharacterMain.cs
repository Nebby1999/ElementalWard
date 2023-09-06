using ElementalWard;

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
            if (HasICharacterMovementController && HasCharacterInputBank)
            {
                var motor = ICharacterMovementController.Motor;
                ICharacterMovementController.MovementDirection = moveVector;
                ICharacterMovementController.CharacterRotation = CharacterInputBank.LookRotation;
            }
            ProcessJump();
            if (HasCharacterBody)
            {
                bool shouldSprint = wantsToSprint;
                if (moveVector.magnitude <= 0.5f)
                    shouldSprint = false;

                CharacterBody.IsSprinting = shouldSprint;
            }
        }

        protected virtual void ProcessJump()
        {
            if (wantsToJump && HasGroundedCharacterMovementController)
            {
                GroundedCharacterMovementController.Jump();
            }
        }
    }
}