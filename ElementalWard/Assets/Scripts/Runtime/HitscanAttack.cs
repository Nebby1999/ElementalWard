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
        public delegate float FalloffCalculateDelegate(float distance);
        public BodyInfo attacker;
        public DamageType damageType;
        public Vector3 raycastOrigin;
        public Vector3 raycastDirection;
        public float minSpread;
        public float maxSpread;
        public float spreadYawScale = 1f;
        public float spreadPitchScale = 1f;
        public float raycastLength;
        public float raycastRadius;
        public float baseDamage;
        public int raycastCount;
        public GameObject tracerEffect;
        public VFXData tracerData;

        public FalloffCalculateDelegate falloffCalculation = DefaultFalloffCalculation;


        private RaycastHit[] _cachedHits;
        public void Fire()
        {
            Vector3[] spreadArray = new Vector3[raycastCount];
            Vector3 up = Vector3.up;
            Vector3 axis = Vector3.Cross(up, raycastDirection);
            
            for(int i = 0; i < raycastCount; i++)
            {
                float x = UnityEngine.Random.Range(minSpread, maxSpread);
                float z = UnityEngine.Random.Range(0f, 360f);
                Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
                float y = vector.y;
                vector.y = 0f;
                float angle = (Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f) * spreadYawScale;
                float angle2 = Mathf.Atan2(y, vector.magnitude) * 57.29578f * spreadPitchScale;
                spreadArray[i] = Quaternion.AngleAxis(angle, up) * (Quaternion.AngleAxis(angle2, axis) * raycastDirection);
            }

            for (int j = 0; j < raycastCount; j++)
            {
                FireSingle(spreadArray[j]);
            }
        }

        private void FireSingle(Vector3 normal)
        {
            Vector3 endPos = raycastOrigin + normal * raycastLength;
            _cachedHits = Physics.SphereCastAll(raycastOrigin, raycastRadius, normal, raycastLength, LayerIndex.CommonMasks.Bullet, QueryTriggerInteraction.UseGlobal);
            Vector3 hitPos = Vector3.zero;
            foreach(var hit in _cachedHits)
            {
                Collider hitCollider = hit.collider;
                if (!hitCollider)
                    continue;

                var hurtBox = hitCollider.GetComponent<HurtBox>();
                if (!hurtBox || !hurtBox.HealthComponent)
                    continue;

                var falloffFactor = falloffCalculation(hit.distance);
                var damageInfo = new DamageInfo
                {
                    attackerBody = attacker,
                    damage = baseDamage * falloffFactor,
                };

                hurtBox.HealthComponent.TakeDamage(damageInfo);
                hitPos = hit.point;
            }
            if (hitPos == Vector3.zero)
                hitPos = endPos;

            if(tracerEffect)
            {
                tracerData.origin = raycastOrigin;
                tracerData.start = hitPos;
                FXManager.SpawnVisualFX(tracerEffect, tracerData);
            }
        }

        public static float DefaultFalloffCalculation(float distance)
        {
            return 1;
        }

        public static float BuckshotFalloffCalculation(float distance)
        {
            throw new NotImplementedException();
        }

        public static float BulletFalloffCalculation(float distance)
        {
            throw new NotImplementedException();
        }
    }
}