using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Shared;
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

        private VisualElement keyColumn;
        private List<VisualElement> localeColumns;
        private TableCell createEntry;

        private void CreateGUI() {
            if (!stylesheetsLoaded) {
                InitializeWindow(this);
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
                tabs.Add(tabContainer.AddGet(new Tab(table.TableName)).Do(tab => {
                    tab.Clicked += () => TabClicked(tab);
                    tab.DeleteClicked += () => DeleteTable(tab.userData as LocalizationTable);
                    tab.userData = table;
                }));
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
            CreateTableWindow.Display(Event.current.mousePosition + position.position + Vector2.up * 20, settings, () => { TabClicked(tabs[tabs.Count - 1]); });
        }

        private void DeleteTable(LocalizationTable table) {
            if (EditorUtility.DisplayDialog("Confirm table removal", "This action cannot be undone. Are you sure you want to delete the table?", "Yes", "No")) {
                settings.RemoveTable(table);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(table));
                Utils.SaveChanges();
                Utils.DirtyTables(settings);
            }
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
            if (settings == null) return;

            var table = tab.userData as LocalizationTable;
            var entries = table!.Entries;

            var scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            keyColumn = Factory.Create<VisualElement>(null, "table-col");
            keyColumn.AddGet(new TableCell("key", false)).AddToClassList("header");

            for (var i = 0; i < entries.Count; i++) {
                keyColumn.Add(MakeKeyCell(table, entries[i].Key, i));
            }

            createEntry = new TableCell("Add...", true) {name = "CreateEntryCell"};
            createEntry.OnValueChanged += CreateEntry;
            createEntry.OnBeginEdit += field => { field.value = ""; };
            createEntry.OnCancelEdit += field => { createEntry.Text = "Add..."; };
            keyColumn.Add(createEntry);

            var locales = settings.Locales;
            localeColumns = new List<VisualElement>(locales.Count);
            for (var i = 0; i < locales.Count; i++) {
                var col = i;
                var locale = locales[i];
                var isDefaultLocale = locale.Equals(settings.DefaultLocale);
                var localeColumn = Factory.Create<VisualElement>(null, "table-col");
                localeColumn.Add(new TableCell($"{locale.EnglishName}{(isDefaultLocale ? " - Default" : "")}", false).Do(cell => {
                    cell.AddToClassList("header");
                    if (isDefaultLocale) cell.AddToClassList("bold");
                }));
                for (var j = 0; j < entries.Count; j++) {
                    var row = j;
                    localeColumn.Add(new TableCell(entries[row].Values[col], true).Do(cell => {
                        cell.OnValueChanged += newValue => { table.UpdateLocalization(row, col, newValue); };
                    }));
                }

                // Add empty cell for Add entry cell
                localeColumn.Add(new TableCell("", false));
                localeColumns.Add(localeColumn);
            }

            var tableElement = new VisualElement {name = "Table"};
            tableElement.AddToClassList("table");
            tableElement.Add(keyColumn);
            foreach (var column in localeColumns) {
                tableElement.Add(column);
            }

            tableElement.Add(scrollView);
            scrollView.Add(tableElement);
            tabContents.Add(scrollView);
        }

        private void CreateEntry(string key) {
            createEntry.Text = "Add...";

            if (string.IsNullOrWhiteSpace(key)) {
                return;
            }

            var selectedTable = tabs[activeTabIndex].userData as LocalizationTable;
            if (selectedTable!.Entries.Any(entry => string.Equals(entry.Key, key, StringComparison.InvariantCulture)))
                return;

            CreateEntryImpl(selectedTable, key);
        }

        private void CreateEntryImpl(LocalizationTable table, string key) {
            // skipping header and "Add..." cell
            var index = keyColumn.childCount - 1;
            table.AddKey(key);
            keyColumn.Insert(index, MakeKeyCell(table, key, index - 1));
            for (var i = 0; i < localeColumns.Count; i++) {
                var col = i;
                localeColumns[i].Insert(index, new TableCell("", true).Do(cell => { cell.OnValueChanged += newValue => { table.UpdateLocalization(index - 1, col, newValue); }; }));
            }
        }

        private TableCell MakeKeyCell(LocalizationTable table, string key, int row) {
            return new TableCell(key, true).Do(cell => {
                cell.OnValueChanged += newKey => { table.UpdateKey(row, newKey); };
                cell.AddManipulator(new ContextualMenuManipulator(ctx => KeyContextMenu(ctx, table, row)));
            });
        }

        private void KeyContextMenu(ContextualMenuPopulateEvent ctx, LocalizationTable table, int row) {
            ctx.menu.AppendAction("Delete", action => {
                table.RemoveKey(row);
                rootVisualElement.schedule.Execute(RecreateGUI);
            });
        }

        internal void OnTablesDirty() {
            RecreateGUI();
        }

        internal void OnLocalesDirty() {
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