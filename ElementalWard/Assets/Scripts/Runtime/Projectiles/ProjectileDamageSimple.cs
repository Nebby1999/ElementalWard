using Nebula;
using UnityEngine;

namespace ElementalWard.Projectiles
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileDamageSimple : MonoBehaviour, IProjectileImpact, IProjectileInitialization
    {
        public int maxImpacts = 1;
        public DamageType defaultDamageType;
        public float defaultDamageCoefficient;
        private BodyInfo _owner;
        private int impacts = 0;
        public void Initialize(FireProjectileInfo fireProjectileInfo)
        {
            defaultDamageType = fireProjectileInfo.damageType;
            fireProjectileInfo.TryGetProperty(CommonProjectileProperties.DamageCoefficient, out defaultDamageCoefficient);

            _owner = fireProjectileInfo.owner;
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer && renderer.material)
                renderer.material.color = _owner.Element.AsValidOrNull()?.elementColor ?? Color.white;
        }

        public void OnImpact(ProjectileImpactInfo impactInfo)
        {
            impacts++;
            TryDealDamage(impactInfo);
            if(impacts >= maxImpacts)
                Destroy(gameObject);
        }

        private void TryDealDamage(ProjectileImpactInfo impactInfo)
        {
            var collider = impactInfo.collider;
            var hurtBox = collider.GetComponent<HurtBox>();

            if (!hurtBox)
                return;

            bool? canOwnerHarmThisEntity = TeamCatalog.GetTeamInteraction(_owner.team, hurtBox.TeamIndex);
            if (canOwnerHarmThisEntity == false)
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