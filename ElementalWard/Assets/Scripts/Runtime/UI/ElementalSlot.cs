using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalWard.UI
{
    public class ElementalSlot : HUDBehaviour
    {
        [SerializeField]
        private Image _image;
        [SerializeField]
        TextMeshProUGUI _label;
        private IElementProvider _bodyProvider;
        private Inventory inventory;
        public override void OnBodyAssigned()
        {
            inventory = HUD.TiedBody.Inventory;
            _bodyProvider = HUD.TiedBody.GetComponent<IElementProvider>();

            if (!inventory)
                _label.gameObject.SetActive(false);

            if (_bodyProvider == null)
                gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            _image.sprite = _bodyProvider.ElementDef?.icon;
            _image.enabled = _image.sprite;
            _label.text = _bodyProvider.ElementDef ? Mathf.RoundToInt(inventory.GetElementEssence(_bodyProvider.ElementDef)).ToString() : string.Empty;

        }
    }
}