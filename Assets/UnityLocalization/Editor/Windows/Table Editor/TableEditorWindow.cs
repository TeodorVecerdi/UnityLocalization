using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Utility;

namespace UnityLocalization {
    public class TableEditorWindow : EditorWindow {
        [SerializeReference] internal LocalizationSettings settings;
        private VisualElement topContainer;
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
                this.AddStylesheet(utilityStylesheet);
                this.AddStylesheet(stylesheet);
                deferStylesheetLoading = false;
                stylesheetsLoaded = true;
            }


            var tables = settings.Tables;
            var anyTable = tables.Count > 0;
            if (!anyTable) {
                tabContents = new VisualElement {name = "TabContents"};
                tabContents.AddToClassList("no-tables");
                tabContents.AddGet<Label>("NoTablesLabel").Do(label => label.text = "No tables");
                tabContents.AddGet<Button>("NoTablesButton", "large").Do(button => {
                    button.clicked += CreateTable;
                    button.text = "Create Table";
                });
                rootVisualElement.Add(tabContents);
                return;
            }
            
            tabs = new List<Tab>();
            tabContents = new VisualElement {name = "TabContents"};
            topContainer = new VisualElement {name = "TopContainer"};
            tabContainer = new ScrollView(ScrollViewMode.Horizontal) {name = "TabContainer"};
            foreach (var table in tables) {
                tabs.Add(tabContainer.AddGet(new Tab(table.TableName)).Do(tab => { tab.Clicked += () => TabClicked(tab); }));
            }
            if (activeTabIndex >= 0 && activeTabIndex < tabs.Count) {
                tabs[activeTabIndex].AddToClassList("active");
                LoadTabContent(tabs[activeTabIndex]);
            } else {
                activeTabIndex = 0;
                tabs[activeTabIndex].AddToClassList("active");
                LoadTabContent(tabs[activeTabIndex]);
            }

            var createTableButton = new Button(CreateTable) {name = "CreateTableButton", tooltip = "Create table"};
            createTableButton.AddGet<VisualElement>("CreateTableButton-Image");
            
            topContainer.Add(tabContainer);
            topContainer.Add(createTableButton);

            rootVisualElement.Add(topContainer);
            rootVisualElement.Add(tabContents);
        }

        private void CreateTable() {
            CreateTableWindow.Display(Event.current.mousePosition + position.position + Vector2.up * 20, settings, () => {
                TabClicked(tabs[tabs.Count-1]);
            });
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
            tabContents.Add(new TableCell("Hello, World!", true));
            // var header = VisualElementFactory.TableRow("TableEditor-TableHeader", classNames: null, );
            tabContents.AddGet<Label>("Contents", "contents").Do(label => { label.text = tab.TabName; });
        }

        internal void OnTablesDirty() {
            RecreateGUI();
        }

        private void RecreateGUI() {
            rootVisualElement.Clear();
            CreateGUI();
        }
        
        public static void Display(LocalizationSettings settings) {
            var window = Utils.FindMatching<TableEditorWindow>(editorWindow => editorWindow.settings == settings);
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
                window.AddStylesheet(utility);
                window.AddStylesheet(style);
                window.stylesheetsLoaded = true;
            } catch {
                window.deferStylesheetLoading = true;
            }
        }
    }
}