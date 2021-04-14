using UnityEditor;
using UnityEngine;
using UnityLocalization.Data;
using UnityLocalization.Shared;

namespace UnityLocalization {
    public class ActiveLocalizationSettings : ScriptableSingleton<ActiveLocalizationSettings> {
        [SerializeField] private LocalizationSettings activeSettings;
        public LocalizationSettings ActiveSettings {
            get => activeSettings;
            set {
                activeSettings = value;
                foreach (var localizationSettings in Resources.FindObjectsOfTypeAll<LocalizationSettings>()) {
                    localizationSettings.ActiveSettings = value;
                }
            }
        }

        public static ActiveLocalizationSettings Load() {
            var activeSettings = instance;
            if (EditorPrefs.HasKey(Constants.ACTIVE_SETTINGS_PREFS_KEY)) {
                activeSettings.ActiveSettings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(EditorPrefs.GetString(Constants.ACTIVE_SETTINGS_PREFS_KEY));
            }

            return activeSettings;
        }
    }
}