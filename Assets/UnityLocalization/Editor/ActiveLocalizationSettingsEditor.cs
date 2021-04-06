using System;
using UnityEditor;
using UnityEngine;
using UnityLocalization.Data;

namespace UnityLocalization {
    [CustomEditor(typeof(ActiveLocalizationSettings))]
    public class ActiveLocalizationSettingsEditor : UnityEditor.Editor {
        private ActiveLocalizationSettings currentSettings;
        private void OnEnable() {
            currentSettings = target as ActiveLocalizationSettings;
        }

        public override void OnInspectorGUI() {
            currentSettings.ActiveSettings = (LocalizationSettings) EditorGUILayout.ObjectField("Active Settings", currentSettings.ActiveSettings, typeof(LocalizationSettings), false, GUILayout.ExpandWidth(true));
            if (currentSettings.ActiveSettings == null) {
                DrawCreateMenu();
            }
        }

        private void DrawCreateMenu() {
            GUILayout.Box("There is no active Localization Settings selected. Would you like to create one?");
            if (GUILayout.Button("Create")) {
                var savePath = EditorUtility.SaveFilePanel("Create Localization Settings", Application.dataPath, "Localization Settings", "asset");
                if (string.IsNullOrEmpty(savePath)) {
                    Debug.Log("Did not create asset");
                    return;
                }

                if (!savePath.StartsWith(Application.dataPath)) {
                    EditorUtility.DisplayDialog("Invalid location!", "The location specified is not valid. You should create the localization settings in your assets folder.",
                                                "Close");
                    return;
                }
                var cleanPath = savePath.Substring(Application.dataPath.LastIndexOf('/') + 1);
                Debug.Log($"Cleaned path: {cleanPath}");
                var instance = CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(instance, cleanPath);
                AssetDatabase.ImportAsset(cleanPath, ImportAssetOptions.ForceUpdate);
                currentSettings.ActiveSettings = instance;
                Debug.Log($"Created asset at path {savePath}");
            }
        }
    }
}