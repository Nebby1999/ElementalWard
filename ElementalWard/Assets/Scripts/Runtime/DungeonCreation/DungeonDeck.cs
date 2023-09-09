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
        public Card[] entrywayCards = Array.Empty<Card>();
        public Card[] roomCards = Array.Empty<Card>();
        public Card[] hallwayCards = Array.Empty<Card>();
        public Card[] bossRoomCards = Array.Empty<Card>();

        public WeightedSelection<GameObject> GenerateSelection(Card[] cards)
        {
            return WeightedSelection<GameObject>.CreateFrom(cards);
        }

        [Serializable]
        public struct Card : WeightedSelection<GameObject>.IWeightedSelectionEntry
        {
            public float Weight => _weight;
            public GameObject Value => _prefab;

            [SerializeField, ForcePrefab] private GameObject _prefab;
            [SerializeField] private float _weight;

        }
    }
}
