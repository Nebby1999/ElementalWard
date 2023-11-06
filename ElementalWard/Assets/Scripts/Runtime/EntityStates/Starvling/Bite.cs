using ElementalWard;

namespace EntityStates.Starvling
{
    public class Bite : BaseCharacterState
    {
        public static float damageCoefficient;
        public static float baseDuration;
        public static string animationName;
        public static string hitboxGroupName;

        private HitBoxAttack _hitboxAttack;
        private float _damage;
        private float _duration;
        private bool _hasBitten;
        private CharacterAnimationEvents _characterAnimEvents;
        private HitBoxGroup _hitboxGroup;

        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;

            _damage = damageStat * damageCoefficient;
            var locator = GetSpriteBaseTransform();
            _hitboxGroup = HitBoxGroup.FindHitBoxGroup(locator.gameObject, hitboxGroupName);
            if(_hitboxGroup)
            {
                _hitboxGroup.updateRotation = false;
            }

            _hitboxAttack = new HitBoxAttack()
            {
                attacker = new BodyInfo(GameObject),
                baseDamage = _damage,
                hitBoxGroup = _hitboxGroup
            };

            _characterAnimEvents = GetAnimationEvents();
            if(_characterAnimEvents)
                _characterAnimEvents.OnAnimationEvent += FireAttack;

            PlayAnimation("Base", animationName, "attackSpeed", _duration);
        }

        private void FireAttack(int obj)
        {
            if(obj == CharacterAnimationEvents.fireAttackHash && !_hasBitten)
            {
                _hasBitten = true;
                _hitboxAttack.Fire();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(FixedAge > _duration)
            {
                if (!_hasBitten)
                {
                    _hasBitten = true;
                    _hitboxAttack.Fire();
                }
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if(_characterAnimEvents)
                _characterAnimEvents.OnAnimationEvent -= FireAttack;
            if (_hitboxGroup)
                _hitboxGroup.updateRotation = true;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}