using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates
{
    public class GenericCharacterMain : EntityState
    {
        public override void Update()
        {
            base.Update();
            if(BodyInputBank)
            {
                ProcessInputs();
            }
        }

        protected virtual void ProcessInputs()
        {
            MovementController.MovementVector = BodyInputBank.moveVector;
        }
    }
}