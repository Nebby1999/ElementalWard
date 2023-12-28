using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalWard.UI
{
    public class ManaBar : HUDBehaviour
    {
        [SerializeField]
        private Slider _manaSlider;
        [SerializeField]
        private TextMeshProUGUI _manaLabel;
        [SerializeField]
        private string _manaLabelTextFormat;

        private ManaComponent _manaComponent;
        public override void OnBodyAssigned()
        {
            _manaComponent = HUD.TiedBody.ManaComponent;
            if(!_manaComponent)
            {
                gameObject.SetActive(false);
            }
        }
        private void FixedUpdate()
        {
            var maxValue = _manaComponent.FullMana;
            _manaSlider.minValue = 0;
            _manaSlider.maxValue = maxValue;
            _manaSlider.value = _manaComponent.CurrentMana;

            _manaLabel.text = string.Format(_manaLabelTextFormat, Mathf.CeilToInt(_manaComponent.CurrentMana), Mathf.CeilToInt(maxValue));
        }
    }
}