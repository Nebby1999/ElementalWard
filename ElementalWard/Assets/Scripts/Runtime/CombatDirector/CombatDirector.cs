using Nebula;
using System;
using UnityEngine;

namespace ElementalWard
{
    public class CombatDirector : MonoBehaviour
    {
        public static readonly FloatMinMax CREDIT_GAIN_RANGE = new FloatMinMax(10, 50);
        public static readonly FloatMinMax CREDIT_GAIN_INTERVAL = new FloatMinMax(10, 30);

        public ulong DungeonFloor { get => _dungeonFloor; private set => _dungeonFloor = value; }
        [SerializeField] private CombatCardDeck _deck;
        [SerializeField] private ulong _dungeonFloor;
        [SerializeField] private float _startingCredits;

        public WeightedCollection<DirectorCard> Cards { get; private set; }
        public float Credits { get; private set; }

        private Xoroshiro128Plus _rng;
        private float _stopwatch;
        private DirectorCard _currentCard;
        private DirectorCard _previousCard;

        private void Start()
        {
            DungeonFloor = DungeonManager.Instance ? DungeonManager.Instance.DungeonFloor : _dungeonFloor;
            _rng = new Xoroshiro128Plus((ulong)DateTime.Now.Ticks);

            Cards = _deck.GenerateSelectionFromPools(_deck.categories);
            Credits = _startingCredits;
            _stopwatch = CREDIT_GAIN_INTERVAL.GetRandomRange(_rng);
            Simulate(Time.fixedDeltaTime);
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            _stopwatch -= deltaTime;
            if(_stopwatch <= 0)
            {
                _stopwatch = CREDIT_GAIN_INTERVAL.GetRandomRange(_rng);
                Credits += (CREDIT_GAIN_RANGE.GetRandomRange(_rng) );
                Simulate(deltaTime);
            }
        }

        private void Simulate(float deltaTime)
        {
            _currentCard = Cards.Next();
            if (_currentCard == _previousCard)
                return;

            if (_currentCard.cardCost < Credits)
                return;

            var monsterCount = 0;
            var creditsCopy = Credits;
            while(creditsCopy > _currentCard.cardCost)
            {
                monsterCount++;
                creditsCopy -= _currentCard.cardCost;
            }
            if (!(monsterCount > 0))
                return;


        }

    }
}