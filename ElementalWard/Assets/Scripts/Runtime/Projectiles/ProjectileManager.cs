using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElementalWard.Projectiles;

namespace ElementalWard
{
    public static class ProjectileManager
    {
        public static GameObject SpawnProjectile(GameObject projectilePrefab, FireProjectileInfo info)
        {
            if (!projectilePrefab)
            {
                Debug.LogWarning("projectilePrefab is null; cannot spawn prefab.", projectilePrefab);
                return null;
            }
            var projectileController = projectilePrefab.GetComponent<ProjectileController>();
            if (!projectileController)
            {
                Debug.LogWarning("A projectile prefab must have a \"ProjectileController\" component; cannot spawn projectile.", projectilePrefab);
                return null;
            }

            var instantiationPos = info.instantiationPosition;
            var instantiationRotation = info.instantiationRotation;
            var instance = Object.Instantiate(projectilePrefab, instantiationPos, instantiationRotation);
            foreach (IProjectileInitialization initialization in instance.GetComponents<IProjectileInitialization>())
            {
                initialization.Initialize(info);
            }
            return instance;
        }
    }
}