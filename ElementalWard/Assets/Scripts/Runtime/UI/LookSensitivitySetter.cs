using Nebula;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ElementalWard.UI
{
    public class LookSensitivitySetter : MonoBehaviour
    {
        private void Start()
        {
            var slider = GetComponent<Slider>();
            if(slider)
            {
                slider.value = SettingsCollection.LookSensitivity;
            }
        }
        public void OnValueChanged(float flt)
        {
            SettingsCollection.LookSensitivity = flt;
        }
    }
}