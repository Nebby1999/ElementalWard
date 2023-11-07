using UnityEngine;

namespace ElementalWard.Projectiles
{
    public class ProjectileExplosion : MonoBehaviour, IProjectileInitialization, IProjectileImpact
    {
        public bool explodeOnImpact;
        public bool destroyWithExplosion;
        public bool requireLOS;
        public bool hitSelf;
        public float explosionRadius;
        public float explosionDamageCoefficient;
        public float explosionProcCoefficient;
        public DamageType damageType;
        public ExplosiveAttack.DefaultFalloffCalculationMethod falloffCalculation;


        public ExplosiveAttack.FalloffCalculateDelegate falloffCalculationDelegate;
        private BodyInfo owner;

        private void Awake()
        {
            switch(falloffCalculation)
            {
                case ExplosiveAttack.DefaultFalloffCalculationMethod.Default:
                    falloffCalculationDelegate = ExplosiveAttack.DefaultFalloffCalculation;
                    break;
                case ExplosiveAttack.DefaultFalloffCalculationMethod.Linear:
                    falloffCalculationDelegate = ExplosiveAttack.LinearFalloffCalculation;
                    break;
                case ExplosiveAttack.DefaultFalloffCalculationMethod.Sweetspot:
                    falloffCalculationDelegate = ExplosiveAttack.SweetspotFalloffCalculation;
                    break;
            }
        }
        public void Explode()
        {
            ExplosiveAttack attack = new ExplosiveAttack()
            {
                attacker = owner,
                baseDamage = explosionDamageCoefficient,
                explosionOrigin = transform.position,
                explosionRadius = explosionRadius,
                requireLineOfSight = requireLOS,
                hitSelf = hitSelf,
                damageType = damageType
            };
            if(falloffCalculationDelegate != null)
            {
                attack.falloffCalculation = falloffCalculationDelegate;
            }
            attack.Fire();
            Destroy(gameObject);
        }

        public void Initialize(FireProjectileInfo fireProjectileInfo)
        {
            owner = fireProjectileInfo.owner;
            fireProjectileInfo.TryGetProperty(ProjectileProperties.DamageCoefficientOverride, out explosionDamageCoefficient, explosionDamageCoefficient);
        }

        public void OnImpact(ProjectileImpactInfo impactInfo)
        {
            if(explodeOnImpact)
            {
                Explode();
            }
        }
    }
}