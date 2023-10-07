using ElementalWard;
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
            /*if (HasSkillManager)
            {
                HandleSkill(BodySkillManager.SkillEnum.skill1, ref CharacterInputBank.skill1Button);
                HandleSkill(BodySkillManager.SkillEnum.skill2, ref CharacterInputBank.skill2Button);
                HandleSkill(BodySkillManager.SkillEnum.skill3, ref CharacterInputBank.skill3Button);
                HandleSkill(BodySkillManager.SkillEnum.skill4, ref CharacterInputBank.skill4Button);
            }*/
            wantsToJump = false;
            wantsToSprint = false;
        }

        /*private void HandleSkill(BodySkillManager.SkillEnum skillEnum, ref CharacterInputBank.Button button)
        {
            if (button.down)
            {
                SkillManager.ExecuteSkill(skillEnum);
            }
        }*/
    }
}