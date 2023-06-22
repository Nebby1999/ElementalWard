using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElementalWard;
using KinematicCharacterController;
using UnityEngine;

namespace EntityStates
{
    public class EntityState : EntityStateBase
    {
        public new EntityStateMachine outer => base.outer as EntityStateMachine;
        public CharacterMovementController CharacterMovementController => outer.CommonComponents.characterMovementController;
        public CharacterBody CharacterBody => outer.CommonComponents.characterBody;
        public CharacterInputBank CharacterInputBank => outer.CommonComponents.inputBank;
        public Transform Transform => outer.CommonComponents.transform;
        public GameObject GameObject => outer.CommonComponents.gameObject;
    }
}
