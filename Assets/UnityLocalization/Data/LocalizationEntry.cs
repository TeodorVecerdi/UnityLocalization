using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityLocalization.Data {
    [Serializable]
    public class LocalizationEntry {
        [SerializeField] public string Key;
        [SerializeField] public List<string> Values = new List<string>();

        internal LocalizationEntry(string key, int localeCount) {
            Key = key;
            for (var i = 0; i < localeCount; i++) {
                Values.Add(null);
            }
        }

        internal void AddColumn() {
            Values.Add(null);
        }

        internal void RemoveColumn(int columnIndex) {
            Values.RemoveAt(columnIndex);
        }

        internal void UpdateValue(int columnIndex, string value) {
            Values[columnIndex] = value;
        }

        internal void SwapColumns(int oldIndex, int newIndex) {
            var oldValue = Values[oldIndex];
            Values[oldIndex] = Values[newIndex];
            Values[newIndex] = oldValue;
        }

        internal bool IsLocalized(int columnIndex) {
            return !string.IsNullOrEmpty(Values[columnIndex]);
        }

        internal string GetValue(int columnIndex) {
            return Values[columnIndex];
        }
    }
}