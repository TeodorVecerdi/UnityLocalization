using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Runtime;
using UnityLocalization.Shared;

namespace UnityLocalization {
    [CustomEditor(typeof(LocalizedString))]
    public class LocalizedStringEditor : Editor {
        // Properties //
        private SerializedProperty setterProperty;
        private SerializedProperty keyProperty;

        // VisualElements //
        private VisualElement rootElement;
        private VisualElement activeSettingsWarning;
        private VisualElement rootSettingsContainer;
        private VisualElement localizationSettingsContainer;
        private VisualElement previewSettings;
        private Button previewToggleButton;
        private List<Button> previewButtons;

        // Other //
        private LocalizedString localizedString;

        private void OnEnable() {
            AcquireProperties();
            localizedString = target as LocalizedString;
            localizedString!.Settings = ActiveLocalizationSettings.Load().ActiveSettings;

            if (rootElement != null) UpdateWarningVisibility();
        }

        public override VisualElement CreateInspectorGUI() {
            rootElement = new VisualElement {name = "LocalizedString"};

            CreateGUI();
            //4.  Preview?

            return rootElement;
        }

        private void CreateGUI() {
            AcquireProperties();
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

            rootSettingsContainer = rootElement.AddGet<VisualElement>("SettingsContainer");

            var settings = localizedString.Settings;
            if (settings != null) {
                CreateSettingsGUI(settings);
            }

            UpdateWarningVisibility();
            UpdateKeyVisibility();
        }

        private void CreateSettingsGUI(LocalizationSettings settings) {
            var tableChoices = settings.TableGuids.Prepend(Guid.Empty.ToString()).ToList();
            var defaultIndex = settings.GuidToTableIndex(localizedString.TableGuid) + 1;
            var tableDropdown = new PopupField<string>("Table", tableChoices, defaultIndex, guid => settings.GuidToTableName(guid, "None"),
                                                       guid => settings.GuidToTableName(guid, "None"));
            tableDropdown.name = "TableDropdown";
            tableDropdown.RegisterValueChangedCallback(evt => {
                var table = settings.GuidToTable(evt.newValue);
                localizedString.TableGuid = evt.newValue;
                localizedString.Table = table;
                UpdateKeyVisibility();
            });
            rootSettingsContainer.Add(tableDropdown);
            rootSettingsContainer.Add(localizationSettingsContainer = new VisualElement {name = "LocalizationSettingsContainer"});
            localizationSettingsContainer.AddGet<VisualElement>("KeyContainer").Do(keyContainer => {
                keyContainer.AddGet(new TextField("Key") {name = "LocalizationKey", isReadOnly = true}).Do(self => {
                    self.SetEnabled(false);
                    self.BindProperty(keyProperty);
                });
                keyContainer.AddGet(new Button(OnSelectKey) {text = "Select Key", name = "SelectKeyButton"});
            });
            localizationSettingsContainer.Add(new PropertyField(setterProperty).Do(field => {
                field.RegisterValueChangeCallback(evt => {
                    var count = localizedString.Setter.GetPersistentEventCount();
                    for (var i = 0; i < count; i++) {
                        localizedString.Setter.SetPersistentListenerState(i, UnityEventCallState.EditorAndRuntime);
                    }
                });
            }));

            // Preview section
            if (localizedString.PreviewLocaleIndex == -1) localizedString.PreviewLocaleIndex = settings.DefaultLocaleIndex();

            var previewContainer = localizationSettingsContainer.AddGet<VisualElement>("PreviewContainer");
            previewContainer.AddGet(previewToggleButton = new Button(TogglePreview) {name = "TogglePreviewButton", text = "Preview"})
                            .SetClass(localizedString.IsPreviewActive, "active");

            previewButtons = new List<Button>();
            previewSettings = previewContainer.AddGet<VisualElement>("PreviewSettings").SetClass(!localizedString.IsPreviewActive, "hidden");
            previewSettings.AddGet(new ScrollView(ScrollViewMode.Horizontal) {name = "PreviewLocalesContainer"}).Do(container => {
                for (var i = 0; i < settings.Locales.Count; i++) {
                    var locale = settings.Locales[i];
                    var localeIndex = i;
                    previewButtons.Add(container.AddGet(
                                           new Button(() => SetPreviewLocale(localeIndex)) {text = $"{locale.LocaleCode}"}
                                               .SetClass(localeIndex == localizedString.PreviewLocaleIndex, "active")
                                       )
                    );
                }
            });
            if (localizedString.IsPreviewActive) UpdatePreview();
        }

        private void TogglePreview() {
            localizedString.IsPreviewActive = !localizedString.IsPreviewActive;
            previewSettings.SetClass(!localizedString.IsPreviewActive, "hidden");
            previewToggleButton.SetClass(localizedString.IsPreviewActive, "active");
            if (localizedString.IsPreviewActive) UpdatePreview();
        }

        private void UpdatePreview() {
            localizedString.Translate(localizedString.PreviewLocaleIndex);
            var count = localizedString.Setter.GetPersistentEventCount();
            for (var i = 0; i < count; i++) {
                EditorUtility.SetDirty(localizedString.Setter.GetPersistentTarget(i));
            }
        }

        private void SetPreviewLocale(int index) {
            previewButtons[localizedString.PreviewLocaleIndex].SetClass(false, "active");
            localizedString.PreviewLocaleIndex = index;
            previewButtons[localizedString.PreviewLocaleIndex].SetClass(true, "active");
            UpdatePreview();
        }

        private void AcquireProperties() {
            setterProperty = serializedObject.FindProperty("Setter");
            keyProperty = serializedObject.FindProperty("Key");
        }

        private void UpdateWarningVisibility() {
            var showSettings = localizedString.Settings != null;
            activeSettingsWarning.SetClass(showSettings, "hidden");
            rootSettingsContainer.SetClass(!showSettings, "hidden");
        }

        private void UpdateKeyVisibility() {
            var show = localizedString.Table != null;
            localizationSettingsContainer.SetClass(!show, "hidden");
        }

        public void OnSettingsDirty(LocalizationSettings newSettings) {
            localizedString.Settings = newSettings;
            rootElement.Clear();
            CreateGUI();
        }

        private void OnSelectKey() {
            SelectKeyWindow.Display(this, localizedString.Settings, localizedString.Table);
        }

        public void OnKeyDirty(int newKeyIndex) {
            SetKeyIndex(newKeyIndex);
            // TODO: Update view / preview etc
        }

        private void SetKeyIndex(int index) {
            var entry = localizedString.Table.Entries[index];
            var entryGuid = localizedString.Table.EntryGuids[index];

            localizedString.Key = entry.Key;
            localizedString.KeyGuid = entryGuid;
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