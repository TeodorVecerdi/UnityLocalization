using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Utility;

namespace UnityLocalization {
    public class LocalizationSettingsWindow : EditorWindow {
        private ActiveLocalizationSettings activeSettings;
        private ActiveLocalizationSettingsEditor activeSettingsEditor;

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void Initialize() {
            if (!HasOpenInstances<LocalizationSettingsWindow>()) return;

            var window = GetWindow<LocalizationSettingsWindow>();
            var stylesheet = Resources.Load<StyleSheet>("Stylesheets/SettingsWindow");
            try {
                window.rootVisualElement.styleSheets.Add(stylesheet);
            } catch {
                window.deferStylesheetLoading = true;
            }
        }

        [MenuItem("Localization/Settings Editor")]
        private static void ShowWindow() {
            var window = GetWindow<LocalizationSettingsWindow>(typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            window.titleContent = new GUIContent("Localization Settings");
            window.Show();

            Initialize();
        }

        private void OnEnable() {
            activeSettings = ActiveLocalizationSettings.instance;
            if (EditorPrefs.HasKey(Constants.ACTIVE_SETTINGS_PREFS_KEY)) {
                activeSettings.ActiveSettings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(EditorPrefs.GetString(Constants.ACTIVE_SETTINGS_PREFS_KEY));
            }

            activeSettingsEditor = Editor.CreateEditor(activeSettings) as ActiveLocalizationSettingsEditor;
            activeSettingsEditor.OnActiveSettingsChanged += ActiveSettingsChanged;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            UpdateFilter();
        }

        private void OnDisable() {
            activeSettingsEditor.OnActiveSettingsChanged -= ActiveSettingsChanged;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        [SerializeField] private List<Locale> filteredLocales;
        [SerializeField] private string localeSearchQuery;
        [SerializeField] private Vector2 localeSearchScroll;
        [SerializeReference] private Locale selectedLocale;
        private bool localeQueryChanged;
#pragma warning disable 0414 // reason: used implicitly 
        [SerializeField] private bool localeFoldoutClosed = true;
#pragma warning restore 0414
        private bool deferStylesheetLoading;

        private VisualElement settingsContainer;
        private Label defaultLocaleLabel;

        private void CreateGUI() {
            if (activeSettings == null || activeSettingsEditor == null) {
                Initialize();
                OnEnable();
            }

            if (deferStylesheetLoading) {
                var stylesheet = Resources.Load<StyleSheet>("Stylesheets/SettingsWindow");
                rootVisualElement.styleSheets.Add(stylesheet);
                deferStylesheetLoading = false;
            }

            var header = new Label("Localization Settings") {name = "LocalizationTitle"};
            header.AddToClassList("header");

            rootVisualElement.Add(header);
            rootVisualElement.Add(activeSettingsEditor.CreateInspectorGUI());

            settingsContainer = new VisualElement {name = "SettingsContainer"};

            var localesFoldout = new Foldout {text = "Locales", name = "LocaleFoldout"};
            localesFoldout.AddToClassList("firstOfType");
            localesFoldout.AddToClassList("themeLocale");
            localesFoldout.AddToClassList("section-foldout");
            if (!localeFoldoutClosed) localesFoldout.AddToClassList("foldout-open");
            localesFoldout.BindProperty(new SerializedObject(this).FindProperty("localeFoldoutClosed"));
            localesFoldout.RegisterValueChangedCallback(evt => {
                if (evt.newValue) localesFoldout.AddToClassList("foldout-open");
                else localesFoldout.RemoveFromClassList("foldout-open");
            });
            defaultLocaleLabel = new Label {name = "DefaultLocale"};
            UpdateDefaultLocaleLabel();

            var localesSearchField = new ToolbarSearchField {name = "LocalesSearchField", tooltip = "Search locales", value = localeSearchQuery};
            localesSearchField.AddToClassList("searchField");
            localesSearchField.AddToClassList("themeLocale");
            var localesSearchPlaceholder = new Label("Search locales") {name = "LocalesSearchPlaceholder"};
            if (!string.IsNullOrEmpty(localeSearchQuery)) localesSearchPlaceholder.AddToClassList("hidden");
            var localesTextField = localesSearchField.Q<TextField>();
            localesTextField[0].Add(localesSearchPlaceholder);
            localesTextField.BindProperty(new SerializedObject(this).FindProperty("localeSearchQuery"));
            localesTextField.RegisterValueChangedCallback(evt => {
                if (string.IsNullOrEmpty(evt.newValue)) localesSearchPlaceholder.RemoveFromClassList("hidden");
                else localesSearchPlaceholder.AddToClassList("hidden");
                localeQueryChanged = true;
            });
            localesFoldout.Add(defaultLocaleLabel);
            localesFoldout.Add(localesSearchField);
            localesFoldout.Add(new IMGUIContainer(OnLocalesGUI) {name = "LocaleEditor"});
            var checkmark = localesFoldout.Q<VisualElement>(className: "unity-toggle__checkmark");
            var opacity = checkmark.style.opacity;
            opacity.value = 5f;
            checkmark.style.opacity = opacity;

            var otherFoldout = new Foldout {text = "Other foldout", name = "OtherFoldout"};
            otherFoldout.AddToClassList("section-foldout");

            settingsContainer.Add(localesFoldout);
            settingsContainer.Add(otherFoldout);
            rootVisualElement.Add(settingsContainer);
        }

        private void OnLocalesGUI() {
            var settings = activeSettings.ActiveSettings;
            if (settings == null) return;
            if (!stylesLoaded) LoadStyles();

            if (localeQueryChanged) {
                UpdateFilter();
                localeQueryChanged = false;
            }

            GUILayout.BeginVertical(style_containerNoMarginTop);
            localeSearchScroll = GUILayout.BeginScrollView(localeSearchScroll,
                                                           filteredLocales.Count * 40 >= 200 ? new[] {GUILayout.MaxHeight(200), GUILayout.MinHeight(0)} : new GUILayoutOption[0]);
            selectedLocale = GUILayoutExtras.Selection(selectedLocale, filteredLocales, (val, isSelected, index) => {
                var back = GUI.backgroundColor;
                GUI.backgroundColor = index % 2 == 0 ? new Color(0.8f, 0.8f, 0.8f) : Color.white;
                GUI.enabled = !isSelected;
                var clicked = GUILayout.Button("", style_localeSelectionEntryButton, GUILayout.MinHeight(40));
                var lastRect = GUILayoutUtility.GetLastRect();
                GUI.enabled = true;
                GUI.Label(lastRect, $"{val.EnglishName} ({val.NativeName}){(val.Equals(settings.DefaultLocale) ? " - <b>Default</b>" : "")}", style_localeSelectionEntryLabel);
                GUI.backgroundColor = back;
                return clicked;
            });

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal(style_containerNoMarginTop);
            if (GUILayout.Button("Add Locale", GUILayout.Height(30))) {
                AddLocaleWindow.Display(this, activeSettings.ActiveSettings);
            }

            if (selectedLocale == null || settings.DefaultLocale.Equals(selectedLocale)) GUI.enabled = false;
            if (GUILayout.Button("Remove Selected", GUILayout.Height(30))) {
                Utils.RecordChange(settings, "Removed locale");
                settings.RemoveLocale(selectedLocale);
                Utils.SaveChanges();
                selectedLocale = null;
                UpdateFilter();
            }

            if (GUILayout.Button("Make Selected Default", GUILayout.Height(30))) {
                Utils.RecordChange(settings, "Set default locale");
                settings.SetDefaultLocale(selectedLocale);
                Utils.SaveChanges();
                UpdateDefaultLocaleLabel();
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private void ActiveSettingsChanged(LocalizationSettings newSettings) {
            if (newSettings != null) {
                UpdateFilter(newSettings);
                settingsContainer.RemoveFromClassList("hidden");
                return;
            }

            settingsContainer.AddToClassList("hidden");
        }

        private void UndoRedoPerformed() {
            UpdateFilter();
            EditorUtility.SetDirty(this);
        }

        private void UpdateDefaultLocaleLabel() {
            var defaultLocale = activeSettings == null || activeSettings.ActiveSettings == null ? null : activeSettings.ActiveSettings.DefaultLocale;

            if (defaultLocale == null || string.IsNullOrEmpty(defaultLocale.LocaleCode)) {
                defaultLocaleLabel.text = "Default Locale: No default locale selected";
                defaultLocaleLabel.AddToClassList("no-locale");
            } else {
                defaultLocaleLabel.text = $"Default Locale: {defaultLocale.EnglishName} ({defaultLocale.NativeName})";
                defaultLocaleLabel.RemoveFromClassList("no-locale");
            }
        }

        public void UpdateFilter(LocalizationSettings newSettings = null) {
            var settings = newSettings != null ? newSettings : activeSettings.ActiveSettings;
            if (settings == null) {
                filteredLocales = new List<Locale>();
                return;
            }

            if (string.IsNullOrEmpty(localeSearchQuery)) {
                filteredLocales = settings.Locales.ToList();
                return;
            }

            filteredLocales = settings.Locales.Where(locale => locale.EnglishName.ToLowerInvariant().Contains(localeSearchQuery.ToLowerInvariant()) ||
                                                               locale.NativeName.ToLowerInvariant().Contains(localeSearchQuery.ToLowerInvariant()) ||
                                                               locale.LocaleCode.ToLowerInvariant().Contains(localeSearchQuery.ToLowerInvariant()))
                                      .ToList();
        }

        private static void LoadStyles() {
            baseLabel = new GUIStyleDescription {
                Alignment = EditorStyles.label.alignment, Font = EditorStyles.label.font, FontSize = EditorStyles.label.fontSize, Clipping = EditorStyles.label.clipping,
                FontStyle = EditorStyles.label.fontStyle, WordWrap = EditorStyles.label.wordWrap, TextColor = EditorStyles.label.normal.textColor
            };

            style_defaultLocale_noLocale = GUIStyleDescription.Combine(baseLabel, !textRed, marginLeft8);
            style_defaultLocale = GUIStyleDescription.Combine(baseLabel);
            style_container = GUIStyleDescription.Combine(padding8866);
            style_containerNoMarginTop = GUIStyleDescription.Combine(padding8866, !paddingTop0);
            style_redCenterMiddleLabel = GUIStyleDescription.Combine(baseLabel, !textRed, !textCenter);
            style_localeSelectionEntryLabel = GUIStyleDescription.Combine(baseLabel, !richText, !textCenter);
            style_localeSelectionEntryButton = new GUIStyle(EditorStyles.toolbarButton) {fixedHeight = 0};
            stylesLoaded = true;
        }

        private static bool stylesLoaded;
        private static GUIStyle style_defaultLocale_noLocale;
        private static GUIStyle style_defaultLocale;
        private static GUIStyle style_container;
        private static GUIStyle style_containerNoMarginTop;
        private static GUIStyle style_redCenterMiddleLabel;
        private static GUIStyle style_localeSelectionEntryLabel;
        private static GUIStyle style_localeSelectionEntryButton;

        private static GUIStyleDescription baseLabel;
        private static readonly GUIStyleDescription textRed = new GUIStyleDescription {TextColor = Color.red};
        private static readonly GUIStyleDescription richText = new GUIStyleDescription {RichText = true};
        private static readonly GUIStyleDescription marginLeft8 = new GUIStyleDescription {Margin = new Int4 {left = 8}};
        private static readonly GUIStyleDescription paddingTop0 = new GUIStyleDescription {Padding = new Int4 {top = 0}};
        private static readonly GUIStyleDescription padding8866 = new GUIStyleDescription {Padding = new Int4 {left = 8, right = 10, bottom = 6, top = 6}};
        private static readonly GUIStyleDescription textCenter = new GUIStyleDescription {Alignment = TextAnchor.MiddleCenter};
    }
}