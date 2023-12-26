using Cinemachine;

namespace ElementalWard
{
    public class ModifiedCinemachineInputProvider : CinemachineInputProvider
    {
        private float _scale;
        private void OnEnable()
        {
            SettingsCollection.OnSettingChanged += UpdateSetting;
            _scale = SettingsCollection.LookSensitivity;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SettingsCollection.OnSettingChanged -= UpdateSetting;
        }

        private void UpdateSetting()
        {
            _scale = SettingsCollection.LookSensitivity;
        }

        public override float GetAxisValue(int axis)
        {
            return base.GetAxisValue(axis) * _scale;
        }
    }
}