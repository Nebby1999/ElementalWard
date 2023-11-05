using System;
using UnityEngine;

namespace ElementalWard.Projectiles
{
    public class ProjectileController : MonoBehaviour, IProjectileInitialization
    {
        public bool canImpactOnTrigger;
        public bool ignoreWorld;
        private IProjectileImpact[] _projectileImpacts = Array.Empty<IProjectileImpact>();
        private Collider[] _projectileColliders;
        private Rigidbody _rigidBody;
        private BodyInfo _owner;
        private void Awake()
        {
            _projectileImpacts = GetComponentsInChildren<IProjectileImpact>();
            _projectileColliders = GetComponents<Collider>();
            for (int i = 0; i < _projectileColliders.Length; i++)
            {
                _projectileColliders[i].enabled = false;
            }
        }

        private void Start()
        {
            for (int i = 0; i < _projectileColliders.Length; i++)
            {
                _projectileColliders[i].enabled = true;
            }
            IgnoreCollisionsWithOwner(true);
        }

        public void IgnoreCollisionsWithOwner(bool shouldIgnore)
        {
            if (!_owner)
                return;
            if (!_owner.TryGetComponent<HealthComponent>(out var hc))
                return;
            HurtBoxGroup hurtBoxGroup = hc.hurtBoxGroup;
            if (!hurtBoxGroup)
                return;
            HurtBox[] hurtBoxes = hurtBoxGroup.HurtBoxes;
            for (int i = 0; i < hurtBoxes.Length; i++)
            {
                var collider = hurtBoxes[i].TiedCollider;
                for (int j = 0; j < _projectileColliders.Length; j++)
                {
                    var collider2 = _projectileColliders[j];
                    Physics.IgnoreCollision(collider, collider2, shouldIgnore);
                }
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!canImpactOnTrigger)
                return;

            if (ignoreWorld && other.gameObject.layer == LayerIndex.world.IntVal)
                return;

            Vector3 normal = Vector3.zero;
            if (_rigidBody)
                normal = _rigidBody.velocity;

            ProjectileImpactInfo info = new()
            {
                collider = other,
                estimatedImpactNormal = -normal.normalized,
                estimatedImpactPosition = transform.position
            };
            foreach (var impact in _projectileImpacts)
            {
                impact.OnImpact(info);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (ignoreWorld && collision.gameObject.layer == LayerIndex.world.IntVal)
                return;

            ContactPoint[] contacts = collision.contacts;
            Collider collider = collision.collider;
            Vector3 pointOfImpact = contacts.Length == 0 ? collider.transform.position : contacts[0].point;
            Vector3 normal = contacts.Length == 0 ? Vector3.zero : contacts[0].normal;
            ProjectileImpactInfo info = new ProjectileImpactInfo
            {
                collider = collider,
                estimatedImpactNormal = normal,
                estimatedImpactPosition = pointOfImpact
            };
            foreach (var impact in _projectileImpacts)
            {
                impact.OnImpact(info);
            }
        }

        public void Initialize(FireProjectileInfo fireProjectileInfo)
        {
            _owner = fireProjectileInfo.owner;
        }
    }
}