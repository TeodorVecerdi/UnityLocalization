using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityLocalization.Data {
    [CreateAssetMenu(menuName = "Localization/Localization Table", order = 1)]
    public class LocalizationTable : ScriptableObject {
        [SerializeField] private string tableName;
        [SerializeField] private Table table;
        public string TableName => tableName;

        internal void Initialize(string tableName, List<Locale> locales) {
            this.tableName = tableName;
            table = new Table(locales);
        }

        internal void AddLocalization(string key) {
            table.AddLocalization(key);
        }

        internal void UpdateKey(int row, string newKey) {
            table.UpdateKey(row, newKey);
        }
        
        internal void RemoveKey(int row) {
            table.RemoveKey(row);
        }

        internal void UpdateLocalization(int row, int column, string value) {
            table.UpdateLocalization(row, column, value);
        }

        internal void OnLocaleAdded(Locale locale) {
            table.OnLocaleAdded(locale);
        }

        internal void OnLocaleRemoved(Locale locale) {
            table.OnLocaleRemoved(locale);
        }

        internal void OnDefaultLocaleChanged(Locale oldDefaultLocale, Locale newDefaultLocale) {
            // table.OnDefaultLocaleChanged(oldDefaultLocale, newDefaultLocale);
        }

        [Serializable]
        internal class Table {
            [SerializeField] internal List<Locale> locales;
            [SerializeField] internal List<LocalizationEntry> entries;
            [SerializeField] internal List<Guid> entryGuids;

            internal Table(List<Locale> locales) {
                this.locales = new List<Locale>(locales);
                entries = new List<LocalizationEntry>();
                entryGuids = new List<Guid>();
            }

            internal void AddLocalization(string key) {
                var entry = new LocalizationEntry(key, locales.Count);
                entries.Add(entry);
                entryGuids.Add(Guid.NewGuid());
            }

            internal void UpdateLocalization(int row, int column, string value) {
                entries[row].UpdateValue(column, value);
            }

            internal void UpdateKey(int row, string newKey) {
                if (row >= entries.Count) throw new IndexOutOfRangeException();
                entries[row].Key = newKey;
            }

            internal void RemoveKey(int row) {
                if (row >= entries.Count) throw new IndexOutOfRangeException();
                entries.RemoveAt(row);
                entryGuids.RemoveAt(row);
            }

            internal void OnLocaleAdded(Locale locale) {
                locales.Add(locale);
                foreach (var entry in entries) entry.AddColumn();
            }

            internal void OnLocaleRemoved(Locale locale) {
                var localeIndex = locales.IndexOf(locale);
                if (localeIndex == -1) throw new ArgumentException($"Locale {locale} does not exist in the table.");
                
                locales.RemoveAt(localeIndex);
                foreach (var entry in entries) entry.RemoveColumn(localeIndex);
            }

            internal void OnDefaultLocaleChanged(Locale oldDefaultLocale, Locale newDefaultLocale) {
                var oldIndex = locales.IndexOf(oldDefaultLocale);
                var newIndex = locales.IndexOf(newDefaultLocale);
                if (oldIndex == -1) throw new ArgumentException($"Locale {oldDefaultLocale} does not exist in the table.");
                if (newIndex == -1) throw new ArgumentException($"Locale {newDefaultLocale} does not exist in the table.");
                foreach (var entry in entries) {
                    entry.SwapColumns(oldIndex, newIndex);
                }
            }
        }
    }
}