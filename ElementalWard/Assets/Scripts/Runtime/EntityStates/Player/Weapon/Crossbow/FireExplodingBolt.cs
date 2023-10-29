using ElementalWard;
using ElementalWard.Projectiles;
using UnityEngine;

namespace EntityStates.Player.Weapon.Crossbow
{
    public class FireExplodingBolt : BaseCharacterState
    {
        public static GameObject boltPrefab;
        public static float radius;

        public float charge;
        public override void OnEnter()
        {
            base.OnEnter();
            Ray aimRay = GetAimRay();
            new HitscanAttack
            {
                attacker = new BodyInfo(CharacterBody),
                falloffCalculation = HitscanAttack.DefaultFalloffCalculation,
                hitCallback = HitCallback,
                damageType = DamageType.None,
                baseDamage = damageStat * charge,
                maxSpread = 0,
                minSpread = 0,
                spreadYawScale = 0,
                spreadPitchScale = 0,
                raycastRadius = radius,
                raycastLength = 1000,
                raycastCount = 1,
                raycastDirection = aimRay.direction,
                raycastOrigin = aimRay.origin,
            }.Fire();
        }

        public bool HitCallback(HitscanAttack attack, ref HitscanAttack.Hit hit)
        {
            if (boltPrefab && hit.collider)
            {
                FireProjectileInfo info = new FireProjectileInfo
                {
                    owner = attack.attacker,
                    target = new BodyInfo(hit.entityObject),
                    instantiationPosition = hit.hitPoint,
                    instantiationRotation = Quaternion.LookRotation(hit.surfaceNormal, hit.entityObject.transform.up),
                };
                info.AddProperty("normalValue", hit.surfaceNormal);
                ProjectileManager.SpawnProjectile(boltPrefab, info);
            }
            return true;
        }
    }
}