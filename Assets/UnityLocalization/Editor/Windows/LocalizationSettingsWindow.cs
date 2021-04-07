using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
                if (!window.rootVisualElement.styleSheets.Contains(stylesheet)) window.rootVisualElement.styleSheets.Add(stylesheet);
            } catch (Exception e) {
                Debug.LogException(e);
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
        [SerializeField] private Locale selectedLocale;

#pragma warning disable 0414 // used implicitly
        [SerializeField] private bool localeFoldoutExpanded = true;
#pragma warning restore 0414
        private void CreateGUI() {
            var header = new Label("Localization Settings");
            header.AddToClassList("header");

            rootVisualElement.Add(header);
            rootVisualElement.Add(activeSettingsEditor.CreateInspectorGUI());

            var localesFoldout = new Foldout() {text = "Locales"};
            localesFoldout.BindProperty(new SerializedObject(this).FindProperty("localeFoldoutExpanded"));
            localesFoldout.AddToClassList("section-foldout");
            localesFoldout.Add(new IMGUIContainer(OnLocalesGUI));
            rootVisualElement.Add(localesFoldout); 
        }

        private void OnLocalesGUI() {
            var settings = activeSettings.ActiveSettings;
            if (settings == null) return;
            if (!stylesLoaded) LoadStyles();

            var defaultLocale = settings.DefaultLocale;

            // Draw default locale
            GUILayout.BeginHorizontal(style_container);
            GUILayout.Label("Default Locale: ", style_defaultLocale, GUILayout.ExpandWidth(false));
            if (defaultLocale == null || string.IsNullOrEmpty(defaultLocale.LocaleCode)) {
                GUILayout.Label("No default locale selected", style_defaultLocale_noLocale);
            } else {
                GUILayout.Label($"{defaultLocale.EnglishName} ({defaultLocale.NativeName})", style_defaultLocale);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginVertical(style_containerNoMarginTop);

            EditorGUI.BeginChangeCheck();
            localeSearchQuery = GUILayout.TextField(localeSearchQuery);
            if (EditorGUI.EndChangeCheck()) UpdateFilter();
            if (string.IsNullOrEmpty(localeSearchQuery)) {
                var guiColor = GUI.color;
                GUI.color = Color.grey;
                var lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x += 4;
                lastRect.width -= 4;
                EditorGUI.LabelField(lastRect, "Search locales");
                GUI.color = guiColor;
            }

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

            GUI.enabled = selectedLocale != null;
            if (settings.DefaultLocale.Equals(selectedLocale)) GUI.enabled = false;
            if (GUILayout.Button("Remove Selected", GUILayout.Height(30))) {
                Utils.RecordChange(settings, "Removed locale");
                settings.RemoveLocale(selectedLocale);
                Utils.SaveChanges();
                UpdateFilter();
                selectedLocale = null;
            }

            if (GUILayout.Button("Make Selected Default", GUILayout.Height(30))) {
                Utils.RecordChange(settings, "Set default locale");
                settings.SetDefaultLocale(selectedLocale);
                Utils.SaveChanges();
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();

            // Draw locale search box
            // Draw locales list
        }

        private void ActiveSettingsChanged() {
            if (activeSettings.ActiveSettings != null) {
                filteredLocales = activeSettings.ActiveSettings.Locales;
                UpdateFilter();
            }
        }

        private void UndoRedoPerformed() {
            UpdateFilter();
            EditorUtility.SetDirty(this);
        }

        public void UpdateFilter() {
            if (activeSettings.ActiveSettings == null) {
                filteredLocales = new List<Locale>();
                return;
            }

            if (string.IsNullOrEmpty(localeSearchQuery)) {
                filteredLocales = activeSettings.ActiveSettings.Locales.ToList();
                return;
            }

            filteredLocales = activeSettings.ActiveSettings.Locales.Where(locale => locale.EnglishName.ToLowerInvariant().Contains(localeSearchQuery.ToLowerInvariant()) ||
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
            style_localeSelectionEntryLabel = GUIStyleDescription.Combine(baseLabel, !textCenter);
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
        private static GUIStyleDescription toolbarButton;
        private static readonly GUIStyleDescription textRed = new GUIStyleDescription {TextColor = Color.red};
        private static readonly GUIStyleDescription marginLeft8 = new GUIStyleDescription {Margin = new Int4 {left = 8}};
        private static readonly GUIStyleDescription paddingTop0 = new GUIStyleDescription {Padding = new Int4 {top = 0}};
        private static readonly GUIStyleDescription padding8866 = new GUIStyleDescription {Padding = new Int4 {left = 10, right = 10, bottom = 6, top = 6}};
        private static readonly GUIStyleDescription textCenter = new GUIStyleDescription {Alignment = TextAnchor.MiddleCenter};
    }
}