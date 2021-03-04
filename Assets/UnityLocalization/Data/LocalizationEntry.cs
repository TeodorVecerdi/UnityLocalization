using System;
using System.Collections.Generic;

namespace UnityLocalization.Data {
    [Serializable]
    public class LocalizationEntry {
        internal string key;
        internal List<string> values = new List<string>();

        internal LocalizationEntry(string key, int localeCount) {
            this.key = key;
            for (var i = 0; i < localeCount; i++) {
                values.Add(null);
            }
        }

        internal void AddColumn() {
            values.Add(null);
        }

        internal void RemoveColumn(int columnIndex) {
            values.RemoveAt(columnIndex);
        }

        internal void UpdateValue(int columnIndex, string value) {
            values[columnIndex] = value;
        }

        internal void SwapColumns(int oldIndex, int newIndex) {
            var oldValue = values[oldIndex];
            values[oldIndex] = values[newIndex];
            values[newIndex] = oldValue;
        }

        internal bool IsLocalized(int columnIndex) {
            return !string.IsNullOrEmpty(values[columnIndex]);
        }

        internal string GetValue(int columnIndex) {
            return values[columnIndex];
        }
    }
}