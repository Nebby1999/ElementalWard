using Nebula;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class ExplosiveAttack
    {
        public struct Hit
        {
            public HurtBox hurtBox;
            public Vector3 hitPos;
            public Vector3 hitNormal;
            public float distanceSqr;
            public bool passLos;

            public static Hit FromHurtBox(HurtBox hurtBox, ExplosiveAttack explosiveAttack)
            {
                var initialHitPos = hurtBox.transform.position;
                var initialHitNormal = explosiveAttack.explosionOrigin - initialHitPos;
                var initialDistanceSqr = initialHitNormal.sqrMagnitude;

                var flag = (hurtBox.TiedCollider.Raycast(new Ray(explosiveAttack.explosionOrigin, -initialHitNormal), out var hitInfo, explosiveAttack.explosionRadius));
                Hit result = default;
                result.distanceSqr = flag ? (explosiveAttack.explosionOrigin - hitInfo.point).sqrMagnitude : initialDistanceSqr;
                result.hitPos = flag ? hitInfo.point : initialHitPos;
                result.hitNormal = flag ? hitInfo.normal : initialHitNormal;
                result.passLos = explosiveAttack.requireLineOfSight;
                return result;
            }
        }

        public struct Result
        {
            public int hitCount;
            public Hit[] hits;
        }
        public delegate float FalloffCalculateDelegate(float distance, float explosionRadius);
        public BodyInfo attacker;
        public DamageType damageType;
        public float baseDamage;
        public Vector3 explosionOrigin;
        public float explosionRadius;
        public bool hitSelf;
        public bool requireLineOfSight;
        public FalloffCalculateDelegate falloffCalculation = DefaultFalloffCalculation;

        private static List<Hit> _hitsBuffer = new List<Hit>(256);
        private static List<HealthComponent> _encounteredHealthComponentsBuffer = new List<HealthComponent>();
        private float _damage;
        public Result Fire()
        {
            Hit[] hits = CollectHits();
            HandleHits(hits);
            return new Result
            {
                hits = hits,
                hitCount = hits.Length
            };
        }

        private Hit[] CollectHits()
        {
            _hitsBuffer.Clear();
            Collider[] colliders = Physics.OverlapSphere(explosionOrigin, explosionRadius, LayerIndex.entityPrecise.Mask);
            for(int i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                HurtBox hurtBox = collider.GetComponent<HurtBox>();
                if (!hurtBox)
                    continue;
                HealthComponent healthComponent = hurtBox.HealthComponent;
                if (!healthComponent)
                    continue;
                if((healthComponent.gameObject == attacker.gameObject) && !hitSelf)
                    continue;
                if (_encounteredHealthComponentsBuffer.Contains(healthComponent))
                    continue;
                _encounteredHealthComponentsBuffer.Add(healthComponent);

                var hit = Hit.FromHurtBox(hurtBox, this);
                _hitsBuffer.Add(hit);
            }
            _encounteredHealthComponentsBuffer.Clear();
            if(requireLineOfSight)
            {
                CheckLineOfSight();
            }
            return _hitsBuffer.ToArray();
        }

        private void HandleHits(Hit[] hits)
        {
            for(int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (!hit.passLos)
                    continue;

                float distance = Mathf.Sqrt(hit.distanceSqr);
                float damageCoefFromFalloff = falloffCalculation(distance, explosionRadius);
                HealthComponent healthComponent = hit.hurtBox.HealthComponent;

                DamageInfo damageInfo = new DamageInfo
                {
                    attackerBody = attacker,
                    damage = baseDamage * damageCoefFromFalloff,
                    damageType = damageType,
                };
                healthComponent.TakeDamage(damageInfo);
            }
        }

        private void CheckLineOfSight()
        {
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(_hitsBuffer.Count, Allocator.TempJob);
            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(_hitsBuffer.Count, Allocator.TempJob);
            var queryParams = new QueryParameters(LayerIndex.world.Mask);
            for(int i = 0; i <  _hitsBuffer.Count; i++)
            {
                var hit = _hitsBuffer[i];
                commands[i] = new RaycastCommand(explosionOrigin, hit.hitNormal, queryParams, Mathf.Sqrt(hit.distanceSqr));
            }
            RaycastCommand.ScheduleBatch(commands, results, 1).Complete();
            for(int j = 0; j < _hitsBuffer.Count; j++)
            {
                var hit = _hitsBuffer[j];
                if (results[j].collider)
                    hit.passLos = false;
                _hitsBuffer[j] = hit;
            }
            commands.Dispose();
            results.Dispose();
        }

        public static float DefaultFalloffCalculation(float distance, float explosionRadius)
        {
            return 1;
        }

        public static float LinearFalloffCalculation(float distance, float explosionRadius)
        {
            return 1 - Mathf.Clamp01(distance / explosionRadius);
        }

        public static float SweetspotFalloffCalculation(float distance, float explosionRadius)
        {
            return 1f - ((distance > explosionRadius / 2f) ? 0.75f : 0f);
        }
    }
}