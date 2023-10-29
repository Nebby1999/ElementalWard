using EntityStates;
using Nebula;
using Nebula.Serialization;
using System;
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
        [SerializableSystemType.RequiredBaseType(typeof(ISkillBehaviourCallback))]
        public SerializableSystemType[] skillBehaviourCallbacks = Array.Empty<SerializableSystemType>();
        public InterruptPriority interruptStrength = InterruptPriority.Any;

        public void OnAssign(GenericSkill skillSlot)
        {
            ISkillBehaviourCallback[] callbacks = new ISkillBehaviourCallback[skillBehaviourCallbacks.Length];

            for(int i = 0; i < skillBehaviourCallbacks.Length; i++)
            {
                callbacks[i] = (ISkillBehaviourCallback)Activator.CreateInstance(skillBehaviourCallbacks[i]);
                callbacks[i].OnAssigned(skillSlot);
            }
            skillSlot.SkillBehaviourCallbacks = callbacks;
        }

        public void OnUnassign(GenericSkill skillSlot)
        {
            for(int i = 0; i < skillSlot.SkillBehaviourCallbacks.Length; i++)
            {
                skillSlot.SkillBehaviourCallbacks[i].OnUnassigned(skillSlot);
            }
            skillSlot.SkillBehaviourCallbacks = null;
        }

        public bool CanExecute(GenericSkill skillSlot)
        {
            if(skillSlot.Stock >= requiredStock && skillSlot.CooldownTimer <= 0)
            {
                bool canInterruptState = skillSlot.CachedStateMachine.CanInterruptState(interruptStrength);
                
                for(int i = 0; i < skillSlot.SkillBehaviourCallbacks.Length; i++)
                {
                    canInterruptState = skillSlot.SkillBehaviourCallbacks[i].OnCanExecute(skillSlot, canInterruptState);

                    if (canInterruptState == false)
                        return false;
                }
                return canInterruptState;
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

            var state = EntityStateCatalog.InstantiateState(stateType);
            if(state is ISkillState iSkillState)
            {
                iSkillState.ActivatorSkillSlot = skillSlot;
            }

            for(int i = 0; i < skillSlot.SkillBehaviourCallbacks.Length; i++)
            {
                skillSlot.SkillBehaviourCallbacks[i].OnExecute(skillSlot, state);
            }
            stateMachine.SetNextState(state);
            skillSlot.Stock--;
            skillSlot.CooldownTimer = baseCooldown;
        }

        public void OnFixedUpdate(GenericSkill skillSlot)
        {
            var deltaTime = Time.fixedDeltaTime;
            for(int i = 0; i < skillSlot.SkillBehaviourCallbacks.Length; i++)
            {
                deltaTime = skillSlot.SkillBehaviourCallbacks[i].OnFixedUpdate(skillSlot, deltaTime);
            }
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