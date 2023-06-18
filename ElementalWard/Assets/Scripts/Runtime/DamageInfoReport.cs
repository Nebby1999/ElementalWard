using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    public struct BodyInfo
    {
        public GameObject gameObject;
        public CharacterBody characterBody;
        public ElementDef element;

        public T GetComponent<T>() => gameObject ? gameObject.GetComponent<T>() : default(T);

        public static implicit operator bool(BodyInfo info) => info.gameObject;
        public BodyInfo(CharacterBody characterBody)
        {
            gameObject = characterBody.gameObject;
            this.characterBody = characterBody;

            var provider = characterBody.GetComponent<IElementProvider>();
            element = provider?.Element;
        }

        public BodyInfo(GameObject bodyGameObject)
        {
            gameObject = bodyGameObject;
            this.characterBody = bodyGameObject.GetComponent<CharacterBody>();

            var provider = bodyGameObject.GetComponent<IElementProvider>();
            element = provider?.Element;
        }
    }
    public class DamageInfo
    {
        public DamageType damageType = DamageType.None;
        public float damage;
        public bool rejected = false;
        public BodyInfo attackerBody;
    }

    public class DamageReport
    {
        public DamageType damageType = DamageType.None;
        public float damage;
        public BodyInfo attackerBody;
        public BodyInfo victimBody;
        public DamageInfo damageInfo;
    }
}