using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElementalWard;
using KinematicCharacterController;

namespace EntityStates
{
    public class EntityState : EntityStateBase
    {
        public new EntityStateMachine outer => base.outer as EntityStateMachine;
        public CharacterMovementController MovementController => outer.CommonComponents.characterMovementController;
        public CharacterBody Body => outer.CommonComponents.characterBody;
        public CharacterInputBank BodyInputBank => outer.CommonComponents.inputBank;
    }
}
