using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New DungeonDeck", menuName = "ElementalWard/DungeonDeck")]
    public class DungeonDeck : NebulaScriptableObject
    {
        public Card[] entrywayRoomCards = Array.Empty<Card>();
        public RoomPool[] roomCards = Array.Empty<RoomPool>();
        public Card[] bossRoomCards = Array.Empty<Card>();

        public WeightedCollection<Card> GenerateSelection(Card[] cards)
        {
            var collection = new WeightedCollection<Card>();
            foreach(var card in cards)
            {
                collection.Add(card, card.weight);
            }
            return collection;
        }

        public WeightedCollection<Card> GenerateSelectionFromPool(RoomPool[] pool)
        {
            var collection = new WeightedCollection<Card>();
            for(int i = 0; i < pool.Length; i++)
            {
                RoomPool roomPool = pool[i];
                float num = SumAllWeightsInCategory(roomPool);
                float num2 = roomPool.weight / num;
                if(!(num > 0f))
                {
                    continue;
                }
                Card[] cards = roomPool.cards;
                foreach(Card card in cards)
                {
                    float weight = card.weight * num2;
                    collection.Add(card, weight);
                }
            }

            return collection;

            float SumAllWeightsInCategory(RoomPool roomPool)
            {
                float totalWeight = 0f;
                for(int i = 0; i < roomPool.cards.Length; i++)
                {
                    totalWeight += roomPool.cards[i].weight;
                }
                return totalWeight;
            }
        }

        [ContextMenu("Log Cards")]
        private void LogCards()
        {
            Debug.Log(GenerateSelection(entrywayRoomCards));
            Debug.Log(GenerateSelectionFromPool(roomCards));
        }
        [Serializable]
        public class RoomPool
        {
            public string name;
            [Range(0, 100)]
            public float weight;
            public Card[] cards;
        }
        [Serializable]
        public class Card
        { 
            [ForcePrefab]
            public GameObject prefab;
            [Range(0, 100)]
            public float weight;
            public float cardCost;

            public override string ToString()
            {
                return $"{prefab} ({cardCost})";
            }
        }
    }
}
