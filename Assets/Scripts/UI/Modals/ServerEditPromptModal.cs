using Assets.Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerEditPromptModal : JustCloseModal
{
    public static ServerEditPromptModal I => instance;
    static ServerEditPromptModal instance;

    [SerializeField] TMP_InputField fileNameInput;
    [SerializeField] TMP_Dropdown endConvoAbilityDropdown;
    [SerializeField] TMP_Dropdown languageSelectionDropdown;

    [SerializeField] TMP_InputField promptTextInput;

    [Header("Buttons")]
    [SerializeField] private Button testPromptBtn;
    [SerializeField] private Button saveChangesBtn;

    private MinimalPromptSkeleton prompt;
    private string originalPromptText;
    private string currentlySelectedLanguage = "English";

    protected override void Awake()
    {
        instance = this;

        saveChangesBtn.onClick.AddListener(() =>
        {
            //SaveChangesInPrompt();
        });

        languageSelectionDropdown.onValueChanged.AddListener(newValue =>
        {
            currentlySelectedLanguage = languageSelectionDropdown.options[newValue].text;
            RefreshForLanguage(currentlySelectedLanguage);
        });
    }

    protected override void Start()
    {
        base.Start();

        endConvoAbilityDropdown.PopulateDropdownAndPreselect(Utilities.ValueList<EndConvoAbility>().Select(x => x.ToString()).ToList());
        languageSelectionDropdown.PopulateDropdownAndPreselect(PromptManager.I.Languages, currentlySelectedLanguage);
    }

    public override void Display()
    {
        base.Display();
    }

    public void Populate(MinimalPromptSkeleton promptSkeleton, string language)
    {
        prompt = promptSkeleton;
        fileNameInput.text = prompt.Name;
        var lbl = prompt.EndConvoAbility.ToString();
        endConvoAbilityDropdown.SelectLabelInDropdown(lbl);
        languageSelectionDropdown.SelectLabelInDropdown(language);
        PromptManager.I.TryGetPromptTextInLanguage(prompt.Name, language, out originalPromptText);
    }

    private void RefreshForLanguage(string language)
    {
        if (prompt == null)
            return;

        if (!PromptManager.I.TryGetPromptTextInLanguage(prompt.Name, currentlySelectedLanguage, out originalPromptText))
            originalPromptText = "";
    }
}
