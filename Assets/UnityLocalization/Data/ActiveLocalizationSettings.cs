using UnityEditor;
using UnityEngine;

namespace UnityLocalization.Data {
    [CreateAssetMenu(menuName = "Localization/Active Localization Settings")]
    public class ActiveLocalizationSettings : ScriptableSingleton<ActiveLocalizationSettings> {
        [SerializeField] private LocalizationSettings activeSettings;
        public LocalizationSettings ActiveSettings {
            get => activeSettings;
            set => activeSettings = value;
        }
    }
}