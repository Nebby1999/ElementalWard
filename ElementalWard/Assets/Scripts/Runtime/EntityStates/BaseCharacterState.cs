using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates
{
    public class BaseCharacterState : EntityState
    {
        public float attackSpeed;
        public float movementSpeed;
        public float damage;

        public override void OnEnter()
        {
            base.OnEnter();
            if(Body)
            {
                attackSpeed = Body.AttackSpeed;
                movementSpeed = Body.MovementSpeed;
                damage = Body.Damage;
            }
        }
    }
}