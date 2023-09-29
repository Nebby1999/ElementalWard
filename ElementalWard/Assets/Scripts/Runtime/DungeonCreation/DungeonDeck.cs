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

        [ContextMenu("Log Cards")]
        private void LogCards()
        {
            Debug.Log(GenerateSelection(entrywayRoomCards));
            Debug.Log(GenerateSelection(roomCards[0].cards));
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
