using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates
{
    public class GenericCharacterMain : EntityState
    {
        private Vector3 aimVector;
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
            MovementController.yRotation = BodyInputBank.yRotation;
            MovementController.MovementInput = BodyInputBank.moveVector;
        }
    }
}