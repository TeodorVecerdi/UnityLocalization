using UnityEngine.UIElements;

namespace UnityLocalization.Utility {
    internal static class Extensions {
        public static void AddStylesheet(this VisualElement element, StyleSheet styleSheet) {
            element.styleSheets.Add(styleSheet);
        }
    }
}