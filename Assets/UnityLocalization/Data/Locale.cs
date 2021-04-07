using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace UnityLocalization.Data {
    [Serializable]
    public class Locale : IEquatable<Locale> {
        [SerializeField] private string localeCode;
        [SerializeField] private string englishName;
        [SerializeField] private string nativeName;

        public string LocaleCode => localeCode;
        public string EnglishName => englishName;
        public string NativeName => nativeName;

        public Locale(string localeCode, string englishName, string nativeName) {
            this.localeCode = localeCode;
            this.englishName = englishName;
            this.nativeName = nativeName;
        }

        public static Locale CreateFromCultureInfo(CultureInfo culture) {
            string cultureName;
            try {
                cultureName = CultureInfo.CreateSpecificCulture(culture.Name).Name;
            } catch {
                cultureName = culture.Name;
            }

            return new Locale(cultureName, culture.EnglishName, culture.NativeName);
        }

        public static List<Locale> GetAllLocales(bool extended) {
            return CultureInfo.GetCultures(extended ? CultureTypes.SpecificCultures : CultureTypes.NeutralCultures)
                              .Where(l => !string.IsNullOrEmpty(l.Name)).Select(CreateFromCultureInfo).ToList();
        }

        #region Equality Methods

        public bool Equals(Locale other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return localeCode == other.localeCode;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Locale) obj);
        }

        public override int GetHashCode() {
            return (localeCode != null ? localeCode.GetHashCode() : 0);
        }

        #endregion
    }
}