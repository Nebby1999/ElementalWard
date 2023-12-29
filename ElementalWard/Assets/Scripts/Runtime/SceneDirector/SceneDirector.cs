using Nebula;
using UnityEngine;

namespace ElementalWard
{
    public class SceneDirector : MonoBehaviour
    {
        private static readonly FloatMinMax BASE_CREDITS = new FloatMinMax(100, 200);

        public ulong DungeonFloor { get => _dungeonFloor; private set => _dungeonFloor = value; }

        public WeightedCollection<DirectorCard> Cards { get; private set; }

        public float Credits { get; private set; }

        [SerializeField] private SceneCardDeck _deck;
        [SerializeField] private ulong _dungeonFloor;

        private Xoroshiro128Plus _rng;
        private float _difficultyCoefficient;
        private DirectorCard _currentCard;
        private DirectorCard _cheapestCard;
        private DirectorCard _lastAttemptedCard;

        private void Start()
        {
            DungeonFloor = DungeonManager.Instance.DungeonFloor;
            _difficultyCoefficient = DungeonManager.Instance.DifficultyCoefficient;

            _rng = new Xoroshiro128Plus(DungeonManager.Instance.rng.NextUlong);

            Cards = _deck.GenerateSelectionFromPools(_deck.categories);
            Cards.SetSeed(_rng.NextUlong);

            int cheapestIndex = -1;
            float cheapestCardCost = float.PositiveInfinity;
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].value.cardCost < cheapestCardCost)
                {
                    cheapestIndex = i;
                    cheapestCardCost = Cards[i].value.cardCost;
                }
            }
            if (cheapestIndex != -1)
            {
                _cheapestCard = Cards[cheapestIndex].value;
            }

            Credits = BASE_CREDITS.GetRandomRangeLimits(_rng) * _difficultyCoefficient;
        }

        public void PopulateScene()
        {
            int tries = 0;
            while(Credits > _cheapestCard.cardCost)
            {
                if (tries > 100)
                    break;

                if(_currentCard == null)
                {
                    if (Cards == null)
                        break;

                    PrepareNewPickup(Cards.Next());
                }

                if (_currentCard == null)
                {
                    tries++;
                    continue;
                }

                PickupSpawnCard spawnCard = _currentCard.spawnCard as PickupSpawnCard;

                if(Credits < _currentCard.cardCost)
                {
                    _lastAttemptedCard = _currentCard;
                    _currentCard = null;
                    tries++;
                    continue;
                }

                if(TrySpawn(spawnCard, transform, PlacementRule.RandomNodePlacement))
                {
                    Credits -= _currentCard.cardCost;
                    _currentCard = null;
                    Physics.SyncTransforms();
                    continue;
                }
                tries++;
            }
        }

        private void PrepareNewPickup(DirectorCard card)
        {
            _currentCard = card;
            if (_currentCard == _lastAttemptedCard)
            {
                _currentCard = null;
                return;
            }
        }

        private bool TrySpawn(PickupSpawnCard spawnCard, Transform target, PlacementRule.PlacementDelegate placementDelegate = null)
        {
            placementDelegate ??= PlacementRule.NearestNodePlacement;
            var placementRule = new PlacementRule(target)
            {
                maxDistance = _currentCard.maximumSpawnDistance,
                minDistance = _currentCard.minimumSpawnDistance,
                placement = placementDelegate,
            };
            var spawnRequest = new SpawnRequest(spawnCard, placementRule, _rng);

            return DungeonManager.Instance.TrySpawnObject(spawnRequest);
        }
    }
}