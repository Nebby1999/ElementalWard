using ElementalWard;

namespace EntityStates
{
    public class BaseGroundedCharacterMain : BaseCharacterMain
    {
        public bool HasGroundedCharacterMovementController { get; private set; }
        public GroundedCharacterMovementController GroundedCharacterMovementController { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
            if (ICharacterMovementController is GroundedCharacterMovementController groundedController)
            {
                HasGroundedCharacterMovementController = groundedController;
                GroundedCharacterMovementController = groundedController;
            }
        }
    }
}
