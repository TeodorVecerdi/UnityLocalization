using UnityEditor;
using UnityEngine;
using UnityLocalization.Data;

namespace UnityLocalization {
    public class LocalizationSettingsWindow : EditorWindow {
        private ActiveLocalizationSettings activeSettings;
        private Editor activeSettingsEditor;
        
        [MenuItem("Localization/Settings Editor")]
        private static void ShowWindow() {
            var window = GetWindow<LocalizationSettingsWindow>();
            window.titleContent = new GUIContent("Localization Settings");
            window.Show();
        }

        private void OnEnable() {
            activeSettings = ActiveLocalizationSettings.instance;
            activeSettingsEditor = Editor.CreateEditor(activeSettings);
        }

        private void OnGUI() {
            activeSettingsEditor.OnInspectorGUI();
        }
    }
}