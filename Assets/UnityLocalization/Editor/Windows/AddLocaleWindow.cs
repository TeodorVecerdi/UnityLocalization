using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityLocalization.Data;
using UnityLocalization.Shared;
using UnityLocalization.Utility;

namespace UnityLocalization {
    public class AddLocaleWindow : EditorWindow {
        public static void Display(LocalizationSettingsWindow owner, LocalizationSettings settings) {
            if (!EditorPrefs.HasKey(Constants.SHOW_SPECIFIC_LOCALES_PREFS_KEY)) {
                EditorPrefs.SetBool(Constants.SHOW_SPECIFIC_LOCALES_PREFS_KEY, false);
            }
            
            var window = CreateInstance<AddLocaleWindow>();
            window.settings = settings;
            window.owner = owner;
            window.titleContent = new GUIContent("Add Locale");
            window.showSpecificLocales = EditorPrefs.GetBool(Constants.SHOW_SPECIFIC_LOCALES_PREFS_KEY);
            window.Initialize();
            window.ShowModal();
        }

        private LocalizationSettingsWindow owner;
        private LocalizationSettings settings;
        private List<Locale> allLocales;
        private List<Locale> filteredLocales;
        private Vector2 scrollPosition;
        private string searchQuery = string.Empty;
        private bool showSpecificLocales;

        private void Initialize() {
            allLocales = Locale.GetAllLocales(showSpecificLocales);
            UpdateFilter();
        }

        private void OnGUI() {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Add Locale");
            showSpecificLocales = GUILayout.Toggle(showSpecificLocales, "Show country/region specific locales?");
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) {
                EditorPrefs.SetBool(Constants.SHOW_SPECIFIC_LOCALES_PREFS_KEY, showSpecificLocales);
                allLocales = Locale.GetAllLocales(showSpecificLocales);
                UpdateFilter();
            }

            EditorGUI.BeginChangeCheck();
            searchQuery = GUILayout.TextField(searchQuery);
            if (string.IsNullOrEmpty(searchQuery)) {
                var guiColor = GUI.color;
                GUI.color = Color.grey;
                var lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x += 4;
                lastRect.width -= 4;
                EditorGUI.LabelField(lastRect, "Search locales");
                GUI.color = guiColor;
            }
            if (EditorGUI.EndChangeCheck()) {
                UpdateFilter();
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (var index = 0; index < filteredLocales.Count; index++) {
                var locale = filteredLocales[index];
                var back = GUI.backgroundColor;
                GUI.backgroundColor = index % 2 == 0 ? Color.white : new Color(0.07f, 0.07f, 0.07f);
                GUILayout.BeginHorizontal("box");
                GUI.backgroundColor = back;
                GUILayout.Label($"{locale.EnglishName} ({locale.NativeName}) - {locale.LocaleCode}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add", GUILayout.MinWidth(100))) {
                    Utils.RecordChange(settings, "Added locale");
                    settings.AddLocale(locale);
                    Utils.SaveChanges();
                    Utils.DirtyLocales(settings);
                    UpdateFilter();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        private void UpdateFilter() {
            filteredLocales = allLocales.Where(locale => !settings.HasLocale(locale) && (locale.EnglishName.ToLowerInvariant().Contains(searchQuery.ToLowerInvariant()) ||
                                                         locale.NativeName.ToLowerInvariant().Contains(searchQuery.ToLowerInvariant()) ||
                                                         locale.LocaleCode.ToLowerInvariant().Contains(searchQuery.ToLowerInvariant()))).ToList();
        }
    }
}