using UnityEngine;

namespace ElementalWard
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