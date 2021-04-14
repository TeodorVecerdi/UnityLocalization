using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityLocalization.Data;
using UnityLocalization.Runtime;

public class LanguageOption : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;

    public void Initialize(int index, Locale locale) {
        text.text = $"{locale.NativeName}";
        button.onClick.AddListener(() => Localization.SetLocale(index));
    }
}