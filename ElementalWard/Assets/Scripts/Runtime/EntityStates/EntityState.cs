using ElementalWard;
using KinematicCharacterController;
using UnityEngine;

namespace EntityStates
{
    public class EntityState : EntityStateBase
    {
        protected new EntityStateMachine outer;
        protected CharacterBody CharacterBody => outer.CommonComponents.characterBody;
        protected CharacterMotorController CharacterController => outer.CommonComponents.characterController;
        protected KinematicCharacterMotor CharacterMotor => outer.CommonComponents.characterMotor;
        protected CharacterInputBank CharacterInputBank => outer.CommonComponents.inputBank;
        protected Transform Transform => outer.CommonComponents.transform;
        protected GameObject GameObject => outer.CommonComponents.gameObject;
        protected SkillManager SkillManager => outer.CommonComponents.skillManager;
        protected Rigidbody RigidBody => outer.CommonComponents.rigidBody;
        protected TeamComponent TeamComponent => outer.CommonComponents.teamComponent;
        protected HealthComponent HealthComponent => outer.CommonComponents.healthComponent;
        protected SpriteLocator SpriteLocator => outer.CommonComponents.spriteLocator;
        protected IElementProvider ElementProvider => outer.CommonComponents.elementProvider;

        public override void OnEnter()
        {
            base.OnEnter();
            outer = base.outer as EntityStateMachine;
        }
        protected Transform GetSpriteBaseTransform()
        {
            return SpriteLocator ? SpriteLocator.spriteBaseTransform : null;
        }

        protected Transform GetSpriteTransform()
        {
            return SpriteLocator ? SpriteLocator.SpriteTransform : null;
        }

        protected SpriteRenderer3D GetSpriteRenderer()
        {
            return SpriteLocator ? SpriteLocator.SpriteTransform.GetComponent<SpriteRenderer3D>() : null;
        }
        protected CharacterAnimationEvents GetAnimationEvents()
        {
            return SpriteLocator ? SpriteLocator.SpriteTransform.GetComponent<CharacterAnimationEvents>() : null;
        }
        protected override Animator GetAnimator()
        {
            if (SpriteLocator && SpriteLocator.SpriteTransform)
            {
                return SpriteLocator.SpriteTransform.GetComponent<Animator>();
            }
            return null;
        }

        public virtual InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}
