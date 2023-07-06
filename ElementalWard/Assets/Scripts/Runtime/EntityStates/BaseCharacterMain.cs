using UnityEngine;

namespace EntityStates
{
    public class BaseCharacterMain : BaseCharacterState
    {
        public bool HasICharacterMovementController { get; private set; }
        public bool HasCharacterInputBank { get; private set; }

        protected Vector3 moveVector;
        protected Vector3 aimDirection;
        protected bool wantsToJump;
        protected bool wantsToSprint;
        public override void OnEnter()
        {
            base.OnEnter();
            HasICharacterMovementController = ICharacterMovementController != null;
            HasCharacterInputBank = CharacterInputBank;
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

        protected virtual void ProcessInputs()
        {
            wantsToJump = false;
            wantsToSprint = false;
        }
    }
}