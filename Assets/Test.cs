using System.Collections.Generic;
using NaughtyAttributes;
using UnityCommons;
using UnityEngine;
using UnityLocalization.Data;

    public class Test : MonoBehaviour {
        private static List<Locale> locales;
        [Expandable] public LocalizationSettings Settings;

        [Button]
        private void AddRandomLocale() {
            if (locales == null || locales.Count == 0) locales = Locale.GetAllLocales();
            Settings.AddLocale(Rand.ListItem(locales));
        }

        [Button]
        private void RemoveRandomLocale() {
            if (locales == null || locales.Count == 0) locales = Locale.GetAllLocales();
            Settings.RemoveLocale(Settings.Locales[Rand.Range(0, Settings.Locales.Count)]);
        }
    }