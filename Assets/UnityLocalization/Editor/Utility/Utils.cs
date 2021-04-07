using UnityEditor;
using UnityLocalization.Data;

namespace UnityLocalization.Utility {
    public static class Utils {
        public static void RecordChange(LocalizationSettings settings, string action) {
            EditorUtility.SetDirty(settings);
            Undo.RecordObject(settings, action);
        }

        public static void SaveChanges() {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}