using UnityEngine;
using UnityEngine.UI;

namespace ElementalWard.UI
{
    public class SkillIcon : HUDBehaviour
    {
        [SerializeField]
        private Image _skillIcon;
        [SerializeField]
        private Image _cooldownOverlay;
        [SerializeField]
        private SkillSlot _skillSlot;

        GenericSkill _skill;
        public override void OnBodyAssigned()
        {
            _skill = HUD.TiedBody.GetComponent<SkillManager>().GetSkillBySkillSlot(_skillSlot);

            if (!_skill)
                gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            var skillDef = _skill.SkillDef;
            gameObject.SetActive(skillDef);
            if (!skillDef)
                return;

            _skillIcon.sprite = skillDef.icon;
            _skillIcon.enabled = _skillIcon.sprite;

            if(!(skillDef.baseCooldown > 0))
            {
                _cooldownOverlay.fillAmount = 0;
                return;
            }

            float remap = Nebula.NebulaMath.Remap(_skill.CooldownTimer, 0, skillDef.baseCooldown, 0, 1);
            _cooldownOverlay.fillAmount = remap;
        }
    }
}