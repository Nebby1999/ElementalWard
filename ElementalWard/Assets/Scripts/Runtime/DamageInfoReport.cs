﻿using UnityEngine;

namespace ElementalWard
{
    public struct BodyInfo
    {
        public GameObject gameObject;
        public CharacterBody characterBody;
        public CharacterMaster characterMaster;
        public ElementDef ElementDef => _elementProvider?.ElementDef ?? fallbackElement;
        public ElementDef fallbackElement;
        public TeamIndex team;

        public void NullElementProvider()
        {
            _elementProvider = null;
        }

        public bool TryGetComponent<T>(out T component)
        {
            if (gameObject)
                return gameObject.TryGetComponent<T>(out component);
            component = default;
            return false;
        }

        public bool TryGetComponents<T>(out T[] components)
        {
            if(gameObject)
            {
                components = gameObject.GetComponents<T>();
                return components.Length > 0;
            }
            components = null;
            return false;
        }

        public bool IsValid()
        {
            return gameObject || ElementDef;
        }

        private IElementProvider _elementProvider;

        public static implicit operator bool(BodyInfo info) => info.IsValid();
        public BodyInfo(CharacterBody characterBody)
        {
            gameObject = characterBody.gameObject;
            this.characterBody = characterBody;
            team = TeamComponent.GetObjectTeamIndex(gameObject);
            characterMaster = characterBody.TiedMaster;

            _elementProvider = characterBody.GetComponent<IElementProvider>();
            fallbackElement = null;
        }

        public BodyInfo(GameObject bodyGameObject)
        {
            gameObject = bodyGameObject;
            this.characterBody = bodyGameObject.GetComponent<CharacterBody>();
            team = TeamComponent.GetObjectTeamIndex(gameObject);
            characterMaster = characterBody ? characterBody.TiedMaster : null;

            _elementProvider = bodyGameObject.GetComponent<IElementProvider>();
            fallbackElement = null;
        }
    }
    public class DamageInfo
    {
        public BodyInfo attackerBody;
        public DamageType damageType = DamageType.None;
        public ProcMask procMask;
        public float damage;
        public float procCoefficient;

        public bool rejected = false;

        public static float GetDamageModifier(HurtBox hurtBox)
        {
            return hurtBox ? hurtBox.damageMultiplier : 1;
        }
    }

    public class DamageReport
    {
        public DamageInfo damageInfo;
        public BodyInfo attackerBody;
        public BodyInfo victimBody;
        public DamageType damageType = DamageType.None;
        public ProcMask procMask;
        public float damage;
        public float procCoefficient;
    }
}