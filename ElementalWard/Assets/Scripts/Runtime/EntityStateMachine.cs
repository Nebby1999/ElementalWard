using Nebula;
using UnityEngine;

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
            public readonly Rigidbody rigidBody;
            public readonly TeamComponent teamComponent;
            public readonly HealthComponent healthComponent;
            public readonly SpriteLocator spriteLocator;
            public CommonComponentLocator(GameObject go)
            {
                gameObject = go;
                transform = go.transform;
                characterMovementController = go.GetComponent<ICharacterMovementController>();
                characterBody = go.GetComponent<CharacterBody>();
                inputBank = go.GetComponent<CharacterInputBank>();
                skillManager = go.GetComponent<BodySkillManager>();
                rigidBody = go.GetComponent<Rigidbody>();
                teamComponent = go.GetComponent<TeamComponent>();
                healthComponent = go.GetComponent<HealthComponent>();
                spriteLocator = go.GetComponent<SpriteLocator>();
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
