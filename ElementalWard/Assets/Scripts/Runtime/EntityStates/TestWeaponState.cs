using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UImGui;

namespace ElementalWard
{
    public class TestWeaponState : BaseCharacterState
    {
        public static ElementDef element;
        public bool useFire;
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void Update()
        {
            ElementChange();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(CharacterInputBank && CharacterInputBank.skill1Button.down)
            {
                var fireState = new TestWeaponStateFire();

                if(useFire)
                    fireState.elementDef = element;

                outer.SetNextState(fireState);
                //Go to fire state and set elementDef;
            }
        }

        public void ElementChange()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                useFire = !useFire;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
