using UnityEngine;

namespace ElementalWard.Projectiles
{
    [RequireComponent(typeof(ProjectileExplosion))]
    public class AddToRemoteDetonateTracker : MonoBehaviour, IProjectileInitialization
    {
        public ProjectileExplosion ProjectileExplosion { get; private set; }

        private void Awake()
        {
            ProjectileExplosion = GetComponent<ProjectileExplosion>();
        }
        public void Initialize(FireProjectileInfo info)
        {
            var owner = info.owner.gameObject;
            if(owner && owner.TryGetComponent<RemoteDetonateTracker>(out var component))
            {
                component.AddProjectile(this);
            }
        }
    }
}