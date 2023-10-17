using EntityStates;
using Nebula;
using Nebula.Serialization;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New SkillDef", menuName = ElementalWardApplication.APP_NAME + "/Skills/SkillDef")]
    public class SkillDef : NebulaScriptableObject
    {
        public float baseCooldown;
        public uint requiredStock = 1;
        public string entityStateMachineName;
        [SerializableSystemType.RequiredBaseType(typeof(EntityState))]
        public SerializableSystemType stateType;
        public InterruptPriority interruptStrength = InterruptPriority.Any;

        public bool CanExecute(GenericSkill skillSlot)
        {
            if(skillSlot.Stock >= requiredStock && skillSlot.CooldownTimer <= 0)
            {
                return skillSlot.CachedStateMachine.CanInterruptState(interruptStrength);
            }
            return false;
        }

        public void Execute(GenericSkill skillSlot)
        {
            if (!skillSlot)
                return;

            var stateMachine = skillSlot.CachedStateMachine ? skillSlot.CachedStateMachine : EntityStateMachine.FindEntityStateMachineByName<EntityStateMachine>(skillSlot.gameObject, entityStateMachineName);

            if(!stateMachine)
            {
                Debug.LogWarning($"{this} cannot be executed on {skillSlot.gameObject.name} because it doesnt have a state machine with name {entityStateMachineName}.", skillSlot.gameObject);
                return;
            }

            stateMachine.SetNextState(EntityStateCatalog.InstantiateState(stateType));
            skillSlot.Stock--;
            skillSlot.CooldownTimer = baseCooldown;
        }

        public void OnFixedUpdate(GenericSkill skillSlot)
        {
            skillSlot.TickRecharge(Time.fixedDeltaTime);
        }
    }

    public enum InterruptPriority
    {
        Any = 0,
        Skill = 1,
        PrioritySkill = 2,
        Stun = 3,
        Death = 4
    }
}