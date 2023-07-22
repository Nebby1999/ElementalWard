using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    [RequireComponent(typeof(BuffController), typeof(HealthComponent))]
    public class EnemyElementProvider : MonoBehaviour, IElementProvider, IOnTakeDamage, IOnIncomingDamage
    {
        private static BuffDef _buffDef;
        private static GameObject _overloadEffect;
        public HealthComponent HealthComponent { get; private set; }
        public BuffController BuffController { get; private set; }
        public ElementDef Element { get => _element; set => _element = value; }
        [SerializeField] private ElementDef _element;
        public bool canBeOverLoaded;
        public int amountRequiredForDeath;

        private int _currentCount;
        private CharacterBody _body;

        private GameObject _overloadEffectInstance;
        private ParticleSystemForceField _effectForceField;
        private float _currentForceFieldGravityStrength;

        private static void SystemInitializer()
        {
            BuffCatalog.resourceAvailability.CallWhenAvailable(() =>
            {
                _buffDef = BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("bdOverload"));
                _overloadEffect = Addressables.LoadAssetAsync<GameObject>("ElementalWard/Base/ElementDefs/ElementOverload.prefab").WaitForCompletion();
            });
        }
        private void Awake()
        {
            BuffController = GetComponent<BuffController>();
            HealthComponent = GetComponent<HealthComponent>();
            _body = GetComponent<CharacterBody>();
        }

        public void OnIncomingDamage(DamageInfo dinfo)
        {
            if (_currentCount >= amountRequiredForDeath)
            {
                dinfo.damage = HealthComponent.CurrentHealth * 10;
                dinfo.damageType |= DamageType.InstaKill;
            }
        }

        public void OnTakeDamage(DamageReport report)
        {
            if (report.damageType.HasFlag(DamageType.DOT))
                return;

            if (!canBeOverLoaded || !_element)
                return;

            if (report.attackerBody.Element != Element)
                return;

            _currentCount = BuffController.GetBuffCount(_buffDef.BuffIndex);
            BuffController.AddTimedBuff(_buffDef.BuffIndex, 3, 1, amountRequiredForDeath);
            if(!_overloadEffectInstance)
            {
                var data = new VFXData
                {
                    instantiationPosition = transform.position,
                    instantiationRotation = transform.rotation
                };
                data.AddProperty(CommonVFXProperties.Color, _element.elementColor);
                data.AddProperty(CommonVFXProperties.Scale, _body.AsValidOrNull()?.Radius ?? 1);
                _overloadEffectInstance = FXManager.SpawnVisualFX(_overloadEffect, data);
                _overloadEffectInstance.transform.SetParent(transform);
                _effectForceField = GetComponentInChildren<ParticleSystemForceField>();
            }
        }

        private void Update()
        {
            if (!_effectForceField)
                return;
            _currentCount = BuffController.GetBuffCount(_buffDef.BuffIndex);
            if(_currentCount == 0 && _overloadEffectInstance)
            {
                var disableParticleLooping = _overloadEffectInstance.GetComponentInChildren<DisableParticleLooping>();
                disableParticleLooping.DisableLooping();
                _overloadEffectInstance.transform.DetachChildren();
                Destroy(_overloadEffectInstance);
            }
            var gravity = _effectForceField.gravity;
            var constant = Mathf.Max(1, _currentCount);
            gravity.constant = Mathf.SmoothDamp(gravity.constant, constant, ref _currentForceFieldGravityStrength, 0.5f);
            _effectForceField.gravity = gravity;
        }

        private void OnDestroy()
        {
            if (_overloadEffectInstance)
                Destroy(_overloadEffectInstance);
        }
    }
}