using ElementalWard;
using UnityEngine;

namespace EntityStates.Player.Weapon.Staff
{
    public class FireShotgun : BaseCharacterState
    {
        public static float bulletRadius;
        public static float bulletRange;
        public static float damageCoefficient;
        public static float baseSpread;
        public static GameObject tracerPrefab;

        public int bulletCount;

        private float _maxSpread;
        public override void OnEnter()
        {
            base.OnEnter();
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
                tracerEffect = tracerPrefab
            }.Fire();
            outer.SetNextStateToMain();
        }
    }
}