using ElementalWard;
using ElementalWard.Projectiles;
using UnityEngine;

namespace EntityStates.Player
{
    public class FireBolt : BaseCharacterState
    {
        public static GameObject projectilePrefab;
        public static float projectileVelocity;

        public override void OnEnter()
        {
            base.OnEnter();

            Ray aimRay = GetAimRay();
            var bodyInfo = new BodyInfo(CharacterBody);
            FireProjectileInfo info = new FireProjectileInfo
            {
                owner = bodyInfo,
                instantiationPosition = aimRay.origin,
                instantiationRotation = Quaternion.LookRotation(aimRay.direction, Vector3.up)
            };
            info.AddProperty(CommonProjectileProperties.MovementSpeed, projectileVelocity);
            ProjectileManager.SpawnProjectile(projectilePrefab, info);
            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}