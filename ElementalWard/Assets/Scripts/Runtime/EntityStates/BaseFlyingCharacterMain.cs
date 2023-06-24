using ElementalWard;

namespace EntityStates
{
    public class BaseFlyingCharacterMain : BaseCharacterMain
    {
        public bool HasFlyingCharacterMovementController { get; private set; }
        public FlyingCharacterMovementController FlyingCharacterMovementController { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
            if (ICharacterMovementController is FlyingCharacterMovementController flyingController)
            {
                HasFlyingCharacterMovementController = flyingController;
                FlyingCharacterMovementController = flyingController;
            }
        }
    }
}