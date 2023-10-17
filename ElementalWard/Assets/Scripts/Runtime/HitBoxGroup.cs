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

        [ContextMenu("Autopopulate array")]
        private void AutoPopulateArray()
        {
            hitboxes = GetComponentsInChildren<HitBox>();
        }

        public static HitBoxGroup FindHitBoxGroup(GameObject obj, string groupName)
        {
            var hitboxGroups = obj.GetComponents<HitBoxGroup>();
            for(int i = 0; i < hitboxGroups.Length; i++)
            {
                if (hitboxGroups[i].groupName.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                    return hitboxGroups[i];
            }
            return null;
        }
    }
}
