using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class DungeonDirector : MonoBehaviour
    {
        public DungeonDeck deck;

        private WeightedSelection<GameObject> _entrywaySelection;
        private Xoroshiro128Plus _rng;
        private void Awake()
        {
            _rng = new Xoroshiro128Plus((ulong)DateTime.UtcNow.Ticks);
        }
        private void Start()
        {
            _entrywaySelection = deck.GenerateSelection(deck.entrywayCards);
            _entrywaySelection.RecalculateTotalWeight();
            GenerateEntryway();
        }

        private void GenerateEntryway()
        {
            var prefab = _entrywaySelection.Evaluate(_rng.nextNormalizedFloat);
            Instantiate(prefab, transform);
        }
    }
}
