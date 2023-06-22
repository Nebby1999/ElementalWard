using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates
{
    public class BaseCharacterMain : BaseCharacterState
    {
        public bool HasCharacterMovementController { get; private set; }
        public bool HasCharacterInputBank { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
            HasCharacterMovementController = CharacterMovementController;
            HasCharacterInputBank = CharacterInputBank;
        }
    }
}
