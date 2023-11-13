using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class ElectricElementInteractions : IElementInteraction
    {
        public ElementDef SelfElement { get; set; }
        private SphereSearch _sphereSearch = new SphereSearch();
        public IEnumerator LoadAssetsAsync()
        {
            yield break;
        }

        public void ModifyIncomingDamage(DamageInfo damageInfo, GameObject self)
        {
            if (damageInfo == null || !damageInfo.attackerBody.IsValid() || !damageInfo.attackerBody.ElementDef)
            {
                return;
            }

            var attackerElement = damageInfo.attackerBody.ElementDef;
            if (attackerElement == SelfElement)
                return;

            if (attackerElement == StaticElementReferences.FireDef)
            {
                damageInfo.damage *= 1.25f;
                return;
            }
            if (attackerElement == StaticElementReferences.WaterDef)
            {
                damageInfo.damage *= 0.75f;
            }
        }

        public void ModifyStatArguments(StatModifierArgs args, CharacterBody body)
        {
            args.movementSpeedMultAdd += 0.25f;
            args.attackSpeedMultAdd += 0.25f;
        }

        public void OnElementalDamageDealt(DamageReport damageReport)
        {
            var victim = damageReport.victimBody;
            if (!victim)
                return;

            var victimElement = damageReport.victimBody.ElementDef;
            if (victimElement == SelfElement)
                return;

            if(!victim.TryGetComponent<BuffController>(out var bd))
            {
                return;
            }

            if(bd.HasBuff(StaticElementReferences.Charged) && !damageReport.procMask.HasProc(StaticElementReferences.ChargedJumpProc))
            {
                if (TryElectricJump(damageReport))
                {
                    bd.RemoveBuff(StaticElementReferences.Charged);
                }
            }
            else if(Util.ProcCheckRoll(100, damageReport.procCoefficient))
            {
                bd.AddBuff(StaticElementReferences.Charged);
            }
        }

        private bool TryElectricJump(DamageReport damageReport)
        {
            var victim = damageReport.victimBody.characterBody;
            var radius = victim ? victim.Radius : 1;
            radius *= 10;

            TeamMask mask = new TeamMask();
            mask.AddTeam(damageReport.victimBody.team);
            _sphereSearch.radius = radius;
            _sphereSearch.origin = victim.transform.position;
            _sphereSearch.FindCandidates()
                .FilterCandidatesByTeam(mask)
                .FilterCandidatesByDistinctHealthComponent()
                .FilterCandidatesByLOS(LayerIndex.world.Mask)
                .FilterBy(c =>
                {
                    var healthComponent = c.hurtBox.HealthComponent;
                    return healthComponent.gameObject != victim.gameObject && healthComponent.TryGetComponent<BuffController>(out var bd) && bd.HasBuff(StaticElementReferences.Charged);
                })
                .OrderByDistance()
                .FirstOrDefault(out var candidate);

            var healthComponent = candidate.hurtBox ? candidate.hurtBox.HealthComponent : null;
            if (!healthComponent)
                return false;

            var procMask = new ProcMask(damageReport.procMask);
            procMask.AddProc(StaticElementReferences.ChargedJumpProc);
            healthComponent.TakeDamage(new DamageInfo
            {
                attackerBody = damageReport.attackerBody,
                damage = damageReport.damage * 0.1f,
                procCoefficient = 0,
                procMask = procMask
            });
            healthComponent.GetComponent<BuffController>().RemoveBuff(StaticElementReferences.Charged);
            return true;
        }

        public ElectricElementInteractions()
        {
            _sphereSearch = new SphereSearch
            {
                candidateMask = LayerIndex.entityPrecise.Mask,
                triggerInteraction = QueryTriggerInteraction.Ignore,
            };
        }
    }
}