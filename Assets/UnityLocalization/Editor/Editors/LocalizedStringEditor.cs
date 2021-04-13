using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Runtime;
using UnityLocalization.Shared;

namespace UnityLocalization {
    [CustomEditor(typeof(LocalizedString))]
    public class LocalizedStringEditor : Editor {
        // Properties //
        private SerializedProperty setterProperty;

        // VisualElements //
        private VisualElement rootElement;
        private VisualElement activeSettingsWarning;
        private VisualElement settingsContainer;
        
        // Other //
        private LocalizedString localizedString;

        private void OnEnable() {
            AcquireProperties();
            localizedString = target as LocalizedString;
            Debug.Assert(localizedString != null);
            localizedString.Settings = ActiveLocalizationSettings.Load().ActiveSettings;
            
            if(rootElement != null) UpdateWarningVisibility();
        }

        public override VisualElement CreateInspectorGUI() {
            
            rootElement = new VisualElement {name = "LocalizedString"};

            if (PropertiesDirty()) AcquireProperties();
            LoadStylesheets();

            rootElement.AddGet<VisualElement>("Header").Do(selfHeader => {
                selfHeader.AddGet<VisualElement>(null, "logo");
                var title = selfHeader.AddGet<Label>(null, "text");
                title.text = "Unity Localization";
            });

            activeSettingsWarning = rootElement.AddGet<VisualElement>("ActiveSettingsWarning").Do(self => {
                self.AddGet<VisualElement>("WarningContainer").Do(self2 => {
                    self2.AddGet<VisualElement>(null, "icon");
                    self2.AddGet<Label>(null, "text").text = "There is no active Localization Settings selected.";
                });
                self.AddGet(new Button(OpenSettingsWindow) {text = "Open Settings", name = "OpenSettingsButton"});
            });

            settingsContainer = rootElement.AddGet<VisualElement>("SettingsContainer");

            UpdateWarningVisibility();
            //1.  Active settings missing => message + button to open settings
            // (how to do callback when active settings change) // Resources.FindObjectsOfTypeAll<LocalizedStringEditor>()

            //2.  Table selection
            //3.  Key selection (dropdown? menu? window?)
            //4.  Preview?

            rootElement.schedule.Execute(() => {
                var parent = rootElement.parent;
                parent.AddStylesheet(Resources.Load<StyleSheet>("Stylesheets/LocalizedString_Parent"));
            });
            return rootElement;
        }

        public void OnSettingsDirty(LocalizationSettings newSettings) {
            localizedString.Settings = newSettings;
            UpdateWarningVisibility();
        }

        private bool PropertiesDirty() {
            return setterProperty == null;
        }

        private void AcquireProperties() {
            setterProperty = serializedObject.FindProperty("setter");
        }

        private void UpdateWarningVisibility() {
            var showSettings = localizedString.Settings != null;
            activeSettingsWarning.SetClass(showSettings, "hidden");
            settingsContainer.SetClass(!showSettings, "hidden");
        }

        private void LoadStylesheets(StyleSheet utility = null, StyleSheet main = null) {
            utility ??= Resources.Load<StyleSheet>("Stylesheets/Utility");
            main ??= Resources.Load<StyleSheet>("Stylesheets/LocalizedString");
            try {
                rootElement.AddStylesheet(utility);
                rootElement.AddStylesheet(main);
            } catch {
                // empty
            }
        }

        private void OpenSettingsWindow() {
            LocalizationSettingsWindow.ShowWindow();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void Initialize() {
            var allWindows = Resources.FindObjectsOfTypeAll<LocalizedStringEditor>();
            var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
            var stylesheet = Resources.Load<StyleSheet>("Stylesheets/LocalizedString");

            foreach (var window in allWindows) {
                window.LoadStylesheets(utilityStylesheet, stylesheet);
            }
        }
    }
}