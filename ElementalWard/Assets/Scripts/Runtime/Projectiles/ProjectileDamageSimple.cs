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
            TryDealDamage(impactInfo);
            Destroy(gameObject);
        }

        private void TryDealDamage(ProjectileImpactInfo impactInfo)
        {
            var collider = impactInfo.collider;
            var hurtBox = collider.GetComponent<HurtBox>();

            if (!hurtBox)
                return;

            bool? canOwnerHarmThisEntity = TeamCatalog.GetTeamInteraction(_owner.team, hurtBox.TeamIndex);
            if (!canOwnerHarmThisEntity == false)
                return;

            var healthComponent = hurtBox.HealthComponent;
            if (!healthComponent)
                return;

            var damageInfo = new DamageInfo
            {
                attackerBody = _owner,
                damage = _owner.characterBody.Damage * defaultDamageCoefficient,
                damageType = defaultDamageType,
            };
            damageInfo.damage *= DamageInfo.GetDamageModifier(hurtBox);
            healthComponent.TakeDamage(damageInfo);
        }
    }
}