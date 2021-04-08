using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Utility;

namespace UnityLocalization {
    [CustomEditor(typeof(ActiveLocalizationSettings))]
    public class ActiveLocalizationSettingsEditor : Editor {
        private ActiveLocalizationSettings currentSettings;

        public event Action<LocalizationSettings> OnActiveSettingsChanged;

        private VisualElement rootElement;
        private ObjectField activeSettingsField;
        private VisualElement createAssetContainer;

        public override VisualElement CreateInspectorGUI() {
            currentSettings = target as ActiveLocalizationSettings;
            
            rootElement = new VisualElement { name = "ActiveSettings" };
            activeSettingsField = new ObjectField("Active Settings") {objectType = typeof(LocalizationSettings)};
            activeSettingsField.BindProperty(new SerializedObject(currentSettings).FindProperty("activeSettings"));
            activeSettingsField.RegisterValueChangedCallback(evt => {
                if (evt.previousValue == null && evt.newValue != null) {
                    createAssetContainer.AddToClassList("hidden");
                } else if (evt.previousValue != null && evt.newValue == null) {
                    createAssetContainer.RemoveFromClassList("hidden");
                }
                OnActiveSettingsChanged?.Invoke(evt.newValue as LocalizationSettings);
                if (evt.newValue == null) {
                    EditorPrefs.DeleteKey(Constants.ACTIVE_SETTINGS_PREFS_KEY);
                } else {
                    var assetPath = AssetDatabase.GetAssetPath(evt.newValue);
                    EditorPrefs.SetString(Constants.ACTIVE_SETTINGS_PREFS_KEY, assetPath);
                }
            });
            createAssetContainer = new VisualElement();
            createAssetContainer.Add(new HelpBox("There is no active Localization Settings selected. Would you like to create one?", HelpBoxMessageType.Info));
            var createAssetButton = new Button(TriggerCreateAsset) {text = "Create"};
            createAssetContainer.Add(createAssetButton);
            if(currentSettings.ActiveSettings != null) createAssetContainer.AddToClassList("hidden");
            activeSettingsField.value = currentSettings.ActiveSettings;
            
            rootElement.Add(activeSettingsField);
            rootElement.Add(createAssetContainer);
            return rootElement;
        }

        private void TriggerCreateAsset() {
            var savePath = EditorUtility.SaveFilePanel("Create Localization Settings", Application.dataPath, "Localization Settings", "asset");
            if (string.IsNullOrEmpty(savePath))
                return;

            if (!savePath.StartsWith(Application.dataPath)) {
                EditorUtility.DisplayDialog("Invalid location!", "The location specified is not valid. You should create the localization settings in your assets folder.", "Close");
                return;
            }

            var cleanPath = savePath.Substring(Application.dataPath.LastIndexOf('/') + 1);
            var instance = CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(instance, cleanPath);
            AssetDatabase.ImportAsset(cleanPath, ImportAssetOptions.ForceUpdate);
            currentSettings.ActiveSettings = instance;
        }
    }
}