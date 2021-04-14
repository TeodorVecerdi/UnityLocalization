using UnityEngine;
using UnityLocalization.Data;
using UnityLocalization.Shared;

namespace UnityLocalization.Runtime {
    public static class Localization {
        public static LocalizationSettings Settings;
        public static int CurrentLocaleIndex;
        private static bool initialized;
        private static Locale currentLocale;

        public static string TranslateString(string tableGuid, string keyGuid) {
            if (!initialized) Initialize();
            return TranslateString(tableGuid, keyGuid, CurrentLocaleIndex);
        }

        public static string TranslateString(string tableGuid, string keyGuid, Locale locale) {
            if (!initialized) Initialize();
            if (locale.Equals(currentLocale)) return TranslateString(tableGuid, keyGuid);
            var localeIndex = Settings.Locales.IndexOf(locale);
            if (localeIndex < 0) return null;
            return TranslateString(tableGuid, keyGuid, localeIndex);
        }

        public static string TranslateString(string tableGuid, string keyGuid, int localeIndex) {
            if (!initialized) Initialize();
            var table = Settings.GuidToTable(tableGuid);
            if (table == null) return null;
            var entry = table.GuidToEntry(keyGuid);
            var translation = entry?.Values[localeIndex];
            translation = translation?.Replace("\\n", "\n");
            return translation;
        }

        public static void SetLocale(int index) {
            if (!initialized) Initialize();
            if (index < 0 || index >= Settings.Locales.Count) return;
            CurrentLocaleIndex = index;
            currentLocale = Settings.Locales[index];
            SetLocaleIndexPrefs(index);

            // Update all translations
            if (Application.isPlaying) {
                foreach (var localizedString in Object.FindObjectsOfType<LocalizedString>(true)) {
                    localizedString.Translate();
                }
            }
        }

        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod, UnityEditor.Callbacks.DidReloadScripts]
#endif
        private static void Initialize() {
            Settings = Resources.FindObjectsOfTypeAll<LocalizationSettings>()[0].ActiveSettings;
            var localeIndex = GetLocaleIndexPrefs();
            CurrentLocaleIndex = localeIndex;
            currentLocale = Settings.Locales[localeIndex];
            initialized = true;
        }

        private static int GetLocaleIndexPrefs() {
#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.HasKey(Constants.SELECTED_LOCALE_PREFS_KEY)) return UnityEditor.EditorPrefs.GetInt(Constants.SELECTED_LOCALE_PREFS_KEY);
            var localeIndex = Settings.DefaultLocaleIndex();
            UnityEditor.EditorPrefs.SetInt(Constants.SELECTED_LOCALE_PREFS_KEY, localeIndex);
            return localeIndex;
#else
            if (PlayerPrefs.HasKey(Constants.SELECTED_LOCALE_PREFS_KEY)) return PlayerPrefs.GetInt(Constants.SELECTED_LOCALE_PREFS_KEY);
            var localeIndex = settings.DefaultLocaleIndex();
            PlayerPrefs.SetInt(Constants.SELECTED_LOCALE_PREFS_KEY, localeIndex);
            return localeIndex;
#endif
        }

        private static void SetLocaleIndexPrefs(int index) {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetInt(Constants.SELECTED_LOCALE_PREFS_KEY, index);
#else
            PlayerPrefs.SetInt(Constants.SELECTED_LOCALE_PREFS_KEY, index);
#endif
        }
    }
}