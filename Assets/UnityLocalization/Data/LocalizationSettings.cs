using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityLocalization.Data {
    [CreateAssetMenu(menuName = "Localization/Localization Settings", order = 0)]
    public class LocalizationSettings : ScriptableObject {
        [SerializeReference] public LocalizationSettings ActiveSettings;
        [SerializeField] private bool tablesDirty;
        [SerializeField] private Locale defaultLocale;
        [SerializeField] private List<Locale> locales = new List<Locale>();
        [SerializeField] private List<string> tableGuids = new List<string>();
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

        public LocalizationTable AddTable(string tableName) {
            var table = CreateInstance<LocalizationTable>();
            table.Initialize(tableName, locales);
            tables.Add(table);
            tableGuids.Add(Guid.NewGuid().ToString());
            return table;
        }

        public void RemoveTable(LocalizationTable table) {
            var tableIndex = tables.IndexOf(table);
            if (tableIndex < 0) throw new ArgumentException("The specified table does not exist.");

            tables.Remove(table);
            tableGuids.RemoveAt(tableIndex);
        }

        public int DefaultLocaleIndex() {
            return locales.IndexOf(defaultLocale);
        }

        public int GuidToTableIndex(string guid) {
            return tableGuids.IndexOf(guid);
        }

        public LocalizationTable GuidToTable(string guid) {
            var index = GuidToTableIndex(guid);
            if (index < 0) return null;
            return tables[index];
        }

        public string GuidToTableName(string guid, string notFoundName = "Missing") {
            var table = GuidToTable(guid);
            if (table == null) return notFoundName ?? "Missing";
            return table.TableName;
        }

        public Locale DefaultLocale => defaultLocale;
        public List<Locale> Locales => locales;
        public List<LocalizationTable> Tables => tables;
        public List<string> TableGuids => tableGuids;
        public bool HasLocale(Locale locale) => locales.Contains(locale);

        public bool TablesDirty {
            get => tablesDirty;
            private set => tablesDirty = value;
        }
    }
}