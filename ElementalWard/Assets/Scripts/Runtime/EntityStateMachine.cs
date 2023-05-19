using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using Nebula;

namespace ElementalWard
{
    public class EntityStateMachine : EntityStateMachineBase
    {
        public class CommonComponentLocator
        {
            public readonly CharacterBody characterBody;
            public readonly InputBank inputBank;
            public CommonComponentLocator(GameObject gameObject)
            {
                characterBody = gameObject.GetComponent<CharacterBody>();
                inputBank = gameObject.GetComponent<InputBank>();
            }
        }

        public CommonComponentLocator CommonComponents { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            CommonComponents = new CommonComponentLocator(gameObject);
        }
    }
}
