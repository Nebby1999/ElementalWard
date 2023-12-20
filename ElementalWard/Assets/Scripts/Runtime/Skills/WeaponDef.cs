using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New WeaponDef", menuName = ElementalWardApplication.APP_NAME + "/Skills/WeaponDef", order = int.MinValue)]
    public class WeaponDef : NebulaScriptableObject
    {
        public SkillDef primarySkill;
        public SkillDef secondarySkill;
        public RuntimeAnimatorController controller;
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
                manager.Primary.SkillDef = primarySkill;
            }

            if(!manager.Secondary)
            {
                Debug.LogWarning($"Skill manager for object {manager.gameObject} doesnt have a generic secondary skill", manager.gameObject);
            }
            else
            {
                manager.Secondary.SkillDef = secondarySkill;
            }
        }
    }
}