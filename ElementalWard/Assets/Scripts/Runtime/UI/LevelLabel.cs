using TMPro;
using UnityEngine;

namespace ElementalWard
{
    public class LevelLabel : MonoBehaviour
    {
        public TextMeshProUGUI label;

        private void Start()
        {
            if (DungeonManager.Instance)
                label.text = string.Format(label.text, DungeonManager.Instance.DungeonFloor + 1);
        }
    }
}