using UnityEditor;
using UnityEngine;
using UnityLocalization.Data.Utility;

namespace UnityLocalization.Data {
    public class ActiveLocalizationSettings : ScriptableSingleton<ActiveLocalizationSettings> {
        [SerializeField] private LocalizationSettings activeSettings;
        public LocalizationSettings ActiveSettings {
            get => activeSettings;
            set => activeSettings = value;
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