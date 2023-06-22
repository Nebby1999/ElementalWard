﻿using System;
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
        /// <summary>
        /// If provided, the BodyInfo will use this element, instead of the one provided by an IElementProvider
        /// </summary>
        public ElementDef elementOverride;
        public ElementDef Element => elementOverride ? elementOverride : _elementProvider.Element;
        private readonly IElementProvider _elementProvider;
        public T GetComponent<T>() => gameObject ? gameObject.GetComponent<T>() : default(T);

        public static implicit operator bool(BodyInfo info) => info.gameObject;
        public BodyInfo(CharacterBody characterBody)
        {
            gameObject = characterBody.gameObject;
            this.characterBody = characterBody;
            elementOverride = null;

            _elementProvider = characterBody.GetComponent<IElementProvider>();
        }

        public BodyInfo(GameObject bodyGameObject)
        {
            gameObject = bodyGameObject;
            this.characterBody = bodyGameObject.GetComponent<CharacterBody>();
            elementOverride = null;

            _elementProvider = bodyGameObject.GetComponent<IElementProvider>();
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