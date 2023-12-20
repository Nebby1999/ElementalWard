using ElementalWard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Player.Weapon.Staff
{
    public class FireBurst : BaseWeaponState
    {
        public static int fireCount;
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float bulletRadius;
        public static float bulletRange;
        public static GameObject tracerPrefab;

        private int _bulletsFired;
        private float _duration;
        private float _timeBetweenShots;
        private float _stopwatch;
        private HitscanAttack attack;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration * attackSpeedStat;
            _timeBetweenShots = _duration / (fireCount * 2);
            attack = new HitscanAttack
            {
                attacker = new BodyInfo(GameObject),
                baseDamage = damageStat * damageCoefficient,
                procCoefficient = procCoefficient,
                falloffCalculation = HitscanAttack.BulletFalloffCalculation,
                maxSpread = 0,
                minSpread = 0,
                raycastCount = 1,
                raycastLength = bulletRange,
                raycastRadius = bulletRadius,
                spreadPitchScale = 0,
                spreadYawScale = 0,
                tracerEffect = tracerPrefab,
            };

            PlayWeaponAnimation("Base", "Fire", "attackSpeed", _duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _stopwatch += Time.fixedDeltaTime;

            if (_stopwatch >= _timeBetweenShots && _bulletsFired < fireCount)
            {
                _stopwatch -= _timeBetweenShots;
                FireBullet();
            }

            if (FixedAge >= _duration && _bulletsFired == fireCount)
            {
                outer.SetNextStateToMain();
            }
        }

        private void FireBullet()
        {
            Ray ray = GetAimRay();
            var data = new VFXData
            {
                instantiationPosition = ray.origin,
                instantiationRotation = Quaternion.identity
            };
            data.AddProperty(CommonVFXProperties.Color, ElementProvider.Color ?? Color.white);

            attack.tracerData = data;
            attack.raycastOrigin = ray.origin;
            attack.raycastDirection = ray.direction;
            attack.Fire();
            _bulletsFired++;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
