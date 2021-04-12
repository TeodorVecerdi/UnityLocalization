using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityLocalization.Data;
using UnityLocalization.Utility;

namespace UnityLocalization {
    public class CreateTableWindow : EditorWindow {
        public static void Display(Vector2 position, LocalizationSettings settings, Action onCreate = null) {
            var window = CreateInstance<CreateTableWindow>();
            window.settings = settings;
            window.onCreate = onCreate;
            window.titleContent = new GUIContent("Create Table");
            window.Initialize();
            window.ShowAsDropDown(new Rect(position, Vector2.one), new Vector2(200, 82));
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void InitializeOnReload() {
            var window = Utils.Find<CreateTableWindow>();
            if(window == null) return;
            
            var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
            var stylesheet = Resources.Load<StyleSheet>("Stylesheets/CreateTableWindow");
            try {
                window.AddStylesheet(utilityStylesheet);
                window.AddStylesheet(stylesheet);
            } catch {
                window.deferStylesheetLoading = true;
            }
        }

        [SerializeReference] private LocalizationSettings settings;
        [SerializeField] private string tableName;
        private bool deferStylesheetLoading;
        private Action onCreate;

        private void Initialize() {
            var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
            var stylesheet = Resources.Load<StyleSheet>("Stylesheets/CreateTableWindow");
            this.AddStylesheet(utilityStylesheet);
            this.AddStylesheet(stylesheet);
            tableName = "";
        }

        private void CreateGUI() {
            if (deferStylesheetLoading) {
                var utilityStylesheet = Resources.Load<StyleSheet>("Stylesheets/Utility");
                var stylesheet = Resources.Load<StyleSheet>("Stylesheets/CreateTableWindow");
                this.AddStylesheet(utilityStylesheet);
                this.AddStylesheet(stylesheet);
                deferStylesheetLoading = false;
            }
            
            var createTableButton = new Button(TriggerCreateTable) {text = "Create", name = "CreateTableButton"};
            createTableButton.SetEnabled(false);
            VisualElementFactory.TextFieldWithPlaceholder("TableName", "Table Name", null, "Table name", null)
                                .Do(self => {
                                    self.BindProperty(new SerializedObject(this).FindProperty("tableName"));
                                    self.RegisterValueChangedCallback(evt => { createTableButton.SetEnabled(!string.IsNullOrEmpty(evt.newValue)); });
                                    rootVisualElement.Add(self);
                                });
            rootVisualElement.Add(createTableButton);
        }

        private void TriggerCreateTable() {
            var savePath = EditorUtility.SaveFilePanel($"Create Table {tableName}", Application.dataPath, $"{tableName}", "asset");
            if (string.IsNullOrEmpty(savePath))
                return;

            if (!savePath.StartsWith(Application.dataPath)) {
                EditorUtility.DisplayDialog("Invalid location!", "The location specified is not valid. You should create the table in your assets folder, in a Resources subfolder.", "Close");
                Focus();
                return;
            }

            if (!savePath.Contains("/Resources")) {
                EditorUtility.DisplayDialog("Invalid location!", "The location specified is not valid. You should create the table in your assets folder, in a Resources subfolder.", "Close");
                Focus();
                return;
            }

            var cleanPath = savePath.Substring(Application.dataPath.LastIndexOf('/') + 1);
            settings.AddTable(tableName, cleanPath);
            
            Utils.DirtyTables(settings);
            onCreate?.Invoke();
            Close();
        }
    }
}