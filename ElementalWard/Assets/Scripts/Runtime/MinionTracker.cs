using EntityStates;
using Nebula;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class MinionTracker : MonoBehaviour
    {
        public int MaxMinionCount => _maxMinionCount;
        [SerializeField] private int _maxMinionCount;

        public int MinionCount => _minions.Count;

        public bool HasMinionSlots => MinionCount < MaxMinionCount;
        private List<CharacterBody> _minions = new List<CharacterBody>();
        public void AddMinion(CharacterBody body)
        {
            if(HasMinionSlots)
            {
                _minions.Add(body);
            }
        }

        private void FixedUpdate()
        {
            for(int i = _minions.Count - 1; i >= 0; i--)
            {
                var minion = _minions[i];
                if(!minion)
                {
                    _minions.RemoveAt(i);
                }
            }
        }
    }

    public class MinionTrackerSkillBehaviour : ISkillBehaviourCallback
    {
        public MinionTracker minionTracker;
        public bool addedMinionTrackerAtRuntime = false;
        public void OnAssigned(GenericSkill skillSlot)
        {
            if (!skillSlot.gameObject.TryGetComponent<MinionTracker>(out minionTracker))
            {
                addedMinionTrackerAtRuntime = true;
                minionTracker = skillSlot.gameObject.AddComponent<MinionTracker>();
            }
        }

        public bool OnCanExecute(GenericSkill skillSlot, bool previousValue)
        {
            return previousValue && minionTracker.HasMinionSlots;
        }

        public void OnExecute(GenericSkill skillSlot, EntityStateBase incomingState)
        {
        }

        public float OnFixedUpdate(GenericSkill skillSlot, float deltaTime)
        {
            return deltaTime;
        }

        public void OnUnassigned(GenericSkill skillSlot)
        {
            if(minionTracker && addedMinionTrackerAtRuntime)
            {
                Object.Destroy(minionTracker);
            }
        }
    }
}