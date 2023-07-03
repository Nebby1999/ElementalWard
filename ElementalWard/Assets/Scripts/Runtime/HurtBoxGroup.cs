using System;
using UnityEngine;

namespace ElementalWard
{
    public class HurtBoxGroup : MonoBehaviour, ILifeBehaviour
    {
        public HurtBox[] HurtBoxes => _hurtBoxes;
        [SerializeField] private HurtBox[] _hurtBoxes = Array.Empty<HurtBox>();
        public HurtBox MainHurtBox => _mainHurtBox;
        [SerializeField] private HurtBox _mainHurtBox;
        public void OnDeathStart(DamageReport killingDamageInfo)
        {
            SetActiveHurtboxes(false);
        }

        public void SetActiveHurtboxes(bool active)
        {
            for (int i = 0; i < _hurtBoxes.Length; i++)
            {
                _hurtBoxes[i].gameObject.SetActive(active);
            }
        }

        [ContextMenu("Autopopulate array")]
        private void AutoPopulateArray()
        {
            _hurtBoxes = GetComponentsInChildren<HurtBox>();
        }
    }
}