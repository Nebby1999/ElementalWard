using ElementalWard;
using UnityEngine;

namespace EntityStates.ElementalSpectre
{
    public class SummonSouls : BaseCharacterState
    {
        public static CharacterSpawnCard spawnCard;
        public static int summonCount;
        public static float baseDuration;
        public static string animName;

        private float _duration;
        private bool _hasFired;
        private CharacterAnimationEvents _characterAnimEvents;
        private MinionTracker _minionTracker;
        private bool _hasTracker;

        public override void OnEnter()
        {
            base.OnEnter();

            _minionTracker = GetComponent<MinionTracker>();
            _hasTracker = _minionTracker;

            _duration = baseDuration / attackSpeedStat;
            _characterAnimEvents = GetAnimationEvents();
            if(_characterAnimEvents)
                _characterAnimEvents.OnAnimationEvent += DoSummon;

            PlayAnimation("Base", animName, "attackSpeed", _duration);
        }

        private void DoSummon(int obj)
        {
            if(obj == CharacterAnimationEvents.fireAttackHash)
            {
                _hasFired = true;
                Summon();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(FixedAge > _duration)
            {
                if(!_hasFired)
                {
                    _hasFired = true;
                    Summon();
                }
                outer.SetNextStateToMain();
            }
        }

        private void Summon()
        {
            for(int i = 0; i < summonCount; i++)
            {
                if (_hasTracker && !_minionTracker.HasMinionSlots)
                    return;
                var spawnRequest = new SpawnRequest(spawnCard, new PlacementRule(Transform)
                {
                    maxDistance = 10,
                    minDistance = 0,
                    placement = PlacementRule.NearestNodePlacement,
                }, CharacterBody.CharacterRNG);
                spawnRequest.spawnerObject = GameObject;
                spawnRequest.onSpawned += (x) =>
                {
                    var result = (CharacterSpawnCard.CharacterSpawnResult)x;
                    if (result.body && result.body.TryGetComponent<IElementProvider>(out var provider))
                    {
                        provider.ElementDef = ElementProvider.ElementDef;
                    }

                    if(CharacterBody.TiedMaster && CharacterBody.TiedMaster.CharacterMasterAI)
                    {
                        result.spawnedInstance.GetComponent<CharacterMasterAI>().SetTarget(CharacterBody.TiedMaster.CharacterMasterAI.CurrentTarget);
                    }
                    if (_hasTracker)
                        _minionTracker.AddMinion(result.body);
                };

                DungeonManager.Instance.TrySpawnObject(spawnRequest);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (_characterAnimEvents)
                _characterAnimEvents.OnAnimationEvent -= DoSummon;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}