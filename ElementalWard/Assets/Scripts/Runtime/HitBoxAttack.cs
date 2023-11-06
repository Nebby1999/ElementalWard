using Nebula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    public class HitBoxAttack
    {
        public BodyInfo attacker;
        public DamageType damageType;
        public float baseDamage;
        public HitBoxGroup hitBoxGroup;

        private static List<Hit> _hitsBuffer = new List<Hit>(256);
        private static List<HealthComponent> _encounteredHealthComponentsBuffer = new List<HealthComponent>(64);

        public bool Fire()
        {
            if (!hitBoxGroup)
                return false;

            _hitsBuffer.Clear();
            HitBox[] hitBoxes = hitBoxGroup.hitboxes;
            foreach(HitBox hitBox in hitBoxes)
            {
                if (!hitBox || !hitBox.enabled || !hitBox.isActiveAndEnabled)
                    continue;

                Transform transform = hitBox.transform;
                Vector3 position = transform.position;
                Vector3 halfExtents = transform.lossyScale * 0.5f;
                Quaternion rotation = transform.rotation;

#if DEBUG
                GlobalGizmos.EnqueueGizmoDrawing(() =>
                {
                    var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                    Gizmos.DrawWireMesh(mesh, position, rotation, halfExtents * 2);
                });
#endif

                CollectHits(position, halfExtents, rotation);
            }

            ProcessHits(_hitsBuffer);
            bool result = _hitsBuffer.Count > 0;
            _hitsBuffer.Clear();
            return result;
        }

        private void CollectHits(Vector3 position, Vector3 halfExtents, Quaternion rotation)
        {
            Collider[] colliders = Physics.OverlapBox(position, halfExtents, rotation, LayerIndex.entityPrecise.Mask);

            for(int i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                HurtBox hurtBox = collider.GetComponent<HurtBox>();
                if (!hurtBox)
                    continue;

                HealthComponent healthComponent = hurtBox.HealthComponent;
                if (!healthComponent)
                    continue;
                if (_encounteredHealthComponentsBuffer.Contains(healthComponent))
                    continue;
                _encounteredHealthComponentsBuffer.Add(healthComponent);

                var hit = new Hit
                {
                    hurtBox = hurtBox,
                    hitPosition = hurtBox.transform.position,
                    pushDirection = hurtBox.transform.position - position
                };
                _hitsBuffer.Add(hit);
            }
            _encounteredHealthComponentsBuffer.Clear();
        }

        private void ProcessHits(List<Hit> hits)
        {
            for(int i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];

                HealthComponent healthComponent = hit.hurtBox.HealthComponent;

                DamageInfo damageInfo = new DamageInfo
                {
                    attackerBody = attacker,
                    damage = baseDamage,
                    damageType = damageType,
                };
                healthComponent.TakeDamage(damageInfo);
            }
        }

        public struct Hit
        {
            public HurtBox hurtBox;
            public Vector3 hitPosition;
            public Vector3 pushDirection;
        }
    }
}
