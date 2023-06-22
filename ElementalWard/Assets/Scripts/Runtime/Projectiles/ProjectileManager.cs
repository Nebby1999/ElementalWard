using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public struct FireProjectileInfo
    {
        public BodyInfo owner;
        public BodyInfo target;
        public Vector3 instantiationPosition;
        public Quaternion instantiationRotation;
        public float? projectileSpeed;
        public float? projectileLifetime;
        public float? projectileDamageCoefficient;
        public DamageType? damageType;
    }
    public interface IProjectileInitialization
    {
        public void Initialize(FireProjectileInfo fireProjectileInfo);
    }
    public static class ProjectileManager
    {
        public static GameObject SpawnProjectile(GameObject projectilePrefab, FireProjectileInfo info)
        {
            if(!projectilePrefab)
            {
                Debug.LogWarning("projectilePrefab is null; cannot spawn vfx.", projectilePrefab);
                return null;
            }
            var projectileController = projectilePrefab.GetComponent<ProjectileController>();
            if(!projectileController)
            {
                Debug.LogWarning("A projectile prefab must have a \"ProjectileController\" component; cannot spawn projectile.", projectilePrefab);
                return null;
            }

            var instantiationPos = info.instantiationPosition;
            var instantiationRotation = info.instantiationRotation;
            var instance = Object.Instantiate(projectilePrefab, instantiationPos, instantiationRotation);
            foreach(IProjectileInitialization initialization in instance.GetComponents<IProjectileInitialization>())
            {
                initialization.Initialize(info);
            }
            return instance;
        }
    }
}