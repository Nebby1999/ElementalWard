using System;
using System.Linq;
using UnityEngine;

namespace ElementalWard
{
    public class HurtBoxGroup : MonoBehaviour, ILifeBehaviour
    {
        public HurtBox[] HurtBoxes => _hurtBoxes;
        [SerializeField] private HurtBox[] _hurtBoxes = Array.Empty<HurtBox>();
        public HurtBox MainHurtBox => _mainHurtBox;
        [SerializeField] private HurtBox _mainHurtBox;

        private void Awake()
        {
            TeamComponent component = GetComponentInParent<TeamComponent>();
            if (component)
                component.OnTeamChange += UpdateHurtboxes;
        }

        private void UpdateHurtboxes(TeamDef def)
        {
            var index = def ? def.TeamIndex : TeamIndex.None;
            for (int i = 0; i < _hurtBoxes.Length; i++)
            {
                _hurtBoxes[i].TeamIndex = index;
            }
        }

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
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnValidate()
        {
            if(_hurtBoxes.Length == 0)
            {
                Debug.LogWarning($"HurtBoxGroup {this} has no HurtBoxes", this);
                return;
            }

            if(!_hurtBoxes.Any(x => x.isBullseye))
            {
                Debug.LogWarning($"HurtBoxGroup {this} has no HurtBox marked as a Bullseye hurtbox!");
                return;
            }
        }
    }
}