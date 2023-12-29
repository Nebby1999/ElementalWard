using UnityEngine;

namespace ElementalWard
{
    public class StageComplete : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if (!other.TryGetComponent<CharacterBody>(out var body))
                return;

            if (!body.IsAIControlled)
                Run.Instance.StageComplete();
        }
    }
}