using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New SceneCardDeck", menuName = "ElementalWard/DirectorCardDecks/SceneCardDeck")]
    public class SceneCardDeck : DirectorCardDeck
    {
        public CardPool[] categories;

        protected override void LogCards()
        {
            base.LogCards();
            Debug.Log(GenerateSelectionFromPools(categories));
        }
    }
}