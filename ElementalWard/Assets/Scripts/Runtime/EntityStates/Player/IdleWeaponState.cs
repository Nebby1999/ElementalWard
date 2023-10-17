using ElementalWard;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace EntityStates.Player
{
    public class IdleWeaponState : BaseCharacterState
    {
        public WeaponController WeaponController { get; private set; }

        private bool4 _weaponSwitch;
        public override void OnEnter()
        {
            base.OnEnter();
            WeaponController = GetComponent<WeaponController>();
        }

        public override void Update()
        {
            if(!CharacterInputBank)
            {
                return;
            }

            _weaponSwitch = new bool4
            {
                x = CharacterInputBank.weaponSlot1.down,
                y = CharacterInputBank.weaponSlot2.down,
                z = CharacterInputBank.weaponSlot3.down,
                w = CharacterInputBank.weaponSlot4.down,
            };
        }

        public override void FixedUpdate()
        {
            if (!math.any(_weaponSwitch))
                return;

            for(int i = 0; i < 4; i++)
            {
                if (_weaponSwitch[i])
                {
                    WeaponController.SwitchToWeapon(i);
                    return;
                }
            }
        }
    }
}
