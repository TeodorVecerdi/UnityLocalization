using UnityEditor;
using UnityEngine;
using UnityLocalization.Data;

namespace UnityLocalization.Utility {
    public static class Utils {
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

        public static void SaveChanges() {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}