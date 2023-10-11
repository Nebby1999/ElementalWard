using Cinemachine;
using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class SkillManager : MonoBehaviour
    {
        public GenericSkill Primary => _primary;
        [SerializeField, ValueLabel("genericSkillName")] private GenericSkill _primary;

        public GenericSkill Secondary => _secondary;
        [SerializeField, ValueLabel("genericSkillName")] private GenericSkill _secondary;

        public GenericSkill Utility => _utility;
        [SerializeField, ValueLabel("genericSkillName")] private GenericSkill _utility;

        public GenericSkill Special => _special;
        [SerializeField, ValueLabel("genericSkillName")] private GenericSkill _special;

        private GenericSkill[] _allSkills = Array.Empty<GenericSkill>();

        private void Awake()
        {
            _allSkills = GetComponents<GenericSkill>();
        }

        public int GetSkillIndex(GenericSkill skill)
        {
            return Array.IndexOf(_allSkills, skill);
        }

        public GenericSkill GetSkillByIndex(int index)
        {
            return ArrayUtils.GetSafe(ref _allSkills, index);
        }

        public GenericSkill GetSkill(string name)
        {
            for(int i = 0; i < _allSkills.Length; i++)
            {
                var skill = _allSkills[i];
                if(skill.genericSkillName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return skill;
            }
            return null;
        }

    }
}
