using ElementalWard;
using UnityEngine;

namespace EntityStates
{
    public class BaseCharacterState : EntityState
    {
        public bool HasSkillManager { get; private set; }
        public bool HasCharacterBody { get; private set; }
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