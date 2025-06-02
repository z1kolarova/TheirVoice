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

    //private PromptSettingsLabel psl;
    private PromptEntryContent pec;

    // Start is called before the first frame update
    void Start()
    {
        editPromptBtn.onClick.AddListener(() => {
            ServerEditPromptModal.I.Display();
            ServerEditPromptModal.I.Populate(pec, ServerManagePromptsModal.I.CurrentlySelectedLanguage);
        });
    }

    public string GetPromptName() => pec.Name;

    public void AssignLabel(PromptEntryContent promptEntryContent)
    {
        pec = promptEntryContent;
        active.isOn = pec.Active;
        promptNameLabel.text = pec.Name;
        endConvoAbilityLabel.text = pec.EndConvoAbility.ToString();
        SetDisplayedAvailablity(pec.AvailableInCurrentLanguage);
    }

    public void Populate(MinimalPromptSkeleton mps, string language)
    {
        pec = new PromptEntryContent
        {
            Active = false,
            Name = mps.Name,
            EndConvoAbility = mps.EndConvoAbility,
        };

        active.isOn = pec.Active;
        promptNameLabel.text = pec.Name;
        endConvoAbilityLabel.text = pec.EndConvoAbility.ToString();
        SetDisplayedAvailablity(PromptManager.I.GetPromptAvailabilityInLang(mps.Name, language));
    }

    public void UpdatePromptAvailability(string language)
    {
        var newAvailability = PromptManager.I.GetPromptAvailabilityInLang(pec.Name, language);
        SetDisplayedAvailablity(newAvailability);
    }

    public void SetDisplayedAvailablity(bool newAvailable)
    {
        pec.AvailableInCurrentLanguage = newAvailable;
        availableInThisLanguageLabel.text = newAvailable.YesOrNo();
    }
}
