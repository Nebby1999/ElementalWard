using ElementalWard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates
{
    public class GenericSpawnState : EntityState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("base", "Spawn");

            var events = GetAnimationEvents();
            events.OnAnimationEvent += OnSpawnEnd;
        }

        private void OnSpawnEnd(int obj)
        {
            if(obj == CharacterAnimationEvents.spawnEndHash)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
