using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalWard.UI
{
    public class HealthBar : HUDBehaviour
    {
        [SerializeField]
        private Slider _healthSlider;
        [SerializeField]
        private TextMeshProUGUI _healthLabel;
        [SerializeField]
        private string _healthLabelTextFormat;

        private HealthComponent _healthComponent;
        public override void OnBodyAssigned()
        {
            _healthComponent = HUD.TiedBody.HealthComponent;
            if(!_healthComponent)
            {
                gameObject.SetActive(false);
            }
        }
        private void FixedUpdate()
        {
            var maxValue = _healthComponent.FullHealth;
            _healthSlider.minValue = 0;
            _healthSlider.maxValue = maxValue;
            _healthSlider.value = _healthComponent.CurrentHealth;

            _healthLabel.text = string.Format(_healthLabelTextFormat, Mathf.CeilToInt(_healthComponent.CurrentHealth), Mathf.CeilToInt(maxValue));
        }
    }
}