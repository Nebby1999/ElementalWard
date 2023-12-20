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
                bool wasPressed = false;
                switch(_currentSkillSlot)
                {
                    case SkillSlot.Primary:
                        wasPressed = previousInputs.primaryPressed;
                        break;
                    case SkillSlot.Secondary:
                        wasPressed = previousInputs.secondaryPressed;
                        break;
                    case SkillSlot.Utility:
                        wasPressed = previousInputs.utilityPressed;
                        break;
                    case SkillSlot.Special:
                        wasPressed = previousInputs.specialPressed;
                        break;
                }
                bool shouldPress = _currentSkillMeetsActivationConditions;
                switch(pressType)
                {
                    case AIDriverData.ButtonPressType.Abstain:
                        shouldPress = false;
                        break;
                    case AIDriverData.ButtonPressType.TapContinuous:
                        shouldPress = shouldPress && !wasPressed;
                        break;
                }
                switch(_currentSkillSlot)
                {
                    case SkillSlot.Primary:
                        pressPrimary = shouldPress;
                        break;
                    case SkillSlot.Secondary:
                        pressSecondary = shouldPress;
                        break;
                    case SkillSlot.Utility:
                        pressUtility = shouldPress;
                        break;
                    case SkillSlot.Special:
                        pressSpecial = shouldPress;
                        break;
                }
            }
            aiInputs.primaryPressed = pressPrimary;
            aiInputs.secondaryPressed = pressSecondary;
            aiInputs.utilityPressed = pressUtility;
            aiInputs.specialPressed = pressSpecial;
            aiInputs.sprintPressed = false;
            aiInputs.aimDir = Vector3.zero;
            if(_dominantAIDriver != null)
            {
                aiInputs.sprintPressed = _dominantAIDriver.ShouldSprint;
                var aimType = _dominantAIDriver.AimType;
                CharacterMasterAI.AITarget aimTarget = AI.DriverEvaluation.aimTarget;
                if(aimType == AIDriverData.AimType.MoveDirection)
                {
                    AimInDirection(ref aiInputs, CharacterInputBank.moveVector);
                }
                if(aimTarget != null)
                {
                    AimAt(ref aiInputs, aimTarget);
                }
            }
            return aiInputs;
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
            else
            {
                _currentSkillMeetsActivationConditions = true;
            }

            if(_dominantAIDriver.ActivationRequiresAimTargetLOS)
            {
                _currentSkillMeetsActivationConditions = AI.DriverEvaluation.aimTarget.HasAimLOS(CharacterBody, out _, out _);
            }
            else
            {
                _currentSkillMeetsActivationConditions = true;
            }
        }
    }
}