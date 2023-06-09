using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using KinematicCharacterController;
using Nebula;

namespace ElementalWard
{
    public class EntityStateMachine : EntityStateMachineBase
    {
        public struct CommonComponentLocator
        {
            public readonly Transform transform;
            public readonly CharacterMovementController characterMovementController;
            public readonly CharacterBody characterBody;
            public readonly CharacterInputBank inputBank;
            public CommonComponentLocator(GameObject gameObject)
            {
                transform = gameObject.transform;
                characterMovementController = gameObject.GetComponent<CharacterMovementController>();
                characterBody = gameObject.GetComponent<CharacterBody>();
                inputBank = gameObject.GetComponent<CharacterInputBank>();
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
