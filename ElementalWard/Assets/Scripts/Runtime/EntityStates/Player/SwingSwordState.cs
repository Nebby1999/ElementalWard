using ElementalWard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Player
{
    public class SwingSwordState : BaseCharacterState
    {
        public static float damageCoefficient;

        private HitBoxAttack attack;

        public override void OnEnter()
        {
            base.OnEnter();
        }
    }
}
