using ElementalWard;
using UnityEngine;

namespace EntityStates.WanderingSoul.AI
{
    public class AI : BaseAIState
    {
        private Vector3 aimDir;
        private bool primaryPressed;
        public override void Update()
        {
            base.Update();
            var target = GetTarget();
            if (!target.IsValid)
                return;

            if (target.HasLOS(CharacterBody, out aimDir, out _))
            {
                primaryPressed = true;
            }
            else
            {
                primaryPressed = false;
                aimDir = ICharacterMovementController != null ? ICharacterMovementController.Motor.CharacterForward : BodyTransform.forward;
            }
        }

        public override CharacterMasterAI.AIInputs GenerateInputs()
        {
            return new CharacterMasterAI.AIInputs
            {
                primaryPressed = primaryPressed,
                aimDir = aimDir,
            };
        }
    }
}