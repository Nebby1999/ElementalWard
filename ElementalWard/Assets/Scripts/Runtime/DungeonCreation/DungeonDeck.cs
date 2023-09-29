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
            var selection = new WeightedCollection<Card>();
            foreach(var card in cards)
            {
                selection.Add(card, card.weight);
            }
            return selection;
        }

        public WeightedCollection<Card> GenerateSelectionFromRoomPool(RoomPool[] roomPools)
        {
            WeightedCollection<Card> selection = new WeightedCollection<Card>();
            for(int i = 0; i < roomPools.Length; i++)
            {
                RoomPool pool = roomPools[i];
                float num = SumWeights(pool);
                float num2 = pool.weight / num;
                if(!(num > 0f))
                {
                    continue;
                }

                foreach(var card in pool.cards)
                {
                    float weight = card.weight / num2;
                    selection.Add(card, Mathf.RoundToInt(weight));
                }
            }
            return selection;

            float SumWeights(RoomPool pool)
            {
                float num = 0f;
                for(int i = 0; i < pool.cards.Length; i++)
                {
                    num += pool.cards[i].weight;
                }
                return num;
            }
        }

        [ContextMenu("Log Cards")]
        private void LogCards()
        {
            Debug.Log(GenerateSelection(entrywayRoomCards));
            Debug.Log(GenerateSelectionFromRoomPool(roomCards));
        }
        [Serializable]
        public class RoomPool
        {
            public string name;
            public float weight;
            public Card[] cards;
        }
        [Serializable]
        public class Card
        { 
            [ForcePrefab]
            public GameObject prefab;
            public int weight;
            public float cardCost;

            public override string ToString()
            {
                return $"{prefab} ({cardCost})";
            }
        }
    }
}
