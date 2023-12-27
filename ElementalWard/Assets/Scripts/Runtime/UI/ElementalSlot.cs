using UnityEngine;
using UnityEngine.UI;

namespace ElementalWard.UI
{
    public class ElementalSlot : HUDBehaviour
    {
        [SerializeField]
        private Image _image;
        private IElementProvider _bodyProvider;
        public override void OnBodyAssigned()
        {
            _bodyProvider = HUD.TiedBody.GetComponent<IElementProvider>();
            if (_bodyProvider == null)
                gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            _image.sprite = _bodyProvider.ElementDef?.icon;
            _image.enabled = _image.sprite;
        }
    }
}