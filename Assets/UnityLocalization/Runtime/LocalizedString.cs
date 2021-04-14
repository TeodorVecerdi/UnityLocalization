using System;
using UnityEngine;
using UnityEngine.Events;
using UnityLocalization.Data;

namespace UnityLocalization.Runtime {
    public class LocalizedString : MonoBehaviour {
        public LocalizationSettings Settings;
        public string KeyGuid;
        public LocalizationTable Table;
        public string TableGuid;
        public UnityEvent<string> Setter;
        // Editor specific fields
        public string Key;
        public bool IsPreviewActive;
        public int PreviewLocaleIndex = -1;

        public void Translate(int index) {
            var translated = Localization.TranslateString(TableGuid, KeyGuid, index);
            Setter?.Invoke(translated);
        }

        public void Translate() {
            var translated = Localization.TranslateString(TableGuid, KeyGuid);
            if (string.IsNullOrEmpty(translated)) translated = Localization.TranslateString(TableGuid, KeyGuid, Settings.DefaultLocaleIndex());
            if (string.IsNullOrEmpty(translated)) translated = $"'{Table.GuidToEntry(KeyGuid).Key}' : MISSING TRANSLATION";
            Setter?.Invoke(translated);
        }

        private void Start() {
            Translate();
        }

        private void Reset() { 
            Key = null;
            KeyGuid = null;
            Table = null;
            TableGuid = null;
            Setter.RemoveAllListeners();
        }
    }
}