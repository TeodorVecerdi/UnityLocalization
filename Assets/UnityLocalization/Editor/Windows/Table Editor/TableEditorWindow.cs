using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
            window.Initialize();
            window.Show();
        }

        [SerializeReference] private LocalizationSettings settings;
        private VisualElement tabContainer;
        private VisualElement tabContents;
        private bool deferStylesheetLoading;

        private void Initialize() {
            var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
            var stylesheet = Resources.Load<StyleSheet>("Stylesheets/TableEditorWindow");
            try {
                rootVisualElement.styleSheets.Add(utilityStylesheet);
                rootVisualElement.styleSheets.Add(stylesheet);
            } catch {
                deferStylesheetLoading = true;
            }

        }

        private void OnEnable() {
        }

        private void CreateGUI() {
            if (settings == null) {
                Initialize();
                OnEnable();
            }

            if (deferStylesheetLoading) {
                var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
                var stylesheet = Resources.Load<StyleSheet>("Stylesheets/TableEditorWindow");
                rootVisualElement.styleSheets.Add(utilityStylesheet);
                rootVisualElement.styleSheets.Add(stylesheet);
                deferStylesheetLoading = false;
            }

            tabContainer = new VisualElement {name = "TabContainer"};
            tabContents = new VisualElement {name = "TabContents"} ;

            rootVisualElement.Add(tabContainer);
            rootVisualElement.Add(tabContents);
        }
    }
}