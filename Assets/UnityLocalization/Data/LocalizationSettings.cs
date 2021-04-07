using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityLocalization.Data {
    [CreateAssetMenu(menuName = "Localization/Localization Settings", order = 0)]
    public class LocalizationSettings : ScriptableObject {
        [SerializeField] private bool tablesDirty;
        [SerializeField] private Locale defaultLocale;
        [SerializeField] private List<Locale> locales = new List<Locale>();
        [SerializeField] private List<LocalizationTable> tables = new List<LocalizationTable>();

        public void SetDefaultLocale(Locale locale) {
            if (defaultLocale.Equals(locale)) return;
            if (!locales.Contains(locale)) throw new InvalidOperationException("The specified locale is not part of this Localization Settings");
            var oldDefaultLocale = defaultLocale;
            defaultLocale = locale;
            foreach (var table in tables) {
                table.OnDefaultLocaleChanged(oldDefaultLocale, defaultLocale);
            }
        }

        public void AddLocale(Locale locale) {
            if (locales.Contains(locale)) return;
            locales.Add(locale);
            foreach (var table in tables) {
                table.OnLocaleAdded(locale);
            }

            if (defaultLocale == null || string.IsNullOrEmpty(defaultLocale.LocaleCode)) SetDefaultLocale(locale);
        }

        public void RemoveLocale(Locale locale) {
            if (locale.Equals(defaultLocale)) throw new InvalidOperationException("Cannot remove locale when it's set as the default locale.");
            if (!locales.Remove(locale)) {
                Debug.Log("Could not remove locale");
                return;
            }

            foreach (var table in tables) {
                table.OnLocaleRemoved(locale);
            }
        }

        public void AddTable() {
            var table = CreateInstance<LocalizationTable>();
            table.Initialize(locales);
            tables.Add(table);
        }

        public void RemoveTable(LocalizationTable table) {
            tables.Remove(table);
        }

        public Locale DefaultLocale => defaultLocale;
        public List<Locale> Locales => locales;
        public bool HasLocale(Locale locale) => locales.Contains(locale);

        public bool TablesDirty {
            get => tablesDirty;
            private set => tablesDirty = value;
        }
    }
}