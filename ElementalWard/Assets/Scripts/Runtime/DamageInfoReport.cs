using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    public readonly struct BodyInfo
    {
        public readonly GameObject gameObject;
        public readonly CharacterBody characterBody;
        public readonly ElementDef element;

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
        public float damage;
        public BodyInfo attackerBody;
    }

    public class DamageReport
    {
        public float damage;
        public BodyInfo attackerBody;
        public BodyInfo victimBody;
    }
}