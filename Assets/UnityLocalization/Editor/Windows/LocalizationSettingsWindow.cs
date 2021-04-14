using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Shared;
using UnityLocalization.Utility;

namespace UnityLocalization {
    public class LocalizationSettingsWindow : EditorWindow {
        private ActiveLocalizationSettings activeSettings;
        private ActiveLocalizationSettingsEditor activeSettingsEditor;

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void Initialize() {
            var window = Utils.Find<LocalizationSettingsWindow>();
            if(window == null) return;
            
            var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
            var stylesheet = Resources.Load<StyleSheet>("Stylesheets/SettingsWindow");
            try {
                window.AddStylesheet(utilityStylesheet);
                window.AddStylesheet(stylesheet);
            } catch {
                window.deferStylesheetLoading = true;
            }
        }

        [MenuItem("Localization/Settings Editor")]
        public static void ShowWindow() {
            var window = GetWindow<LocalizationSettingsWindow>(typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            window.titleContent = new GUIContent("Localization Settings");
            window.Show();

            Initialize();
        }

        private void OnEnable() {
            activeSettings = ActiveLocalizationSettings.Load();

            activeSettingsEditor = Editor.CreateEditor(activeSettings) as ActiveLocalizationSettingsEditor;
            activeSettingsEditor!.OnActiveSettingsChanged += ActiveSettingsChanged;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            UpdateFilter();
            UpdateTableFilter();
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

        [SerializeField] private List<LocalizationTable> filteredTables;
        [SerializeField] private string tableSearchQuery;
        [SerializeField] private Vector2 tableSearchScroll;
        [SerializeReference] private LocalizationTable selectedTable;
        private bool tableQueryChanged;

#pragma warning disable 0414 // reason: used implicitly 
        [SerializeField] private bool localeFoldoutClosed;
        [SerializeField] private bool tablesFoldoutClosed;
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
                var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
                var stylesheet = Resources.Load<StyleSheet>("Stylesheets/SettingsWindow");
                this.AddStylesheet(utilityStylesheet);
                this.AddStylesheet(stylesheet);
                deferStylesheetLoading = false;
            }

            var header = new Label("Localization Settings") {name = "LocalizationTitle"};
            header.AddToClassList("header");

            rootVisualElement.Add(header);
            rootVisualElement.Add(activeSettingsEditor.CreateInspectorGUI());

            settingsContainer = new VisualElement {name = "SettingsContainer"};

            var localesFoldout = CreateLocalesFoldout();
            var tablesFoldout = CreateTablesFoldout();

            settingsContainer.Add(localesFoldout);
            settingsContainer.Add(tablesFoldout);
            rootVisualElement.Add(settingsContainer);
        }

        private Foldout CreateLocalesFoldout() {
            var localesFoldout = Factory.Foldout("LocaleFoldout", "Locales", nameof(localeFoldoutClosed), this, localeFoldoutClosed, "themeLocale", "firstOfType");
            defaultLocaleLabel = new Label {name = "DefaultLocale"};
            UpdateDefaultLocaleLabel();

            var localesSearchField = Factory
                                     .Create<ToolbarSearchField>("LocalesSearchField", "searchField", "themeLocale")
                                     .Q<TextField>()
                                     .Do(self => {
                                         self.value = localeSearchQuery;

                                         var localesSearchPlaceholder = self[0].AddGet<Label>("LocalesSearchPlaceholder");
                                         localesSearchPlaceholder.text = "Search locales";
                                         if (!string.IsNullOrEmpty(localeSearchQuery)) localesSearchPlaceholder.AddToClassList("hidden");

                                         self.BindProperty(new SerializedObject(this).FindProperty(nameof(localeSearchQuery)));
                                         self.RegisterValueChangedCallback(evt => {
                                             if (string.IsNullOrEmpty(evt.newValue)) localesSearchPlaceholder.RemoveFromClassList("hidden");
                                             else localesSearchPlaceholder.AddToClassList("hidden");
                                             localeQueryChanged = true;
                                         });
                                     }).NthParent<ToolbarSearchField>(1);

            localesFoldout.Add(defaultLocaleLabel);
            localesFoldout.Add(localesSearchField);
            localesFoldout.Add(new IMGUIContainer(OnLocalesGUI) {name = "LocaleEditor"});
            var checkmark = localesFoldout.Q<VisualElement>(className: "unity-toggle__checkmark");
            var opacity = checkmark.style.opacity;
            opacity.value = 5f;
            checkmark.style.opacity = opacity;
            return localesFoldout;
        }

