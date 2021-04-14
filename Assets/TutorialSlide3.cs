using TMPro;
using UnityEngine;

public class TutorialSlide3 : MonoBehaviour {
    [SerializeField, HideInInspector] private string text;
    [SerializeField, HideInInspector] private string woodCutter;
    [SerializeField, HideInInspector] private string building;
    public string Text {
        get => text;
        set {
            text = value;
            UpdateUI();
        }
    }
    public string WoodCutter {
        get => woodCutter;
        set {
            woodCutter = value;
            UpdateUI();
        }
    }
    public string Building {
        get => building;
        set {
            building = value;
            UpdateUI();
        }
    }

    [SerializeField] private TextMeshProUGUI textUI;

    private void UpdateUI() {
        textUI.text = $"{text} <b>{woodCutter} {building}</b>";
    }
}