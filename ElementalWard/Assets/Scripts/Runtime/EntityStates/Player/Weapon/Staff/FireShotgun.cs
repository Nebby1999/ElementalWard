using ElementalWard;
using UnityEngine;

namespace EntityStates.Player.Weapon.Staff
{
    public class FireShotgun : BaseWeaponState
    {
        public static float bulletRadius;
        public static float bulletRange;
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseSpread;
        public static float baseDuration;
        public static GameObject tracerPrefab;

        public int bulletCount;

        private float _maxSpread;
        private float _duration;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            _maxSpread = baseSpread * bulletCount;
            VFXData data = new VFXData
            {
                instantiationPosition = aimRay.origin,
                instantiationRotation = Quaternion.identity,
            };
            data.AddProperty(CommonVFXProperties.Color, Color.white);

            new HitscanAttack()
            {
                attacker = new BodyInfo(GameObject),
                falloffCalculation = HitscanAttack.BuckshotFalloffCalculation,
                baseDamage = damageStat * damageCoefficient,
                minSpread = 0,
                maxSpread = _maxSpread,
                raycastCount = bulletCount,
                raycastDirection = aimRay.direction,
                raycastOrigin = aimRay.origin,
                raycastLength = bulletRange,
                raycastRadius = bulletRadius,
                tracerData = data,
                tracerEffect = tracerPrefab,
                procCoefficient = procCoefficient
            }.Fire();
            PlayWeaponAnimation("Base", "Fire");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(FixedAge > _duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}