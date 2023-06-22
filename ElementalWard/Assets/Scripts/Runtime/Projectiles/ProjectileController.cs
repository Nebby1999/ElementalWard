using System;
using UnityEngine;

namespace ElementalWard
{
    public class ProjectileController : MonoBehaviour
    {
        public bool canImpactOnTrigger;
        public bool ignoreWorld;
        private IProjectileImpact[] _projectileImpacts = Array.Empty<IProjectileImpact>();
        private Rigidbody _rigidBody;
        private void Awake()
        {
            _projectileImpacts = GetComponentsInChildren<IProjectileImpact>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!canImpactOnTrigger)
                return;

            if (ignoreWorld && other.gameObject.layer == LayerIndex.world.IntVal)
                return;

            Vector3 normal = Vector3.zero;
            if(_rigidBody)
                normal = _rigidBody.velocity;

            ProjectileImpactInfo info = new()
            {
                collider = other,
                estimatedImpactNormal = -normal.normalized,
                estimatedImpactPosition = transform.position
            };
            foreach(var impact in _projectileImpacts)
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
            foreach(var impact in _projectileImpacts)
            {
                impact.OnImpact(info);
            }
        }
    }
}