using ElementalWard;
using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates
{
    public class BaseGroundedCharacterMain : BaseCharacterMain
    {
        public bool HasGroundedCharacterMovementController { get; private set; }
        public GroundedCharacterMovementController GroundedCharacterMovementController { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
            if(ICharacterMovementController is GroundedCharacterMovementController groundedController)
            {
                HasGroundedCharacterMovementController = groundedController;
                GroundedCharacterMovementController = groundedController;
            }
        }
    }
}
