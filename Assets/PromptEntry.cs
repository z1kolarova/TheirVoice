using Assets.Classes;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptEntry : MonoBehaviour
{
    [SerializeField] Toggle activeIfAvailable;
    [SerializeField] TMP_Text promptNameLabel;
    [SerializeField] TMP_Text endConvoAbilityLabel;
    [SerializeField] TMP_Text availableInThisLanguageLabel;
    [SerializeField] Button editPromptBtn;

    private Prompt pePrompt;

    // Start is called before the first frame update
    void Start()
    {
        activeIfAvailable.onValueChanged.AddListener((value) =>
        {
            UpdateActiveIfAvailable(value);
        });

        editPromptBtn.onClick.AddListener(() => {
            ServerEditPromptModal.I.Display();
            ServerEditPromptModal.I.Populate(pePrompt, ServerManagePromptsModal.I.CurrentlySelectedLanguage);
        });
    }

    public string GetPromptName() => pePrompt.Name;

    public void Populate(Prompt prompt, string language)
    {
        pePrompt = prompt;

        activeIfAvailable.isOn = prompt.ActiveIfAvailable;
        promptNameLabel.text = prompt.Name;
        endConvoAbilityLabel.text = prompt.EndConvoAbility.ToString();

        var langId = DBService.I.Languages.First(l => l.Name == language).Id;
        SetDisplayedAvailablity(DBService.I.PromptLocs.Any(pl => pl.PromptId == prompt.Id && pl.LangId == langId));
        //SetDisplayedAvailablity(PromptManager.I.GetPromptAvailabilityInLang(prompt.Name, language));
    }

    public void UpdateActiveIfAvailable(bool newValue)
    {
        pePrompt.ActiveIfAvailable = newValue;
        DBService.I.Update(pePrompt);
    }

    public void RefreshForLanguage(string language)
    {
        var newAvailability = PromptManager.I.GetPromptAvailabilityInLang(pePrompt.Name, language);
        SetDisplayedAvailablity(newAvailability);
    }

    public void SetDisplayedAvailablity(bool newAvailable)
    {
        availableInThisLanguageLabel.text = newAvailable.YesOrNo();
    }
}
