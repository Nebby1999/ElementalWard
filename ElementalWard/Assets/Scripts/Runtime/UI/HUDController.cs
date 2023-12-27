using Nebula;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalWard.UI
{
    public class HUDController : MonoBehaviour
    {
        public Animator Animator => _weaponAnimator;
        [SerializeField]
        private Animator _weaponAnimator;

        public Slider HPBar => _hpBar;
        [SerializeField] private Slider _hpBar;

        public CharacterBody TiedBody => _body;
        private CharacterBody _body;
        private WeaponController _weaponController;
        private HealthComponent _healthComponent;

        private HUDBehaviour[] _hudBehaviours = Array.Empty<HUDBehaviour>();
        private void Awake()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned += SetBody;
            _hudBehaviours = GetComponentsInChildren<HUDBehaviour>();
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        public void SetBody(CharacterBody body)
        {
            if(_weaponController)
            {
                _weaponController.OnWeaponUpdated -= SetAnimationController;
            }
            _body = body;
            _weaponController = body.GetComponent<WeaponController>();
            _healthComponent = body.HealthComponent;
            if(_weaponController)
            {
                _weaponController.OnWeaponUpdated += SetAnimationController;
                SetAnimationController();
            }
            foreach(HUDBehaviour behaviour in _hudBehaviours)
            {
                behaviour.OnBodyAssigned();
            }
        }

        private void LateUpdate()
        {
            if(!_healthComponent || !_hpBar)
            {
                return;
            }

            _hpBar.maxValue = _healthComponent.HealthProvider.MaxHealth;
            _hpBar.minValue = 0;
            _hpBar.value = _healthComponent.CurrentHealth;
        }

        private void SetAnimationController()
        {
            _weaponAnimator.runtimeAnimatorController = _weaponController.CurrentWeapon ? _weaponController.CurrentWeapon.controller : null;
        }

        private void OnDestroy()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned -= SetBody;
            
        }

        public static HUDController FindController(CharacterBody body)
        {
            foreach(var hud in InstanceTracker.GetInstances<HUDController>())
            {
                if(hud._body == body)
                {
                    return hud;
                }
            }
            return null;
        }
    }
}