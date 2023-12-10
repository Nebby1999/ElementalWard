namespace EntityStates
{
    public class GenericCharacterDeath : BaseCharacterState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", "Death");
            DestroyBody();
        }

        public virtual void DestroyBody()
        {
            Destroy(GameObject);
        }
    }
}