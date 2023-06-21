using Nebula;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(BuffController), typeof(HealthComponent))]
    public class EnemyElementProvider : MonoBehaviour, IElementProvider, IOnTakeDamage, IOnIncomingDamage
    {
        public HealthComponent HealthComponent { get; private set; }
        public BuffController BuffController { get; private set; }
        public ElementDef Element { get => _element; set => _element = value; }
        [SerializeField] private ElementDef _element;
        [SerializeField] private BuffDef _buffDef;
        [SerializeField] private GameObject _overloadEffect;
        public bool canBeOverLoaded;
        public int amountRequiredForDeath;

        private int _currentCount;
        private CharacterBody _body;

        private GameObject _overloadEffectInstance;
        private ParticleSystemForceField _effectForceField;
        private float _currentForceFieldGravityStrength;
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

            if (report.attackerBody.element != Element)
                return;

            _currentCount = BuffController.GetBuffCount(_buffDef.BuffIndex);
            BuffController.AddTimedBuff(_buffDef.BuffIndex, 3, 1, amountRequiredForDeath);
            if(!_overloadEffectInstance)
            {
                _overloadEffectInstance = FXManager.SpawnVisualFX(_overloadEffect, new VFXData
                {
                    vfxColor = _element.elementColor,
                    scale = _body.AsValidOrNull()?.Radius ?? 1,
                    instantiationPosition = transform.position,
                    instantiationRotation = transform.rotation,
                });
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