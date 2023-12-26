using UnityEngine;
using Nebula.Serialization;
using System;

namespace ElementalWard
{
    public static class SettingsCollection
    {
        public static string PlayerInputOverrides
        {
            get => PlayerPrefs.GetString(nameof(PlayerInputOverrides));
            set
            {
                PlayerPrefs.SetString(nameof(PlayerInputOverrides), value);
                PlayerPrefs.Save();
                OnSettingChanged?.Invoke();
            }
        }

        public static float LookSensitivity
        {
            get
            {
                var str = PlayerPrefs.GetString(nameof(LookSensitivity));
                return (float)StringSerializer.Deserialize(typeof(float), str);
            }
            set
            {
                var str = StringSerializer.Serialize(typeof(float), value);
                PlayerPrefs.SetString(nameof(LookSensitivity), str);
                PlayerPrefs.Save();
                OnSettingChanged?.Invoke();
            }
        }

        public static event Action OnSettingChanged;

    }
}