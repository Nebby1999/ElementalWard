using Nebula;
using UnityEngine;

namespace ElementalWard
{
    public class CombatTest : MonoBehaviour
    {
        public SpawnCard wanderingSoul;
        public SpawnCard starvling;
        [Range(0, 100f)]
        public float elementalChance;
        public FloatMinMax spawnIntervalRange;

        private Xoroshiro128Plus _rng;
        private float _stopwatch;
        private CharacterBody _playerBody;
        private void Awake()
        {
            _rng = new Xoroshiro128Plus(ElementalWardApplication.rng.NextUlong);
            _stopwatch += spawnIntervalRange.GetRandomRange(_rng);
            PlayableCharacterMaster.OnPlayableBodySpawned += PlayableCharacterMaster_OnPlayableBodySpawned;
        }

        private void PlayableCharacterMaster_OnPlayableBodySpawned(CharacterBody obj)
        {
            _playerBody = obj;
        }

        private void FixedUpdate()
        {
            _stopwatch -= Time.fixedDeltaTime;
            if(_stopwatch <= 0)
            {
                _stopwatch += spawnIntervalRange.GetRandomRange(_rng);
                Spawn(_rng.NextInt % 2 == 0 ? wanderingSoul : starvling);
            }
        }

        private void Spawn(SpawnCard chosenCard)
        {
            ElementDef element = Util.CheckRoll(elementalChance, _rng) ? ElementCatalog.GetElementDef((ElementIndex)_rng.RangeInt(0, ElementCatalog.ElementCount)) : null;
            Vector3 position = _playerBody ? _playerBody.transform.position : Vector3.zero;

            var gameObjectInstance = chosenCard.Spawn(position, Quaternion.identity);
            var masterInstance = gameObjectInstance.GetComponent<CharacterMaster>();
            masterInstance.OnBodySpawned += (body) =>
            {
                var elementProvider = body.GetComponent<IElementProvider>();
                if(elementProvider != null)
                    elementProvider.ElementDef = element;
            };
        }

        private void OnDestroy()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned -= PlayableCharacterMaster_OnPlayableBodySpawned;
        }
    }
}