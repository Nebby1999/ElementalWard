using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UImGui;

namespace ElementalWard
{
    public class TestWeaponState : BaseCharacterState
    {
        public ElementIndex elementIndex = ElementIndex.None;
        public ElementDef elementToFire;
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            ElementChange();

            if(CharacterInputBank && CharacterInputBank.fireButton.IsPressed)
            {
                var fireState = new TestWeaponStateFire();
                fireState.elementDef = elementToFire;
                outer.SetNextState(fireState);
                //Go to fire state and set elementDef;
            }
        }

        public void ElementChange()
        {
            if (CharacterInputBank.elementAxis == 0)
                return;

            if (CharacterInputBank.elementAxis > 0)
            {
                ElementIndex newIndex = elementIndex + 1;
                ElementDef newDef = ElementCatalog.GetElementDef(newIndex);
                if (newDef)
                {
                    elementIndex = newIndex;
                    elementToFire = newDef;
                }
                else
                {
                    elementIndex = ElementIndex.None;
                    elementToFire = null;
                }
            }
            else if (CharacterInputBank.elementAxis < 0)
            {
                ElementIndex newIndex = elementIndex - 1;
                ElementDef newDef = ElementCatalog.GetElementDef(newIndex);
                if (newDef)
                {
                    elementIndex = newIndex;
                    elementToFire = newDef;
                }
                else
                {
                    elementIndex = ElementIndex.None;
                    elementToFire = null;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
