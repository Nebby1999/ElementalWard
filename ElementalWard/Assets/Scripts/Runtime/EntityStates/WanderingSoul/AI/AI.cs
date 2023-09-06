using ElementalWard;
using UnityEngine;

namespace EntityStates.WanderingSoul.AI
{
    public class AI : BaseAIState
    {
        private Vector3 aimDir;
        private bool skill1Press;
        public override void Update()
        {
            base.Update();
            var target = GetTarget();
            if (!target.IsValid)
                return;

            if (target.HasLOS(CharacterBody, out aimDir, out _))
            {
                skill1Press = true;
            }
            else
            {
                skill1Press = false;
                aimDir = ICharacterMovementController != null ? ICharacterMovementController.Motor.CharacterForward : BodyTransform.forward;
            }
        }

        public override CharacterMasterAI.AIInputs GenerateInputs()
        {
            return new CharacterMasterAI.AIInputs
            {
                skill1Pressed = skill1Press,
                aimDir = aimDir,
            };
        }
    }
}