        private Foldout CreateTablesFoldout() {
            var tablesFoldout = Factory.Foldout("TableFoldout", "Tables", nameof(tablesFoldoutClosed), this, tablesFoldoutClosed, "themeTable");
            var createTableButton = Factory.Create<Button>("CreateTable", "large").Do(self => {
                self.text = "Create New Table";
                self.clicked += () => CreateTableWindow.Display(Event.current.mousePosition + position.position + Vector2.up * 20, activeSettings.ActiveSettings);
            });
            var searchField = Factory
                              .Create<ToolbarSearchField>("TablesSearchField", "searchField", "themeTable")
                              .Q<TextField>()
                              .Do(self => {
                                  self.value = localeSearchQuery;

                                  var placeholder = self[0].AddGet<Label>("TablesSearchPlaceholder", "placeholderLabel");
                                  placeholder.text = "Search tables";
                                  if (!string.IsNullOrEmpty(tableSearchQuery)) placeholder.AddToClassList("hidden");

                                  self.BindProperty(new SerializedObject(this).FindProperty(nameof(tableSearchQuery)));
                                  self.RegisterValueChangedCallback(evt => {
                                      if (string.IsNullOrEmpty(evt.newValue)) placeholder.RemoveFromClassList("hidden");
                                      else placeholder.AddToClassList("hidden");
                                      tableQueryChanged = true;
                                  });
                              }).NthParent<ToolbarSearchField>(1);

            tablesFoldout.Add(createTableButton);
            tablesFoldout.Add(searchField);
            tablesFoldout.Add(new IMGUIContainer(OnTablesGUI) {name = "TableEditor"});

            return tablesFoldout;
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
                Utils.DirtyLocales(settings);
            }

            if (GUILayout.Button("Make Selected Default", GUILayout.Height(30))) {
                Utils.RecordChange(settings, "Set default locale");
                settings.SetDefaultLocale(selectedLocale);
                Utils.SaveChanges();
                UpdateDefaultLocaleLabel();
                Utils.DirtyLocales(settings);
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private void OnTablesGUI() {
            var settings = activeSettings.ActiveSettings;
            if (settings == null) return;
            if (!stylesLoaded) LoadStyles();

            if (tableQueryChanged) {
                UpdateTableFilter();
                tableQueryChanged = false;
            }

            GUILayout.BeginVertical(style_containerNoMarginTop);
            tableSearchScroll = GUILayout.BeginScrollView(tableSearchScroll,
                                                          filteredTables.Count * 40 >= 200 ? new[] {GUILayout.MaxHeight(200), GUILayout.MinHeight(0)} : new GUILayoutOption[0]);
            selectedTable = GUILayoutExtras.Selection(selectedTable, filteredTables, (val, isSelected, index) => {
                var back = GUI.backgroundColor;
                GUI.backgroundColor = index % 2 == 0 ? new Color(0.8f, 0.8f, 0.8f) : Color.white;
                GUI.enabled = !isSelected;
                var clicked = GUILayout.Button("", style_localeSelectionEntryButton, GUILayout.MinHeight(40));
                var lastRect = GUILayoutUtility.GetLastRect();
                GUI.enabled = true;
                GUI.Label(lastRect, $"{val.TableName}", style_localeSelectionEntryLabel);
                GUI.backgroundColor = back;
                return clicked;
            });

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal(style_containerNoMarginTop);

            if (GUILayout.Button("Open Table Editor", GUILayout.Height(30))) {
                TableEditorWindow.Display(settings);
            }

            if (selectedTable == null) GUI.enabled = false;
            if (GUILayout.Button("Remove Selected", GUILayout.Height(30))) {
                if (EditorUtility.DisplayDialog("Confirm table removal", "This action cannot be undone. Are you sure you want to delete the table?", "Yes", "No")) {
                    settings.RemoveTable(selectedTable);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selectedTable));
                    Utils.SaveChanges();
                    selectedTable = null;
                    Utils.DirtyTables(settings);
                }
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        private void ActiveSettingsChanged(LocalizationSettings newSettings) {
            Utils.DirtySettings(newSettings);
            
            if (newSettings != null) {
                UpdateFilter(newSettings);
                UpdateTableFilter(newSettings);
                UpdateDefaultLocaleLabel(newSettings);
                settingsContainer.RemoveFromClassList("hidden");
                return;
            }

            settingsContainer.AddToClassList("hidden");
        }

        private void UndoRedoPerformed() {
            UpdateFilter();
            UpdateTableFilter();
            EditorUtility.SetDirty(this);
        }

        private void UpdateDefaultLocaleLabel(LocalizationSettings newSettings = null) {
            var settings = newSettings != null ? newSettings : activeSettings.ActiveSettings;
            var defaultLocale = settings == null ? null : settings.DefaultLocale;

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

        public void UpdateTableFilter(LocalizationSettings newSettings = null) {
            var settings = newSettings != null ? newSettings : activeSettings.ActiveSettings;
            if (settings == null) {
                filteredTables = new List<LocalizationTable>();
                return;
            }

            if (string.IsNullOrEmpty(tableSearchQuery)) {
                filteredTables = settings.Tables.ToList();
                return;
            }

            filteredTables = settings.Tables.Where(table => table.TableName.ToLowerInvariant().Contains(tableSearchQuery.ToLowerInvariant()) ||
                                                            tableSearchQuery.ToLowerInvariant().Contains(table.TableName.ToLowerInvariant()))
                                     .ToList();
        }

        internal void OnTablesDirty() {
            UpdateTableFilter();
        }

        internal void OnLocalesDirty() {
            UpdateFilter();
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