using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Nebula;

namespace ElementalWard
{
    public class HitscanAttack
    {
        public BodyInfo attacker;
        public Vector3 raycastOrigin;
        public Vector3 raycastDirection;
        public Vector2 raycastSpread;
        public float raycastLength;
        public float raycastRadius;
        public float damageCoefficient;
        public int raycastCount;
        public GameObject tracerEffect;
        public VFXData tracerData;

        private float _damage;
        private RaycastHit[] _cachedHits;
        public void Fire()
        {
            _damage = attacker.characterBody.AsValidOrNull()?.Damage ?? 0;
            _damage *= damageCoefficient;

            for (int i = 0; i < raycastCount; i++)
            {
                FireSingle();
            }
        }

        private void FireSingle()
        {
            _cachedHits = Physics.SphereCastAll(raycastOrigin, raycastRadius, raycastDirection, raycastLength, LayerIndex.CommonMasks.Bullet, QueryTriggerInteraction.UseGlobal);
            Vector3 hitPos = Vector3.zero;
            foreach(var hit in _cachedHits)
            {
                Collider hitCollider = hit.collider;
                if (!hitCollider)
                    continue;

                var hurtBox = hitCollider.GetComponent<HurtBox>();
                if (!hurtBox || !hurtBox.HealthComponent)
                    continue;

                var damageInfo = new DamageInfo
                {
                    attackerBody = attacker,
                    damage = _damage
                };

                hurtBox.HealthComponent.TakeDamage(damageInfo);
                hitPos = hit.point;
            }
            if (hitPos == Vector3.zero)
                hitPos = raycastDirection * raycastLength;

            if(tracerEffect)
            {
                tracerData.origin = raycastOrigin;
                tracerData.start = hitPos;
                FXManager.SpawnVisualFX(tracerEffect, tracerData);
            }
        }
    }
}