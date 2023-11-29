using ElementalWard;
using UnityEngine;

namespace EntityStates
{
    public class BaseCharacterState : EntityState
    {
        public bool HasSkillManager { get; private set; }
        public bool HasCharacterBody { get; private set; }
        public bool HasElementProvider { get; private set; }
        public ElementalInfusionController ElementalInfusionController { get; private set; }
        public ElementalAffinityController ElementalAffinityController { get; private set; }
        public float attackSpeedStat;
        public float movementSpeedStat;
        public float damageStat;

        public override void OnEnter()
        {
            base.OnEnter();
            HasCharacterBody = CharacterBody;
            if (HasCharacterBody)
            {
                attackSpeedStat = CharacterBody.AttackSpeed;
                movementSpeedStat = CharacterBody.MovementSpeed;
                damageStat = CharacterBody.Damage;
            }
            HasSkillManager = SkillManager;
            HasElementProvider = ElementProvider != null;
            if(HasElementProvider)
            {
                ElementalInfusionController = ElementProvider as ElementalInfusionController;
                ElementalAffinityController = ElementProvider as ElementalAffinityController;
            }
        }

        protected Ray GetAimRay()
        {
            if (CharacterInputBank)
            {
                return new Ray(CharacterInputBank.AimOrigin, CharacterInputBank.AimDirection);
            }
            return new Ray(Transform.position, Transform.forward);
        }

        protected HitBoxGroup GetHitBoxGroup(string groupName)
        {
            return HitBoxGroup.FindHitBoxGroup(GetSpriteBaseTransform().gameObject, groupName);
        }
    }
}