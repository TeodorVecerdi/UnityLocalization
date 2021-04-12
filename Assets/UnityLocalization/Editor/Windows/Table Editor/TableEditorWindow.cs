using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Utility;

namespace UnityLocalization {
    public class TableEditorWindow : EditorWindow {
        [SerializeReference] private LocalizationSettings settings;
        private VisualElement tabContainer;
        private VisualElement tabContents;
        [SerializeField] private int activeTabIndex;
        private List<Tab> tabs;
        private bool deferStylesheetLoading;
        private bool stylesheetsLoaded;

        private void OnEnable() {
        }

        private void CreateGUI() {
            if (!stylesheetsLoaded) {
                InitializeWindow(this);
                OnEnable();
            }

            if (settings == null) {
                Close();
                return;
            }

            if (deferStylesheetLoading) {
                var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
                var stylesheet = Resources.Load<StyleSheet>("Stylesheets/TableEditorWindow");
                rootVisualElement.styleSheets.Add(utilityStylesheet);
                rootVisualElement.styleSheets.Add(stylesheet);
                deferStylesheetLoading = false;
                stylesheetsLoaded = true;
            }

            Debug.Log("CreateGUI called");

            tabs = new List<Tab>();
            tabContainer = new ScrollView(ScrollViewMode.Horizontal) {name = "TabContainer"};
            tabContents = new VisualElement {name = "TabContents"};

            var tables = settings.Tables;
            var anyTable = tables.Count > 0;
            if (anyTable) {
                foreach (var table in tables) {
                    tabs.Add(tabContainer.AddGet(new Tab(table.TableName)).Do(tab => { tab.Clicked += () => TabClicked(tab); }));
                }
            } else {
                tabContainer.AddToClassList("no-tables");
                tabContents.AddToClassList("no-tables");
                tabContents.AddGet<Label>("NoTablesLabel").Do(label => label.text = "No tables");
                tabContents.AddGet<Button>("NoTablesButton", "large").Do(button => {
                    button.clicked += CreateTable;
                    button.text = "Create Table";
                });
            }

            if (anyTable) {
                if (activeTabIndex >= 0 && activeTabIndex < tabs.Count) {
                    tabs[activeTabIndex].AddToClassList("active");
                    LoadTabContent(tabs[activeTabIndex]);
                } else TabClicked(tabs[0]);
            }

            rootVisualElement.Add(tabContainer);
            rootVisualElement.Add(tabContents);
        }

        private void CreateTable() {
            RecreateGUI();
        }

        private void TabClicked(Tab tab) {
            if (tabs[activeTabIndex] == tab) return;

            if (activeTabIndex < tabs.Count && activeTabIndex >= 0)
                tabs[activeTabIndex].RemoveFromClassList("active");
            tab.AddToClassList("active");
            activeTabIndex = tabs.IndexOf(tab);

            LoadTabContent(tab);
        }

        private void LoadTabContent(Tab tab) {
            tabContents.Clear();
            tabContents.AddGet<Label>("Contents", "contents").Do(label => { label.text = tab.TabName; });
        }

        private void RecreateGUI() {
            rootVisualElement.Clear();
            CreateGUI();
        }
        
        public static void Display(LocalizationSettings settings) {
            var window = FindMatching(settings);
            if (window == null) {
                window = CreateInstance<TableEditorWindow>();
                window.settings = settings;
                window.titleContent = new GUIContent($"Table Editor - {settings.name}");
            }

            window.Focus();
            window.Show();
            InitializeWindow(window);
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void Initialize() {
            if (!HasOpenInstances<TableEditorWindow>()) return;

            var allWindows = Resources.FindObjectsOfTypeAll<TableEditorWindow>();
            var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
            var stylesheet = Resources.Load<StyleSheet>("Stylesheets/TableEditorWindow");

            foreach (var window in allWindows) {
                InitializeWindow(window, utilityStylesheet, stylesheet);
            }
        }

        private static void InitializeWindow(TableEditorWindow window, StyleSheet utility = null, StyleSheet style = null) {
            utility ??= Resources.Load<StyleSheet>("Stylesheets/Utility");
            style ??= Resources.Load<StyleSheet>("Stylesheets/TableEditorWindow");
            try {
                window.rootVisualElement.styleSheets.Add(utility);
                window.rootVisualElement.styleSheets.Add(style);
                window.stylesheetsLoaded = true;
            } catch {
                window.deferStylesheetLoading = true;
            }
        }

        internal static TableEditorWindow FindMatching(LocalizationSettings settings) {
            return Resources.FindObjectsOfTypeAll<TableEditorWindow>().FirstOrDefault(editorWindow => editorWindow.settings == settings);
        }
    }
}