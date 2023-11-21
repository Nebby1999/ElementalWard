using ElementalWard;
using ElementalWard.AI;
using UnityEngine;

namespace EntityStates.CharacterAI
{
    public class AICombatState : BaseAIState
    {
        private float _aiUpdateTimer;
        private AIDriver _dominantAIDriver;
        private SkillSlot _currentSkillSlot;
        private bool _currentSkillMeetsActivationConditions;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!AI || !CharacterBody)
                return;

            float deltaTime = Time.fixedDeltaTime;
            _aiUpdateTimer += deltaTime;
            if(_aiUpdateTimer > CharacterMasterAI.TIME_BETWEEN_AI_UPDATE)
            {
                _aiUpdateTimer -= CharacterMasterAI.TIME_BETWEEN_AI_UPDATE;
                UpdateAI(deltaTime);
                if(_dominantAIDriver == null)
                {
                    outer.SetNextState(new AIGuardState());
                }
            }
        }

        public override CharacterMasterAI.AIInputs GenerateAIInputs(in CharacterMasterAI.AIInputs previousInputs)
        {
            bool pressPrimary = false;
            bool pressSecondary = false;
            bool pressUtility = false;
            bool pressSpecial = false;

            if(CharacterInputBank)
            {
                AIDriverData.ButtonPressType pressType = AIDriverData.ButtonPressType.Abstain;
                if(_dominantAIDriver != null)
                {
                    pressType = _dominantAIDriver.ButtonPressType;
                }

            }
            throw new System.Exception();
        }

        private void UpdateAI(float deltaTime)
        {
            _dominantAIDriver = AI.DriverEvaluation.dominantDriver;
            _currentSkillSlot = SkillSlot.None;
            _currentSkillMeetsActivationConditions = false;
            aiInputs.movementInput = Vector3.zero;
            bool useNodeGraph = _dominantAIDriver.UseNodeGraph;
            AIDriverData.MovementType movementType = AIDriverData.MovementType.Stop;
            if (!CharacterBody || !CharacterInputBank)
                return;

            if(_dominantAIDriver != null)
            {
                movementType = _dominantAIDriver.MovementType;
                _currentSkillSlot = _dominantAIDriver.RequiredSkillSlot;
            }

            switch(movementType)
            {
                case AIDriverData.MovementType.Stop:
                    NavigationAgent.AskForPath = false;
                    NavigationAgent.CancelCurrentPath();
                    NavigationAgent.Stop();
                    break;
                case AIDriverData.MovementType.ChaseEnemy:
                    NavigationAgent.AskForPath = useNodeGraph;
                    NavigationAgent.Resume();
                    break;
            }


            NavigationAgent.UpdateFromAI(deltaTime);
            aiInputs.movementInput = NavigationAgent.CurrentPathfindingMovementVector;

            if(_dominantAIDriver.ActivationRequiresTargetLOS)
            {
                _currentSkillMeetsActivationConditions = AI.CurrentTarget.HasLOS(CharacterBody, out var _);
            }

            if(_dominantAIDriver.ActivationRequiresAimTargetLOS)
            {
                _currentSkillMeetsActivationConditions = AI.DriverEvaluation.aimTarget.HasAimLOS(CharacterBody, out _, out _);
            }
        }
    }
}