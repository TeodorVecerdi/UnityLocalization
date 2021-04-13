using UnityEngine;
using UnityEngine.Events;
using UnityLocalization.Data;

namespace UnityLocalization.Runtime {
    public class LocalizedString : MonoBehaviour {
        public LocalizationSettings Settings;
        public string Key;
        public LocalizationTable Table;
        public UnityEvent<string> Setter;
    }
}