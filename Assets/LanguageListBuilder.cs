using UnityEngine;
using UnityLocalization.Runtime;

public class LanguageListBuilder : MonoBehaviour {
    [SerializeField] private LanguageOption LanguagePrefab;
    private void Start() {
        var locales = Localization.Settings.Locales;
        for (var index = 0; index < locales.Count; index++) {
            var locale = locales[index];
            var lang = Instantiate(LanguagePrefab, transform);
            lang.Initialize(index, locale);
        }
    }
}