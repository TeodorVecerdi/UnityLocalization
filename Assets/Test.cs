using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityLocalization.Runtime;

public class Test : MonoBehaviour {
    [Dropdown("GetLocales"), OnValueChanged("UpdateLocale")] public string Locale;
    [SerializeField] private int localeIndex;

    private void Awake() {
        localeIndex = Localization.CurrentLocaleIndex;
        Locale = Localization.Settings.Locales[localeIndex].LocaleCode;
    }

    private List<string> GetLocales() {
        return Localization.Settings.Locales.Select(locale => locale.LocaleCode).ToList();
    }

    private void UpdateLocale() {
        var locale = Localization.Settings.Locales.FirstOrDefault(locale1 => string.Equals(locale1.LocaleCode, Locale, StringComparison.InvariantCulture));
        if(locale == null) return;
        localeIndex = Localization.Settings.Locales.IndexOf(locale);
        Localization.SetLocale(localeIndex);
    }
}