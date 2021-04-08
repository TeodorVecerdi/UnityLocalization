using UnityEditor;
using UnityEngine;

namespace UnityLocalization.Utility {
    public static class Utils {
        public static void RecordChange(Object @object, string action) {
            EditorUtility.SetDirty(@object);
            Undo.RecordObject(@object, action);
        }

        public static void SaveChanges() {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}