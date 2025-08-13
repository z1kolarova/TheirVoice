using Assets.Scripts;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSelectionModal : MonoBehaviour
{
    [SerializeField] TMP_Dropdown languageDropdown;
    [SerializeField] Button confirmLanguageBtn;

    void Start()
    {
        confirmLanguageBtn.onClick.AddListener(() =>
        {
            ApplySelectedLanguage();
            ClientDataUtils.SystemPromptsRequester.RequestData();
            ClientDataUtils.AvailablePromptsRequester.RequestData();
            MainMenuManager.I.SetStartButtonInteractable(true);
            SetActive(false);
        });
    }

    private void PopulateDropdownWithLangOptions()
    {
        languageDropdown.options.Clear();
        Debug.Log("Trying to pupulate dropdown with lang options");
        foreach (var language in ClientDataManager.I.Languages)
        {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(language.Name));
        }

        var preselect = UserSettingsManager.I.ConversationLanguage?.Name ?? "English";
        languageDropdown.SelectLabelInDropdown(preselect);
        languageDropdown.RefreshShownValue();
    }

    private void ApplySelectedLanguage()
    {
        Debug.Log(languageDropdown.GetDisplayedTextOfDropdown());
        UserSettingsManager.I.SetConversationLanguage(ClientDataManager.I.Languages
            .First(l => l.Name == languageDropdown.GetDisplayedTextOfDropdown()));
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
        if (value)
        {
            PopulateDropdownWithLangOptions();
        }
    }
}
