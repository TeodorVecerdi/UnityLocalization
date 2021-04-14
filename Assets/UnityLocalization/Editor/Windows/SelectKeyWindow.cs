using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityLocalization.Data;

namespace UnityLocalization {
    public class SelectKeyWindow : EditorWindow {
        public static void Display(LocalizedStringEditor owner, LocalizationSettings settings, LocalizationTable table) {
            var window = CreateInstance<SelectKeyWindow>();
            window.table = table;
            window.settings = settings;
            window.owner = owner;
            window.titleContent = new GUIContent("Select Key");
            window.Initialize();
            window.ShowModal();
        }

        private LocalizedStringEditor owner;
        private LocalizationSettings settings;
        private LocalizationTable table;
        private List<LocalizationEntry> allEntries;
        private List<LocalizationEntry> filteredEntries;
        private Vector2 scrollPosition;
        private string searchQuery = string.Empty;
        
        private void OnGUI() {
            GUILayout.Label("Select Key");
            EditorGUI.BeginChangeCheck();
            searchQuery = GUILayout.TextField(searchQuery);
            if (string.IsNullOrEmpty(searchQuery)) {
                var guiColor = GUI.color;
                GUI.color = Color.grey;
                var lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x += 4;
                lastRect.width -= 4;
                EditorGUI.LabelField(lastRect, "Search keys");
                GUI.color = guiColor;
            }
            if (EditorGUI.EndChangeCheck()) {
                UpdateFilter();
            }

            var defaultLocaleIndex = settings.DefaultLocaleIndex();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (var index = 0; index < filteredEntries.Count; index++) {
                var entry = filteredEntries[index];
                var back = GUI.backgroundColor;
                GUI.backgroundColor = index % 2 == 0 ? Color.white : new Color(0.07f, 0.07f, 0.07f);
                GUILayout.BeginHorizontal("box");
                GUI.backgroundColor = back;
                GUILayout.Label($"{entry.Key} - {(string.IsNullOrEmpty(entry.Values[defaultLocaleIndex]) ? "NO VALUE SET" : entry.Values[defaultLocaleIndex])}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select", GUILayout.MinWidth(100))) {
                    owner.OnKeyDirty(index);
                    Close();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
        
        private void Initialize() {
            allEntries = table.Entries;
            UpdateFilter();
        }
        
        private void UpdateFilter() {
            filteredEntries = allEntries.Where(entry => entry.Key.ToLowerInvariant().Contains(searchQuery.ToLowerInvariant()) || searchQuery.ToLowerInvariant().Contains(entry.Key.ToLowerInvariant()) ||
                                                        entry.Values.Any(value => value.ToLowerInvariant().Contains(searchQuery.ToLowerInvariant()) || searchQuery.ToLowerInvariant().Contains(value.ToLowerInvariant()))).ToList();
        }
    }
}