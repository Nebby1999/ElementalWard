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
            if (!_onFireVFXInstance && _onFireVFX)
            {
                var data = new VFXData
                {
                    instantiationPosition = _victimTransform.position,
                    instantiationRotation = _victimTransform.rotation,
                };
                data.AddProperty(CommonVFXProperties.Scale, _victimCharacterBody ? _victimCharacterBody.Radius : 1);
                _onFireVFXInstance = FXManager.SpawnVisualFX(_onFireVFX, data);
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