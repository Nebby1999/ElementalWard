using System;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New DungeonCardDeck", menuName = "ElementalWard/DungeonDecks/DungeonCardDeck")]
    public class DungeonCardDeck : DirectorCardDeck
    {
        public CardPool entrywayRoomCards;
        public CardPool[] roomCards = Array.Empty<CardPool>();
        public CardPool bossRoomCards;

        protected override void LogCards()
        {
            Debug.Log(GenerateSelection(entrywayRoomCards));
            Debug.Log(GenerateSelectionFromPools(roomCards));
        }

    }
}