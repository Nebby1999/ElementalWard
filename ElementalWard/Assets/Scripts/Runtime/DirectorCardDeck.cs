using Nebula;
using System;
using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public abstract class DirectorCardDeck : NebulaScriptableObject
    {
        public WeightedCollection<DirectorCard> GenerateSelection(CardPool cardPool)
        {
            var collection = new WeightedCollection<DirectorCard>();
            foreach(var card in cardPool.cards)
            {
                collection.Add(card, card.weight);
            }
            return collection;
        }

        public WeightedCollection<DirectorCard> GenerateSelectionFromPools(CardPool[] cardPools)
        {
            var collection = new WeightedCollection<DirectorCard>();
            for(int i = 0; i < cardPools.Length; i++)
            {
                CardPool cardPool = cardPools[i];
                float num = SumAllWeightsInCategory(cardPool);
                float num2 = cardPool.weight / num;
                if (!(num > 0f))
                    continue;

                DirectorCard[] cards = cardPool.cards;
                foreach(DirectorCard card in cards)
                {
                    float weight = card.weight * num2;
                    collection.Add(card, weight);
                }
            }

            return collection;

            float SumAllWeightsInCategory(CardPool roomPool)
            {
                float totalWeight = 0f;
                for (int i = 0; i < roomPool.cards.Length; i++)
                {
                    totalWeight += roomPool.cards[i].weight;
                }
                return totalWeight;
            }
        }

        [ContextMenu("Log Cards")]
        protected virtual void LogCards()
        {
        }
    }
    [Serializable]
    public class CardPool
    {
        public string poolName;
        [Range(0, 100)]
        public float weight;
        public DirectorCard[] cards = Array.Empty<DirectorCard>();
    }
    [Serializable]
    public class DirectorCard
    {
        public SpawnCard spawnCard;
        [Range(0, 100)]
        public float weight;
        public float cardCost;

        public override string ToString()
        {
            return $"{spawnCard} ({cardCost})";
        }
    }
}