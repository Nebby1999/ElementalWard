using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Player.Weapon.Sword
{
    public class SwingSwordState : BaseCharacterState
    {
        public static float damageCoefficient;
        public static string hitBoxGroup;

        private HitBoxAttack attack;

        public override void OnEnter()
        {
            base.OnEnter();

            var damage = damageStat * damageCoefficient;
            var locator = GetSpriteBaseTransform();
            attack = new HitBoxAttack
            {
                attacker = new BodyInfo(CharacterBody),
                baseDamage = damage,
                damageType = DamageType.None,
                hitBoxGroup = HitBoxGroup.FindHitBoxGroup(locator.gameObject, hitBoxGroup)
            };

            attack.Fire();
            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
