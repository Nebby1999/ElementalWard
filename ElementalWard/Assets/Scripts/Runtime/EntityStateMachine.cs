using EntityStates;
using KinematicCharacterController;
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
            public readonly CharacterMotorController characterController;
            public readonly KinematicCharacterMotor characterMotor;
            public readonly CharacterBody characterBody;
            public readonly CharacterInputBank inputBank;
            public readonly SkillManager skillManager;
            public readonly Rigidbody rigidBody;
            public readonly TeamComponent teamComponent;
            public readonly HealthComponent healthComponent;
            public readonly SpriteLocator spriteLocator;
            public readonly IElementProvider elementProvider;
            public readonly ChildLocator childLocator;
            public CommonComponentLocator(GameObject go)
            {
                gameObject = go;
                transform = go.transform;
                characterController = go.GetComponent<CharacterMotorController>();
                characterMotor = go.GetComponent<KinematicCharacterMotor>();
                characterBody = go.GetComponent<CharacterBody>();
                inputBank = go.GetComponent<CharacterInputBank>();
                skillManager = go.GetComponent<SkillManager>();
                rigidBody = go.GetComponent<Rigidbody>();
                teamComponent = go.GetComponent<TeamComponent>();
                healthComponent = go.GetComponent<HealthComponent>();
                spriteLocator = go.GetComponent<SpriteLocator>();
                elementProvider = go.GetComponent<IElementProvider>();
                childLocator = go.GetComponent<ChildLocator>();
            }
        }

        public CommonComponentLocator CommonComponents { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            CommonComponents = new CommonComponentLocator(gameObject);
        }

        public bool CanInterruptState(InterruptPriority priority)
        {
            EntityState state = (EntityState)(NewState ?? CurrentState);
            return state.GetMinimumInterruptPriority() <= priority;
        }
    }
}
