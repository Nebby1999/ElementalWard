using ElementalWard;
using ElementalWard.Projectiles;
using UnityEngine;

namespace EntityStates.Player.Weapon.Crossbow
{
    public class FireBolt : BaseCharacterState
    {
        public static float baseDuration;
        public static float damageCoefficient;
        public static float projectileVelocity;
        public static GameObject projectilePrefab;

        private float _duration;
        public override void OnEnter()
        {
            base.OnEnter();

            _duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            var bodyInfo = new BodyInfo(CharacterBody);
            FireProjectileInfo info = new FireProjectileInfo
            {
                owner = bodyInfo,
                instantiationPosition = aimRay.origin,
                instantiationRotation = Quaternion.LookRotation(aimRay.direction, Vector3.up)
            };
            info.AddProperty(CommonProjectileProperties.MovementSpeed, projectileVelocity);
            info.AddProperty(CommonProjectileProperties.DamageCoefficient, damageCoefficient);
            ProjectileManager.SpawnProjectile(projectilePrefab, info);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (FixedAge > _duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}