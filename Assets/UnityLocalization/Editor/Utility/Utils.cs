using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityLocalization.Data;
using Object = UnityEngine.Object;

namespace UnityLocalization.Utility {
    internal static class Utils {
        public static void RecordChange(LocalizationSettings settings, string action) {
            Undo.IncrementCurrentGroup();
            EditorUtility.SetDirty(settings);
            Undo.RegisterCompleteObjectUndo(settings, action);
            foreach (var table in settings.Tables) {
                Undo.RegisterCompleteObjectUndo(table, action);
            }
        }

        public static void RecordChange(Object obj, string action) {
            EditorUtility.SetDirty(obj);
            Undo.RecordObject(obj, action);
        }

        public static void ApplyChanges(Object obj) {
            EditorUtility.SetDirty(obj);
            SaveChanges();
        }

        public static void SaveChanges() {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        public static void DirtyTables(LocalizationSettings settings) {
            var tableEditor = FindMatching<TableEditorWindow>(window => window.settings == settings);
            var settingsEditor = Find<LocalizationSettingsWindow>();
            if (tableEditor != null) tableEditor.OnTablesDirty();
            if (settingsEditor != null) settingsEditor.OnTablesDirty();
        }
        
        public static void DirtyLocales(LocalizationSettings settings) {
            var tableEditor = FindMatching<TableEditorWindow>(window => window.settings == settings);
            var settingsEditor = Find<LocalizationSettingsWindow>();
            if (tableEditor != null) tableEditor.OnLocalesDirty();
            if (settingsEditor != null) settingsEditor.OnLocalesDirty();
        }

        public static void DirtySettings(LocalizationSettings settings) {
            var localizedStringEditors = FindAll<LocalizedStringEditor>();
            foreach (var localizedStringEditor in localizedStringEditors) {
                localizedStringEditor.OnSettingsDirty(settings);
            }
        }


        public static T FindMatching<T>(Func<T, bool> predicate) where T : Object {
            if (predicate == null) return Find<T>();
            return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(predicate);
        }
        public static T Find<T>() where T : Object {
            return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
        }
        
        public static T[] FindAll<T>() where T : Object {
            return Resources.FindObjectsOfTypeAll<T>();
        }
    }
}