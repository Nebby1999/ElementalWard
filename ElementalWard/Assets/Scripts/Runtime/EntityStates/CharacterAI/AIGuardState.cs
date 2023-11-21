namespace EntityStates.CharacterAI
{
    public class AIGuardState : BaseAIState
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!AI || !CharacterBody)
                return;

            if(AI.DriverEvaluation.dominantDriver != null)
            {
                outer.SetNextState(new AICombatState());
                return;
            }
        }
    }
}