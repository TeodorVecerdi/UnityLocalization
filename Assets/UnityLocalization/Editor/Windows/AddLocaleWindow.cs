using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityLocalization.Data;

namespace UnityLocalization {
    public class AddLocaleWindow : EditorWindow {
        public static void Display(LocalizationSettingsWindow owner, LocalizationSettings settings) {
            var window = CreateInstance<AddLocaleWindow>();
            window.settings = settings;
            window.owner = owner;
            window.titleContent = new GUIContent("Add Locale");
            window.Initialize();
            window.ShowModal();
        }

        private LocalizationSettingsWindow owner;
        private LocalizationSettings settings;
        private List<Locale> allLocales;
        private List<Locale> filteredLocales;
        private Vector2 scrollPosition;
        private string searchQuery = string.Empty;

        private void Initialize() {
            allLocales = Locale.GetAllLocales().Where(locale => !settings.HasLocale(locale)).ToList();
            filteredLocales = allLocales.ToList();
        }

        private void OnGUI() {
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
                filteredLocales = allLocales.Where(locale => locale.EnglishName.ToLowerInvariant().Contains(searchQuery.ToLowerInvariant()) ||
                                                             locale.NativeName.ToLowerInvariant().Contains(searchQuery.ToLowerInvariant()) ||
                                                             locale.LocaleCode.ToLowerInvariant().Contains(searchQuery.ToLowerInvariant()))
                                            .ToList();
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
                    settings.AddLocale(locale);
                    Close();
                    owner.UpdateFilter();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}