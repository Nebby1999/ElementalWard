using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Player.Weapon.Sword
{
    public class SwingSwordState : BaseWeaponState
    {
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float requiredEssence;
        public static string hitBoxGroup;

        private HitBoxAttack attack;
        private float _duration;
        private CharacterAnimationEvents _events;
        private bool _hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            _duration = baseDuration / attackSpeedStat;
            var damage = damageStat * damageCoefficient;
            var locator = GetSpriteBaseTransform();
            var attacker = new BodyInfo(GameObject);
            attacker.NullElementProvider();
            attacker.fallbackElement = ElementProvider.GetElementDefForAttack(requiredEssence);
            attack = new HitBoxAttack
            {
                attacker = attacker,
                baseDamage = damage,
                procCoefficient = procCoefficient,
                damageType = DamageType.None,
                hitBoxGroup = HitBoxGroup.FindHitBoxGroup(locator.gameObject, hitBoxGroup)
            };


            PlayWeaponAnimation("Base", "Fire", "attackSpeed", _duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(!_hasFired && FixedAge > _duration / 2)
            {
                _hasFired = true;
                attack.Fire();
            }
            if(FixedAge > _duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
