using Nebula;
using System.Collections;

namespace ElementalWard
{
    public class FireDotBehaviour : DotBehaviour
    {
        private HealthComponent victimHealthComponent;
        private float damagePerTick;
        private float stopWatch;
        public override IEnumerator LoadAssetsOnInitialization()
        {
            yield break;
        }

        public override void OnInflicted(DotInflictInfo dotInfo)
        {
            base.OnInflicted(dotInfo);
            victimHealthComponent = dotInfo.victim.GetComponent<HealthComponent>();
            var inflictorBody = dotInfo.inflictor.characterBody;
            var totalDamage = (inflictorBody ? inflictorBody.Damage : dotInfo.customDamageSource) * dotInfo.damageCoefficient;
            damagePerTick = totalDamage / (TiedDotDef.secondsPerTick * dotInfo.fixedAgeDuration);
        }

        public override void OnFixedUpdate(float fixedDeltaTime)
        {
            base.OnFixedUpdate(fixedDeltaTime);
            stopWatch += fixedDeltaTime;
            if(stopWatch > TiedDotDef.secondsPerTick)
            {
                stopWatch = 0;
                victimHealthComponent.AsValidOrNull()?.TakeDamage(new DamageInfo
                {
                    damageType = DamageType.DOT,
                    attackerBody = Info.inflictor,
                    damage = damagePerTick,
                });
            }
        }
    }
}