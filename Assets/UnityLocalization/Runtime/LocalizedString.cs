using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityLocalization.Data;

namespace UnityLocalization.Runtime {
    public class LocalizedString : MonoBehaviour {
        public LocalizationSettings Settings;
        [SerializeField] public string Key;
        [SerializeField] public LocalizationTable Table;
        [SerializeField] public UnityEvent<string> Setter;
        
        private void Reset() {
            Settings = ActiveLocalizationSettings.Load().ActiveSettings;
        }
    }
}