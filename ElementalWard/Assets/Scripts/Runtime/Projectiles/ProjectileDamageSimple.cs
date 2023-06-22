using UnityEngine;
using Nebula;

namespace ElementalWard
{
    public class ProjectileDamageSimple : MonoBehaviour, IProjectileImpact, IProjectileInitialization
    {
        public DamageType defaultDamageType;
        public float defaultDamageCoefficient;
        private BodyInfo _owner;
        public void Initialize(FireProjectileInfo fireProjectileInfo)
        {
            if (fireProjectileInfo.damageType.HasValue)
                defaultDamageType = fireProjectileInfo.damageType.Value;
            if (fireProjectileInfo.projectileDamageCoefficient.HasValue)
                defaultDamageCoefficient = fireProjectileInfo.projectileDamageCoefficient.Value;

            _owner = fireProjectileInfo.owner;
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer && renderer.material)
                renderer.material.color = _owner.Element.AsValidOrNull()?.elementColor ?? Color.white;
        }

        public void OnImpact(ProjectileImpactInfo impactInfo)
        {
            var collider = impactInfo.collider;
            var hurtBox = collider.GetComponent<HurtBox>();
            if (!hurtBox)
                return;
            var healthComponent = hurtBox.HealthComponent;
            if (!healthComponent)
                return;
            healthComponent.TakeDamage(new DamageInfo
            {
                attackerBody = _owner,
                damage = _owner.characterBody.Damage * defaultDamageCoefficient,
                damageType = defaultDamageType
            });
            Destroy(gameObject);
        }
    }
}