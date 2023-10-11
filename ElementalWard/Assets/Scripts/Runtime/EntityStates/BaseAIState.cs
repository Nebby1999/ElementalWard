using ElementalWard;
using System;
using UnityEngine;

namespace EntityStates
{
    public class BaseAIState : EntityState
    {
        protected CharacterMaster Master { get; private set; }
        protected CharacterMasterAI CharacterMasterAI { get; private set; }
        protected new CharacterBody CharacterBody => Master.CurrentBody;
        protected new ICharacterMovementController ICharacterMovementController => CharacterMasterAI ? CharacterMasterAI.CurrentBodyComponents.characterMovementController : null;
        protected new CharacterInputBank CharacterInputBank => CharacterMasterAI ? CharacterMasterAI.CurrentBodyComponents.inputBank : null;
        protected Transform BodyTransform => CharacterMasterAI ? CharacterMasterAI.CurrentBodyComponents.transform : null;
        protected GameObject BodyGameObject => CharacterMasterAI ? CharacterMasterAI.CurrentBodyComponents.gameObject : null;
        protected new TeamComponent TeamComponent => CharacterMasterAI ? CharacterMasterAI.CurrentBodyComponents.teamComponent : null;

        [Obsolete("This member is not valid on a BaseAIState inheriting State", true)]
        protected new SkillManager SkillManager => CharacterMasterAI.CurrentBodyComponents.skillManager;

        [Obsolete("This member is not valid on a BaseAIState inheriting State", true)]
        protected new Rigidbody RigidBody => throw new NotImplementedException();

        [Obsolete("This member is not valid on a BaseAIState inheriting State", true)]
        protected new HealthComponent HealthComponent => throw new NotImplementedException();

        [Obsolete("This member is not valid on a BaseAIState inheriting State", true)]
        protected new SpriteLocator SpriteLocator => outer.CommonComponents.spriteLocator;

        public override void OnEnter()
        {
            base.OnEnter();
            Master = GetComponent<CharacterMaster>();
            CharacterMasterAI = GetComponent<CharacterMasterAI>();
        }

        public virtual CharacterMasterAI.AIInputs GenerateInputs()
        {
            return default;
        }

        public CharacterMasterAI.AITarget GetTarget()
        {
            return CharacterMasterAI ? CharacterMasterAI.CurrentTarget : CharacterMasterAI.AITarget.Invalid;
        }
    }
}