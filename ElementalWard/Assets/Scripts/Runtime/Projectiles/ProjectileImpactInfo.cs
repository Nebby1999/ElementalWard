using UnityEngine;

namespace ElementalWard.Projectiles
{
    public struct ProjectileImpactInfo
    {
        public Collider collider;
        public Vector3 estimatedImpactPosition;
        public Vector3 estimatedImpactNormal;
    }
    public interface IProjectileImpact
    {
        public void OnImpact(ProjectileImpactInfo impactInfo);
    }
}