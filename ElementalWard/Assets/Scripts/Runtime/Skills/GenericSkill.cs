using Nebula;
using System;
using UnityEngine;

namespace ElementalWard
{
    public class GenericSkill : MonoBehaviour
    {
        public string genericSkillName;
        [SerializeField] private SkillDef _defaultSkill;
        public SkillDef SkillDef
        {
            get => _skillDef;
            set
            {
                if (_skillDef != value)
                {
                    if(_skillDef)
                    {
                        _skillDef.OnUnassign(this);
                    }
                    _skillDef = value;
                    if (_skillDef)
                    {
                        _skillDef.OnAssign(this);
                    }
                    OnSkillChanged();
                }
            }
        }
        private SkillDef _skillDef;
        public float CooldownTimer { get; set; }
        public uint MaxStock { get; private set; }
        public uint Stock { get; set; }
        public ISkillBehaviourCallback[] SkillBehaviourCallbacks { get; set; } = Array.Empty<ISkillBehaviourCallback>();
        public EntityStateMachine CachedStateMachine { get; private set; }

        private void Awake()
        {
            SkillDef = _defaultSkill;   
        }

        private void FixedUpdate()
        {
            if (!SkillDef)
                return;

            SkillDef.OnFixedUpdate(this);
        }

        public void TickRecharge(float fixedDeltaTime)
        {
            if (Stock > MaxStock)
                return;

            CooldownTimer -= fixedDeltaTime;
            if(CooldownTimer <= 0)
            {
                Stock += 1;
                CooldownTimer = 0;
                if (Stock > MaxStock)
                    Stock = MaxStock;
            }
        }


        private void OnSkillChanged()
        {
            Stock = SkillDef ? SkillDef.requiredStock : 0;
            MaxStock = Stock;
            CachedStateMachine = SkillDef ? EntityStateMachineBase.FindEntityStateMachineByName<EntityStateMachine>(gameObject, SkillDef.entityStateMachineName) : null;
        }

        private void ExecuteSkill()
        {
            if (!SkillDef)
                return;

            SkillDef.Execute(this);
        }
        public bool IsReady()
        {
            if (!SkillDef)
                return false;

            return SkillDef.CanExecute(this);
        }

        public bool IsInSkillState()
        {
            if (!SkillDef)
                return false;

            return CachedStateMachine.CurrentState.GetType() == (Type)SkillDef.stateType;
        }
        public bool ExecuteSkillIfReady()
        {
            var canExecute = IsReady();

            if(canExecute)
            {
                ExecuteSkill();
                return true;
            }
            return false;
        }
    }
}