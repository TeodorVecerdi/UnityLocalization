using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Utility;

namespace UnityLocalization {
    public class TableEditorWindow : EditorWindow {
        public static void Display(LocalizationSettings settings) {
            var window = CreateInstance<TableEditorWindow>();
            window.settings = settings;
            window.titleContent = new GUIContent("Table Editor");
            window.Show();
            Initialize();
        }

        [DidReloadScripts]
        private static void Initialize() {
            if (!HasOpenInstances<TableEditorWindow>()) return;
            var window = GetWindow<TableEditorWindow>();
            var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
            var stylesheet = Resources.Load<StyleSheet>("Stylesheets/TableEditorWindow");
            try {
                window.rootVisualElement.styleSheets.Add(utilityStylesheet);
                window.rootVisualElement.styleSheets.Add(stylesheet);
            } catch {
                window.deferStylesheetLoading = true;
            }
        }

        [SerializeReference] private LocalizationSettings settings;
        private VisualElement tabContainer;
        private VisualElement tabContents;
        [SerializeField] private int activeTabIndex;
        private List<Tab> tabs;
        private bool deferStylesheetLoading;

        private void OnEnable() {
        }

        private void CreateGUI() {
            if (settings == null) {
                Initialize();
                OnEnable();
            }

            tabs = new List<Tab>();

            if (deferStylesheetLoading) {
                var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
                var stylesheet = Resources.Load<StyleSheet>("Stylesheets/TableEditorWindow");
                rootVisualElement.styleSheets.Add(utilityStylesheet);
                rootVisualElement.styleSheets.Add(stylesheet);
                deferStylesheetLoading = false;
            }

            tabContainer = new ScrollView(ScrollViewMode.Horizontal) {name = "TabContainer"};
            tabContents = new VisualElement {name = "TabContents"};

            tabs.Add(tabContainer.AddGet(new Tab("Hello")).Do(tab => {
                tab.Clicked += () => TabClicked(tab);
            }));
            tabs.Add(tabContainer.AddGet(new Tab("World")).Do(tab => { tab.Clicked += () => TabClicked(tab); }));
            tabs.Add(tabContainer.AddGet(new Tab("Tab3")).Do(tab => { tab.Clicked += () => TabClicked(tab); }));
            tabs.Add(tabContainer.AddGet(new Tab("Tab4")).Do(tab => { tab.Clicked += () => TabClicked(tab); }));


            if (activeTabIndex >= 0 && activeTabIndex < tabs.Count) {
                tabs[activeTabIndex].AddToClassList("active");
                LoadTabContent(tabs[activeTabIndex]);
            } else TabClicked(tabs[0]);

            rootVisualElement.Add(tabContainer);
            rootVisualElement.Add(tabContents);
        }

        private void TabClicked(Tab tab) {
            if(tabs[activeTabIndex] == tab) return;
            
            if (activeTabIndex < tabs.Count && activeTabIndex >= 0)
                tabs[activeTabIndex].RemoveFromClassList("active");
            tab.AddToClassList("active");
            activeTabIndex = tabs.IndexOf(tab);

            LoadTabContent(tab);
        }

        private void LoadTabContent(Tab tab) {
            tabContents.Clear();
            tabContents.AddGet<Label>("Contents", "contents").Do(label => {
                label.text = tab.TabName;
            });
        }
    }
}