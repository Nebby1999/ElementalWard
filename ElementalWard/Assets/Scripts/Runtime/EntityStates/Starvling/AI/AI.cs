using ElementalWard;
using UnityEngine;

namespace EntityStates.Starvling.AI
{
    public class AI : BaseAIState
    {
        private Vector3 aimDir;
        private bool primaryPressed;

        public override void Update()
        {
            base.Update();
            AskForNewPath = false;
            var target = GetTarget();
            if (!target.IsValid)
                return;

            var dist = Vector3.Distance(BodyTransform.position, target.Position.Value);
            Debug.Log(dist);
            if (target.HasLOS(CharacterBody, out aimDir, out _) && dist < 1.5f)
            {
                primaryPressed = true;
            }
            else
            {
                AskForNewPath = true;
                primaryPressed = false;
                aimDir = ICharacterMovementController?.Motor.CharacterForward ?? BodyTransform.forward;
            }
        }

        public override CharacterMasterAI.AIInputs GenerateInputs()
        {
            return new CharacterMasterAI.AIInputs
            {
                primaryPressed = primaryPressed,
                aimDir = aimDir
            };
        }
    }
}