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
        public readonly struct CommonComponentLocator
        {
            public readonly GameObject gameObject;
            public readonly Transform transform;
            public readonly ICharacterMovementController characterMovementController;
            public readonly CharacterBody characterBody;
            public readonly CharacterInputBank inputBank;
            public readonly BodySkillManager skillManager;
            public CommonComponentLocator(GameObject go)
            {
                gameObject = go;
                transform = go.transform;
                characterMovementController = go.GetComponent<ICharacterMovementController>();
                characterBody = go.GetComponent<CharacterBody>();
                inputBank = go.GetComponent<CharacterInputBank>();
                skillManager = go.GetComponent<BodySkillManager>();
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
