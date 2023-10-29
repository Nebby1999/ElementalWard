using ElementalWard;

namespace EntityStates
{
    public class BaseSkillState : BaseCharacterState, ISkillState
    {
        public GenericSkill ActivatorSkillSlot { get; set; }

        private SkillSlot _assignedSlot;

        public override void OnEnter()
        {
            base.OnEnter();
            _assignedSlot = SkillManager ? SkillManager.FindSkillSlot(ActivatorSkillSlot) : SkillSlot.None;
        }

        protected virtual bool IsSkillDown()
        {
            if (!CharacterInputBank)
                return false;

            switch(_assignedSlot)
            {
                case SkillSlot.Primary:
                    return CharacterInputBank.primaryButton.down;
                case SkillSlot.Secondary:
                    return CharacterInputBank.secondaryButton.down;
                case SkillSlot.Utility:
                    return CharacterInputBank.utilityButton.down;
                case SkillSlot.Special:
                    return CharacterInputBank.specialButton.down;
                default:
                    return false;
            }
        }
    }

    public interface ISkillState
    {
        GenericSkill ActivatorSkillSlot { get; set; }
    }
}