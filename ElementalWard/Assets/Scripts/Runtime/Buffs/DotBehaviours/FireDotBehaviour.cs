using Nebula;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ElementalWard
{
    public class FireDotBehaviour : DotBehaviour
    {
        private static GameObject _onFireVFX;
        private static AsyncOperationHandle handle;
        private GameObject _onFireVFXInstance;
        private Transform _victimTransform;
        private HealthComponent _victimHealthComponent;
        private float _damagePerTick;
        private float _stopWatch;
        public override IEnumerator LoadAssetsOnInitialization()
        {
            handle = Addressables.LoadAssetAsync<GameObject>("ElementalWard/Base/ElementDefs/Fire/OnFireVFX.prefab");
            while (!handle.IsDone)
                yield return new WaitForEndOfFrame();
            _onFireVFX = (GameObject)handle.Result;
            yield break;
        }

        public override void OnInflicted(DotInflictInfo dotInfo)
        {
            base.OnInflicted(dotInfo);
            _victimTransform = dotInfo.victim.gameObject.transform;
            _victimHealthComponent = dotInfo.victim.GetComponent<HealthComponent>();
            var inflictorBody = dotInfo.inflictor.characterBody;
            var totalDamage = (inflictorBody ? inflictorBody.Damage : dotInfo.customDamageSource) * TiedDotDef.damageCoefficient;
            _damagePerTick = (totalDamage * DotStacks) / (TiedDotDef.secondsPerTick * dotInfo.fixedAgeDuration);
            if (!_onFireVFXInstance && _onFireVFX)
            {
                var data = new VFXData
                {
                    instantiationPosition = _victimTransform.position,
                    instantiationRotation = _victimTransform.rotation,
                };
                data.AddProperty(CommonVFXProperties.Scale, inflictorBody ? inflictorBody.Radius : 1);
                _onFireVFXInstance = FXManager.SpawnVisualFX(_onFireVFX, data);
            }
        }

        public override void OnFixedUpdate(float fixedDeltaTime)
        {
            base.OnFixedUpdate(fixedDeltaTime);
            _stopWatch += fixedDeltaTime;
            if (_stopWatch > TiedDotDef.secondsPerTick)
            {
                _stopWatch = 0;
                _victimHealthComponent.AsValidOrNull()?.TakeDamage(new DamageInfo
                {
                    damageType = DamageType.DOT,
                    attackerBody = Info.inflictor,
                    damage = _damagePerTick,
                });
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            if (_onFireVFXInstance)
            {
                _onFireVFXInstance.transform.SetPositionAndRotation(_victimTransform.position, _victimTransform.rotation);
            }
        }

        public override void OnRemoved(DotInflictInfo dotInfo)
        {
            base.OnRemoved(dotInfo);
            Destroy(_onFireVFXInstance);
        }
    }
}