using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New WeaponDef", menuName = ElementalWardApplication.APP_NAME + "/Skills/WeaponDef", order = int.MinValue)]
    public class WeaponDef : NebulaScriptableObject
    {
        [SerializeField]
        public SkillDef _primarySkill;
        [SerializeField]
        public SkillDef _secondarySkill;
        public void AssignWeapons(SkillManager manager)
        {
            if (!manager)
                return;

            if(!manager.Primary)
            {
                Debug.LogWarning($"Skill manager for object {manager.gameObject} doesnt have a generic primary skill", manager.gameObject);
            }
            else
            {
                manager.Primary.SkillDef = _primarySkill;
            }

            if(!manager.Secondary)
            {
                Debug.LogWarning($"Skill manager for object {manager.gameObject} doesnt have a generic secondary skill", manager.gameObject);
            }
            else
            {
                manager.Secondary.SkillDef = _secondarySkill;
            }
        }
    }
}