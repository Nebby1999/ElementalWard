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
        private struct BulletHit
        {
            public Vector3 direction;
            public Vector3 hitPoint;
            public Vector3 surfaceNormal;
            public float distance;
            public Collider collider;
            public GameObject entityObject;
            public HurtBox? hurtBox;
        }
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
        public bool smartCollision;
        public GameObject tracerEffect;
        public VFXData tracerData;

        public FalloffCalculateDelegate falloffCalculation = DefaultFalloffCalculation;


        private RaycastHit[] _cachedHits;
        private HashSet<GameObject> _smartCollisionSet = new HashSet<GameObject>();
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
            List<BulletHit> bulletHit = new List<BulletHit>();
            if(raycastRadius == 0)
            {
                _cachedHits = Physics.RaycastAll(raycastOrigin, normal, raycastLength, LayerIndex.CommonMasks.Bullet, QueryTriggerInteraction.UseGlobal);
            }
            else
            {
                _cachedHits = Physics.SphereCastAll(raycastOrigin, raycastRadius, normal, raycastLength, LayerIndex.CommonMasks.Bullet, QueryTriggerInteraction.UseGlobal);
            }
            for(int i = 0; i < _cachedHits.Length; i++)
            {
                BulletHit hit = default;
                InitBulletHitFromRaycastHit(ref hit, raycastOrigin, normal, ref _cachedHits[i]);
            }
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
                    damageType = damageType,
                };
                damageInfo.damage *= DamageInfo.GetDamageModifier(hurtBox);

                hurtBox.HealthComponent.TakeDamage(damageInfo);
                hitPos = hit.point;
            }
            if (hitPos == Vector3.zero)
                hitPos = endPos;

            if(tracerEffect)
            {
                tracerData.AddProperty(CommonVFXProperties.Origin, raycastOrigin);
                tracerData.AddProperty(CommonVFXProperties.Start, hitPos);
                FXManager.SpawnVisualFX(tracerEffect, tracerData);
            }
        }

        private void InitBulletHitFromRaycastHit(ref BulletHit bulletHit, Vector3 rayOrigin, Vector3 rayDirection, ref RaycastHit raycastHit)
        {
            bulletHit.collider = raycastHit.collider;
            bulletHit.hurtBox = raycastHit.collider.GetComponent<HurtBox>();
            bulletHit.direction = rayDirection;
            bulletHit.distance = raycastHit.distance;
            bulletHit.surfaceNormal = raycastHit.normal;
            bulletHit.hitPoint = bulletHit.distance == 0 ? raycastOrigin : raycastHit.point;
            bulletHit.entityObject = (bulletHit.hurtBox && bulletHit.hurtBox.HealthComponent) ? bulletHit.hurtBox.HealthComponent.gameObject : raycastHit.collider.gameObject;
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