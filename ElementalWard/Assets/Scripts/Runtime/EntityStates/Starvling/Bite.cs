using ElementalWard;

namespace EntityStates.Starvling
{
    public class Bite : BaseCharacterState
    {
        public static float damageCoefficient;
        public static float baseDuration;
        public static float baseBiteTime;
        public static string animationName;
        public static string hitboxGroup;

        private HitBoxAttack _hitboxAttack;
        private float _damage;
        private float _duration;
        private float _biteTime;
        private bool _hasBitten;

        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            _biteTime = baseBiteTime / attackSpeedStat;

            _damage = damageStat * damageCoefficient;
            var locator = GetSpriteBaseTransform();
            _hitboxAttack = new HitBoxAttack()
            {
                attacker = new BodyInfo(GameObject),
                baseDamage = _damage,
                hitBoxGroup = HitBoxGroup.FindHitBoxGroup(locator.gameObject, hitboxGroup)
            };
            PlayAnimation("Base", animationName, "attackSpeed", _duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (FixedAge > _biteTime && !_hasBitten)
            {
                _hasBitten = true;
                _hitboxAttack.Fire();
            }
            if(FixedAge > _duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}