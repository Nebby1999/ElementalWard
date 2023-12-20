using ElementalWard;

namespace EntityStates.Player.Weapon.Book
{
    public class DetonateOrbs : BaseWeaponState
    {
        public static float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            PlayWeaponAnimation("Base", "Secondary");
            if(TryGetComponent<RemoteDetonateTracker>(out var component))
            {
                component.DetonateAll();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(FixedAge > duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}