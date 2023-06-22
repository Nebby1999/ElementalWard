using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates
{
    public class BaseCharacterState : EntityState
    {
        public bool HasCharacterBody { get; private set; }
        public float attackSpeedStat;
        public float movementSpeedStat;
        public float damageStat;

        public override void OnEnter()
        {
            base.OnEnter();
            HasCharacterBody = CharacterBody;
            if(HasCharacterBody)
            {
                attackSpeedStat = CharacterBody.AttackSpeed;
                movementSpeedStat = CharacterBody.MovementSpeed;
                damageStat = CharacterBody.Damage;
            }
        }

        protected Ray GetAimRay()
        {
            if(CharacterInputBank)
            {
                return new Ray(CharacterInputBank.AimOrigin, CharacterInputBank.AimDirection);
            }
            return new Ray(Transform.position, Transform.forward);
        }
    }
}