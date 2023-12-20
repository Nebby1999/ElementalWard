using ElementalWard;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace EntityStates.Player.Weapon
{
    public class IdleWeaponState : BaseWeaponState
    {
        public WeaponController WeaponController { get; private set; }

        private bool3 _weaponSwitch;
        public override void OnEnter()
        {
            base.OnEnter();
            WeaponController = GetComponent<WeaponController>();

            PlayWeaponAnimation("Base", "Idle");
        }

        public override void Update()
        {
            if(!CharacterInputBank)
            {
                return;
            }

            _weaponSwitch = new bool3
            {
                x = CharacterInputBank.weaponSlot1.down,
                y = CharacterInputBank.weaponSlot2.down,
                z = CharacterInputBank.weaponSlot3.down,
            };
        }

        public override void FixedUpdate()
        {
            if (!math.any(_weaponSwitch))
                return;

            for(int i = 0; i < 3; i++)
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
