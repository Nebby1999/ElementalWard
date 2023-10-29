using ElementalWard;

namespace EntityStates.Player.Weapon.Book
{
    public class DetonateOrbs : BaseCharacterState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if(TryGetComponent<RemoteDetonateTracker>(out var component))
            {
                component.DetonateAll();
            }
        }
    }
}