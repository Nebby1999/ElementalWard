using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    public class HitBoxGroup : MonoBehaviour
    {
        public string groupName;
        public HitBox[] hitboxes = Array.Empty<HitBox>();

        private CharacterInputBank _bank;
        [ContextMenu("Autopopulate array")]
        private void AutoPopulateArray()
        {
            hitboxes = GetComponentsInChildren<HitBox>();
        }

        private void Awake()
        {
            _bank = GetComponentInParent<CharacterInputBank>();
        }

        private void Update()
        {
            if(_bank)
                transform.rotation = _bank.LookRotation;
        }
        public static HitBoxGroup FindHitBoxGroup(GameObject obj, string groupName)
        {
            var hitboxGroups = obj.GetComponentsInChildren<HitBoxGroup>();
            for(int i = 0; i < hitboxGroups.Length; i++)
            {
                if (hitboxGroups[i].groupName.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                    return hitboxGroups[i];
            }
            return null;
        }
    }
}
