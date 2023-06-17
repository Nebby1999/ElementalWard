using ElementalWard.Buffs;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public class FireInteraction : IElementInteraction
    {
        public ElementDef TiedElement { get; set; }

        private DotBuffDef onFireDot;
        private bool _hasLoaded = false;
        public IEnumerator LoadAssets()
        {
            if (_hasLoaded)
                yield break;

            _hasLoaded = true;

            var handle = Addressables.LoadAssetAsync<DotBuffDef>("ElementalWard/Base/ElementDefs/Fire/OnFire.asset");
            while (!handle.IsDone)
                yield return new WaitForEndOfFrame();

            onFireDot = handle.Result;
        }
        public void OnDealDamage(DamageReport damageReport)
        {
            if(!damageReport.victimBody)
            {
                return;
            }

            var victimObject = damageReport.victimBody.gameObject;
            var controller = victimObject.GetComponent<BuffController>();
        }
    }
}