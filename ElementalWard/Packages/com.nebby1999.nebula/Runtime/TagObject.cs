using UnityEngine;
using System.Collections;

namespace Nebula
{
    [CreateAssetMenu(menuName = "Nebula/TagObject")]
    public class TagObject : NebulaScriptableObject
    {
        public override bool Equals(object other)
        {
            if(other is TagObject tagObj)
            {
                return this.cachedName.Equals(tagObj.cachedName, System.StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
