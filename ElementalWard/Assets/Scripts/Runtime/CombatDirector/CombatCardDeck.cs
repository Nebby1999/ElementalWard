using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New CombmatCardDeck", menuName = "ElementalWard/DirectorCardDecks/CombatCardDeck")]
    public class CombatCardDeck : DirectorCardDeck
    {
        public CardPool[] categories;

        protected override void LogCards()
        {
            Debug.Log(GenerateSelectionFromPools(categories));
        }
    }
}