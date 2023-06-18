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

!c n exeshortcut
# Executable Shortcut

You can create a shortcut that points to the ror2 executable that, when clicked, will open an instance that corresponds to a desired r2modman profile. this is especially useful in scenarios where you need two instances of the game open, such as when testing networking locally (for more information you can type the command ``!localnetwork``

## Steps

* Create a shortcut of your Risk of Rain 2 Executable. (https://cdn.discordapp.com/attachments/723014139060027462/1120088221146173513/image.png)

* Right click the shortcut, select properties. (https://cdn.discordapp.com/attachments/723014139060027462/1120088352306245632/image.png)

* Open your preferred r2modman profile, go into settings, and select debug, then click ``Set Launch Parameters`` (https://cdn.discordapp.com/attachments/723014139060027462/1120088831912329296/image.png)

* Copy the "Modded" parameters (https://cdn.discordapp.com/attachments/723014139060027462/1120089373992554587/image.png)

* Back on the shortcut's properties, append the launch parameters into the target field. (https://cdn.discordapp.com/attachments/723014139060027462/1120089573775659181/image.png)

* double clicking the shortcut will now open the chosen profile
