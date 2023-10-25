using ElementalWard;
using KinematicCharacterController;
using UnityEngine;

namespace EntityStates
{
    public class BaseCharacterMain : BaseCharacterState
    {
        public bool HasCharacterInputBank { get; private set; }
        public bool HasCharacterController { get; private set; }
        public bool IsGrounded => HasCharacterController && CharacterController.IsGrounded;

        protected Vector3 moveVector;
        protected Vector3 aimDirection;
        protected bool wantsToJump;
        protected bool wantsToSprint;
        public override void OnEnter()
        {
            base.OnEnter();
            HasCharacterController = CharacterController;
            HasCharacterInputBank = CharacterInputBank;
        }

        protected virtual void GatherInputs()
        {
            if (HasCharacterInputBank)
            {
                moveVector = CharacterInputBank.moveVector;
                aimDirection = CharacterInputBank.AimDirection;
                wantsToJump = CharacterInputBank.jumpButton.down;
                wantsToSprint |= CharacterInputBank.sprintButton.down;
            }
        }

        protected virtual void ProcessInputs()
        {
            if (HasSkillManager)
            {
                HandleSkill(SkillManager.Primary, ref CharacterInputBank.primaryButton);
                HandleSkill(SkillManager.Secondary, ref CharacterInputBank.secondaryButton);
                HandleSkill(SkillManager.Utility, ref CharacterInputBank.utilityButton);
                HandleSkill(SkillManager.Special, ref CharacterInputBank.specialButton);
            }
            wantsToJump = false;
            wantsToSprint = false;
        }

        private void HandleSkill(GenericSkill skill, ref CharacterInputBank.Button button)
        {
            if (button.down && skill)
            {
                skill.ExecuteSkillIfReady();
            }
        }
    }
}