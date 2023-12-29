using TMPro;
using UnityEngine;

namespace ElementalWard
{
    public class GoldLabel : HUDBehaviour
    {
        public TextMeshProUGUI label;

        private Inventory _inventory;
        public override void OnBodyAssigned()
        {
            base.OnBodyAssigned();
            _inventory = HUD.TiedBody.Inventory;

            if (!_inventory)
            {
                gameObject.SetActive(false);
                return;
            }
        }

        private void FixedUpdate()
        {
            label.text = _inventory.Money.ToString(); ;
        }
    }
}