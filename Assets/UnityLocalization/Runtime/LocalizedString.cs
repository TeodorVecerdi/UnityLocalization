using UnityEngine;
using UnityEngine.Events;
using UnityLocalization.Data;

namespace UnityLocalization.Runtime {
    public class LocalizedString : MonoBehaviour {
        [SerializeField, HideInInspector] private LocalizationSettings settings;
        [SerializeField, HideInInspector] private string key;
        [SerializeField, HideInInspector] private LocalizationTable table;
        [SerializeField, HideInInspector] private UnityEvent<string> setter;
        
        private void Reset() {
            settings = ActiveLocalizationSettings.Load().ActiveSettings;
        }
    }
}