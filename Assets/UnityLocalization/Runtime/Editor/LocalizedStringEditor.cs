using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityLocalization.Shared;


namespace UnityLocalization.Runtime {
    [CustomEditor(typeof(LocalizedString))]
    public class LocalizedStringEditor : Editor {
        // Properties //
        private SerializedProperty setterProperty;
        
        // VisualElements //
        private bool deferStylesheetLoading;
        private bool stylesheetsLoaded;
        private VisualElement rootElement;
       
        private void OnEnable() {
            AcquireProperties();
        }

        public override VisualElement CreateInspectorGUI() {
            rootElement = new VisualElement {name = "LocalizedString"};
            
            if(PropertiesDirty()) AcquireProperties();
            if (!stylesheetsLoaded || deferStylesheetLoading) {
                LoadStylesheets();
                deferStylesheetLoading = false;
            }

            rootElement.schedule.Execute(() => {
                var parent = rootElement.parent;
                parent.AddStylesheet(Resources.Load<StyleSheet>("Stylesheets/LocalizedString_Parent"));
            });
            return rootElement;
        }

        private bool PropertiesDirty() {
            return setterProperty == null;
        }

        private void AcquireProperties() {
            setterProperty = serializedObject.FindProperty("setter");
        }

        private void LoadStylesheets(StyleSheet utility = null, StyleSheet main = null) {
            utility ??= Resources.Load<StyleSheet>("Stylesheets/Utility");
            main ??= Resources.Load<StyleSheet>("Stylesheets/LocalizedString");
            try {
                rootElement.AddStylesheet(utility);
                rootElement.AddStylesheet(main);
                stylesheetsLoaded = true;
            } catch {
                deferStylesheetLoading = true;
            }
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