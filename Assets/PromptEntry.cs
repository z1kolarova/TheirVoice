using Assets.Classes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptEntry : MonoBehaviour
{
    [SerializeField] Toggle active;
    [SerializeField] TMP_Text promptNameLabel;
    [SerializeField] TMP_Text endConvoAbilityLabel;
    [SerializeField] TMP_Text availableInThisLanguageLabel;
    [SerializeField] Button editPromptBtn;

    private PromptSettingsLabel psl;

    // Start is called before the first frame update
    void Start()
    {
        editPromptBtn.onClick.AddListener(() => {
            Debug.Log("edit prompt button was clicked");
        });

    }

    public string GetName() => psl.Name;

    public void AssignLabel(PromptSettingsLabel promptSettingsLabel)
    {
        psl = promptSettingsLabel;
        active.isOn = psl.Active;
        promptNameLabel.text = psl.Name;
        endConvoAbilityLabel.text = psl.GeneralConvoEndingAbility.ToString();
        SetLangAvailability(promptSettingsLabel.AvailableInCurrentLanguage);
    }

    public void SetLangAvailability(bool newAvailable)
    {
        availableInThisLanguageLabel.text = newAvailable ? "Yes" : "No";
    }
}
