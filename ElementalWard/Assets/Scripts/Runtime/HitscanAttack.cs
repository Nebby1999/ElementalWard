using Nebula;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class HitscanAttack
    {
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
        public LayerMask hitMask = LayerIndex.CommonMasks.Bullet;
        public LayerMask stopperMask = LayerIndex.CommonMasks.Bullet;

        public FalloffCalculateDelegate falloffCalculation = DefaultFalloffCalculation;
        public HitCallback hitCallback = DefaultHitCallback;

        private RaycastHit[] _cachedHits;
        private HashSet<GameObject> _smartCollisionSet = new HashSet<GameObject>();
        public void Fire()
        {
            Vector3[] spreadArray = new Vector3[raycastCount];
            Vector3 up = Vector3.up;
            Vector3 axis = Vector3.Cross(up, raycastDirection);

            for (int i = 0; i < raycastCount; i++)
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
            List<Hit> bulletHit = new List<Hit>();
            if (raycastRadius == 0)
            {
                _cachedHits = Physics.RaycastAll(raycastOrigin, normal, raycastLength, hitMask, QueryTriggerInteraction.UseGlobal);
            }
            else
            {
                _cachedHits = Physics.SphereCastAll(raycastOrigin, raycastRadius, normal, raycastLength, hitMask, QueryTriggerInteraction.UseGlobal);
            }
            for (int i = 0; i < _cachedHits.Length; i++)
            {
                Hit hit = default;
                InitBulletHitFromRaycastHit(ref hit, raycastOrigin, normal, ref _cachedHits[i]);
                
                if (hit.hurtBox && hit.hurtBox.TeamIndex == attacker.team)
                    continue;
     
                if(hitCallback(this, ref hit))
                {
                    endPos = hit.hitPoint;
                    break;
                }
            }
#if DEBUG && UNITY_EDITOR
            GlobalGizmos.EnqueueGizmoDrawing(() =>
            {
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawLine(raycastOrigin, endPos, 10);
            });
#endif

            if (tracerEffect)
            {
                var data = tracerData;
                data.AddProperty(CommonVFXProperties.Origin, raycastOrigin);
                data.AddProperty(CommonVFXProperties.Start, endPos);
                FXManager.SpawnVisualFX(tracerEffect, data);
            }
        }

        private void InitBulletHitFromRaycastHit(ref Hit bulletHit, Vector3 rayOrigin, Vector3 rayDirection, ref RaycastHit raycastHit)
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
            return 0.25f + Mathf.Clamp01(Mathf.InverseLerp(25f, 7f, distance)) * 0.75f;
        }

        public static float BulletFalloffCalculation(float distance)
        {
            return 0.5f + Mathf.Clamp01(Mathf.InverseLerp(60f, 25f, distance)) * 0.5f;
        }

        public static bool DefaultHitCallback(HitscanAttack attack, ref Hit hit)
        {
            bool isInStopperMask = attack.stopperMask.Contains(hit.entityObject.layer);
            Vector3 hitPos = Vector3.zero;
            Collider hitCollider = hit.collider;
            if (!hitCollider)
                return isInStopperMask;

            var hurtBox = hitCollider.GetComponent<HurtBox>();
            if (!hurtBox || !hurtBox.HealthComponent)
                return isInStopperMask;

            var falloffFactor = attack.falloffCalculation(hit.distance);
            var damageInfo = new DamageInfo
            {
                attackerBody = attack.attacker,
                damage = attack.baseDamage * falloffFactor,
                damageType = attack.damageType,
            };
            damageInfo.damage *= DamageInfo.GetDamageModifier(hurtBox);

            hurtBox.HealthComponent.TakeDamage(damageInfo);

            return isInStopperMask;
        }

        public delegate float FalloffCalculateDelegate(float distance);
        public delegate bool HitCallback(HitscanAttack attack, ref Hit hit);
        public struct Hit
        {
            public Vector3 direction;
            public Vector3 hitPoint;
            public Vector3 surfaceNormal;
            public float distance;
            public Collider collider;
            public GameObject entityObject;
            public HurtBox? hurtBox;
        }
    }
}