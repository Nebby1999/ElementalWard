using ElementalWard;
using ElementalWard.Navigation;
using KinematicCharacterController;
using System;
using UnityEngine;

namespace EntityStates.CharacterAI
{
    public class BaseAIState : EntityState
    {
        private const string INVALID = "This member is not valid in a BaseAIState inheriting state";
        protected CharacterMaster CharacterMaster { get; private set; }
        protected CharacterMasterAI AI { get; private set; }
        protected NavigationAgent NavigationAgent { get; private set; }
        protected new CharacterBody CharacterBody { get; private set; }
        protected new CharacterInputBank CharacterInputBank { get; private set; }
        protected Transform CharacterTransform { get; private set; }
        protected GameObject CharacterGameObject { get; private set; }

        protected CharacterMasterAI.AIInputs aiInputs = default;
        public override void OnEnter()
        {
            base.OnEnter();
            CharacterMaster = GetComponent<CharacterMaster>();
            AI = GetComponent<CharacterMasterAI>();
            NavigationAgent = GetComponent<NavigationAgent>();
            if (AI)
            {
                CharacterBody = AI.Body;
                CharacterTransform = CharacterBody.transform;
                CharacterGameObject = CharacterBody.gameObject;
                CharacterInputBank = CharacterBody.InputBank;
            }
        }
        public virtual CharacterMasterAI.AIInputs GenerateAIInputs(in CharacterMasterAI.AIInputs previousInputs)
        {
            return aiInputs;
        }

        protected void AimAt(ref CharacterMasterAI.AIInputs dest, CharacterMasterAI.AITarget aimTarget)
        {
            if (aimTarget != null && aimTarget.IsValid && aimTarget.Position != null)
            {
                dest.aimDir = (aimTarget.Position.Value - CharacterInputBank.AimOrigin).normalized;
            }
        }

        protected void AimInDirection(ref CharacterMasterAI.AIInputs dest, Vector3 aimDirection)
        {
            if (aimDirection != Vector3.zero)
            {
                dest.aimDir = aimDirection;
            }
        }

        [Obsolete(INVALID, true)]
        protected new Transform GetSpriteBaseTransform() => throw new NotSupportedException(INVALID);

        [Obsolete(INVALID, true)]
        protected new Transform GetSpriteTransform() => throw new NotSupportedException(INVALID);

        [Obsolete(INVALID, true)]
        protected new SpriteRenderer3D GetSpriteRenderer() => throw new NotSupportedException(INVALID);

        [Obsolete(INVALID, true)]
        protected new CharacterAnimationEvents GetAnimationEvents() => throw new NotSupportedException(INVALID);

        [Obsolete(INVALID, true)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        protected sealed override Animator GetAnimator() => throw new NotSupportedException(INVALID);
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
    }
}