using EntityStates;

namespace ElementalWard
{
    public interface ISkillBehaviourCallback
    {
        void OnAssigned(GenericSkill skillSlot);
        void OnUnassigned(GenericSkill skillSlot);
        bool OnCanExecute(GenericSkill skillSlot, bool previousValue);
        void OnExecute(GenericSkill skillSlot, EntityStateBase incomingState);
        float OnFixedUpdate(GenericSkill skillSlot, float deltaTime);
    }
}