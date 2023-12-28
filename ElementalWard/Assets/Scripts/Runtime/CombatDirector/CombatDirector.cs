using Nebula;
using UnityEngine;

namespace ElementalWard
{
    public class CombatDirector : MonoBehaviour
    {
        private static readonly FloatMinMax CREDIT_GAIN_RANGE = new FloatMinMax(20, 100);
        private static readonly FloatMinMax CREDIT_GAIN_INTERVAL = new FloatMinMax(5, 20);
        private static readonly FloatMinMax SPAWN_RANGE = new FloatMinMax(10, 40);
        private static readonly FloatMinMax REROLL_RANGE = new FloatMinMax(5, 10);

        public ulong DungeonFloor { get => _dungeonFloor; private set => _dungeonFloor = value; }

        public WeightedCollection<DirectorCard> Cards { get; private set; }
        public WeightedCollection<ElementDef> AvailableElements { get; private set; }
        public float Credits { get; private set; }

        [SerializeField] private CombatCardDeck _deck;
        [SerializeField] private SerializableWeightedCollection<ElementDef> availableElements;
        [SerializeField] private ulong _dungeonFloor;
        [SerializeField] private float _startingCredits;

        private Xoroshiro128Plus _rng;
        private float _creditGainStopwatch;
        private float _spawnStopwatch;
        private DirectorCard _currentCard;
        private DirectorCard _cheapestCard;
        private DirectorCard _lastAttemptedCard;
        private ElementDef _currentElement;
        private float _difficultyCoefficient;
        private int _spawnAttempts;
        private int _monsterSpawnCount;
        private uint _maxSpawnTimes; 

        private void Start()
        {
            DungeonFloor = DungeonManager.Instance.DungeonFloor;
            _difficultyCoefficient = DungeonManager.Instance.DifficultyCoefficient;

            _rng = new Xoroshiro128Plus(DungeonManager.Instance.rng.NextUlong);

            Cards = _deck.GenerateSelectionFromPools(_deck.categories);
            Cards.SetSeed(_rng.NextUlong);

            int cheapestIndex = -1;
            float cheapestCardCost = float.PositiveInfinity;
            for(int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].value.cardCost < cheapestCardCost)
                {
                    cheapestIndex = i;
                    cheapestCardCost = Cards[i].value.cardCost;
                }
            }
            if(cheapestIndex != -1)
            {
                _cheapestCard = Cards[cheapestIndex].value;
            }

            AvailableElements = availableElements.CreateWeightedCollection();
            AvailableElements.SetSeed(_rng.NextUlong);

            Credits = _startingCredits * _difficultyCoefficient;
            _creditGainStopwatch = CREDIT_GAIN_INTERVAL.GetRandomRange(_rng);
            _spawnStopwatch = SPAWN_RANGE.GetRandomRange(_rng);
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            _creditGainStopwatch -= deltaTime;
            if(_creditGainStopwatch <= 0)
            {
                _creditGainStopwatch += CREDIT_GAIN_INTERVAL.GetRandomRange(_rng);
                Credits += CREDIT_GAIN_RANGE.GetRandomRange(_rng) * _difficultyCoefficient;
            }

            _spawnStopwatch -= Time.fixedDeltaTime;
            if(!(_spawnStopwatch <= 0))
            {
                return;
            }
            
            if(AttemptSpawnOnTarget(deltaTime))
            {
                _spawnStopwatch += SPAWN_RANGE.GetRandomRange(_rng);
                return;
            }
            _spawnStopwatch += REROLL_RANGE.GetRandomRange(_rng);
            _spawnAttempts++;
            if(_spawnAttempts > 5)
            {
                _spawnAttempts = 0;
                _currentCard = null;
            }
        }

        private bool AttemptSpawnOnTarget(float deltaTime)
        {
            if(_currentCard == null)
            {
                if (Cards == null)
                    return false;

                PrepareNewSpawnWave(Cards.Next());
            }
            if (_currentCard == null)
                return false;

            if(Credits < _currentCard.cardCost)
            {
                return false;
            }
            CharacterSpawnCard spawnCard = _currentCard.spawnCard as CharacterSpawnCard;
            if (TrySpawn(spawnCard, GetTargetForAmbush()))
            {
                Credits -= _currentCard.cardCost;
                _currentCard = null;
                return true;
            }
            return false;
        }

        private void PrepareNewSpawnWave(DirectorCard card)
        {
            _currentCard = card;
            if(_currentCard == _lastAttemptedCard)
            {
                _currentCard = null;
                return;
            }

            _currentElement = null;

            ElementDef elementDef = null;
            if((card.spawnCard as CharacterSpawnCard).canUseElements)
            {
                elementDef = AvailableElements.Next();
            }
            _currentElement = elementDef;
        }

        private bool TrySpawn(CharacterSpawnCard spawnCard, Transform target, PlacementRule.PlacementDelegate placementDelegate = null)
        {
            placementDelegate ??= PlacementRule.NearestNodePlacement;
            var placementRule = new PlacementRule(target)
            {
                maxDistance = _currentCard.maximumSpawnDistance,
                minDistance = _currentCard.minimumSpawnDistance,
                placement = placementDelegate,
            };
            var spawnRequest = new SpawnRequest(spawnCard, placementRule, _rng);
            spawnRequest.onSpawned = OnSpawned;

            return DungeonManager.Instance.TrySpawnObject(spawnRequest);

            void OnSpawned(SpawnCard.SpawnResult spawnResult)
            {
                var result = (CharacterSpawnCard.CharacterSpawnResult)spawnResult;
                if(!result.body)
                {
                    return;
                }
                if(result.body.TryGetComponent<IElementProvider>(out var provider))
                {
                    provider.ElementDef = _currentElement;
                }
                if(result.body.TryGetComponent<CharacterMotorController>(out var controller))
                {
                    if(!controller.IsFlying)
                    {
                        var pos = result.body.transform.position;
                        pos.y += controller.MotorCapsule.height / 2;
                    }
                }
            }

        }

        public void SpendAllCreditsOnMapSpawn()
        {
            while(Credits > _cheapestCard.cardCost)
            {
                if(_currentCard == null)
                {
                    if (Cards == null)
                        break;

                    PrepareNewSpawnWave(Cards.Next());
                }
                if (_currentCard == null)
                    continue;

                CharacterSpawnCard spawnCard = _currentCard.spawnCard as CharacterSpawnCard;

                if(Credits < _currentCard.cardCost)
                {
                    _lastAttemptedCard = _currentCard;
                    _currentCard = null;
                    continue;
                }

                if (TrySpawn(spawnCard, transform, PlacementRule.RandomNodePlacement))
                {
                    Credits -= _currentCard.cardCost;
                    _currentCard = null;
                    Physics.SyncTransforms();
                }
            }
        }

        private Transform GetTargetForAmbush()
        {
            if(InstanceTracker.Any<PlayableCharacterMaster>())
            {
                return _rng.NextElementUniform(InstanceTracker.GetInstances<PlayableCharacterMaster>()).transform;
            }
            return transform;
        }
    }
}