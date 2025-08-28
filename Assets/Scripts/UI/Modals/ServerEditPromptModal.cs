using Assets.Enums;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerEditPromptModal : JustCloseModal
{
    public static ServerEditPromptModal I => instance;
    static ServerEditPromptModal instance;

    [SerializeField] TMP_InputField promptNameCodeInput;
    [SerializeField] TMP_Dropdown endConvoAbilityDropdown;
    [SerializeField] TMP_Dropdown promptCanBeUsedDropdown;
    [SerializeField] TMP_Dropdown languageSelectionDropdown;
    [SerializeField] TMP_Dropdown promptLocReadyDropdown;

    [SerializeField] TMP_InputField promptTextInput;

    [Header("Buttons")]
    [SerializeField] private Button testPromptBtn;
    [SerializeField] private Button saveChangesBtn;

    private Prompt dbPrompt;
    private PromptLoc dbPromptLoc;
    private string currentlySelectedLanguage = "English";

    //TODO: warn before closing editor with unsaved changes
    //TODO: warn before language switch with unsaved changes
    private bool HasPromptChange()
        => dbPrompt.Name != promptNameCodeInput.text
        || dbPrompt.EndConvoAbility.ToString() != endConvoAbilityDropdown.GetDisplayedTextOfDropdown()
        || dbPrompt.ActiveIfAvailable.YesOrNo() != promptCanBeUsedDropdown.GetDisplayedTextOfDropdown();

    private bool HasPromptLocChange()
        => dbPromptLoc.Text != promptTextInput.text
        || dbPromptLoc.Available.YesOrNo() != promptLocReadyDropdown.GetDisplayedTextOfDropdown();

    protected override void Awake()
    {
        instance = this;

        testPromptBtn.onClick.AddListener(() =>
        {
            testPromptBtn.enabled = false;
            ServerSideManagerUI.I.WriteCyanLineToOutput("Test prompt button was clicked");
            if (PrepareForTestStart())
                PromptTestingManager.I.StartTestingPrompt(promptNameCodeInput.text);
        });

        saveChangesBtn.onClick.AddListener(() =>
        {
            if (HasPromptChange())
                SaveChangesInPrompt();

            if (HasPromptLocChange())
                SaveChangesInPromptLoc();
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

        endConvoAbilityDropdown.PopulateDropdownAndPreselect(Utils.ValueList<EndConvoAbility>().Select(x => x.ToString()));
        promptCanBeUsedDropdown.PopulateDropdownAndPreselect(Utils.NoYesSelectOptions);
        languageSelectionDropdown.PopulateDropdownAndPreselect(LanguageManager.I.LanguageNames, currentlySelectedLanguage);
        promptLocReadyDropdown.PopulateDropdownAndPreselect(Utils.NoYesSelectOptions);
    }

    public override void Display()
    {
        base.Display();
    }

    public void Populate(Prompt prompt, string language)
    {
        dbPrompt = prompt;
        promptNameCodeInput.text = dbPrompt.Name;
        endConvoAbilityDropdown.SelectLabelInDropdown(dbPrompt.EndConvoAbility.ToString());
        promptCanBeUsedDropdown.SelectLabelInDropdown(dbPrompt.ActiveIfAvailable.YesOrNo());

        RefreshForLanguage(language);
    }

    private void RefreshForLanguage(string language)
    {
        if (dbPrompt == null)
            return;

        PromptManager.I.TryGetPromptLocFromDB(dbPrompt.Name, language, out dbPromptLoc);

        promptLocReadyDropdown.SelectLabelInDropdown((dbPromptLoc?.Available ?? false).YesOrNo());
        promptTextInput.text = dbPromptLoc?.Text ?? "";
    }

    private void SaveChangesInPrompt()
    {
        dbPrompt.Name = promptNameCodeInput.text;
        dbPrompt.EndConvoAbility = Enum.Parse<EndConvoAbility>(endConvoAbilityDropdown.GetDisplayedTextOfDropdown());
        dbPrompt.ActiveIfAvailable = promptCanBeUsedDropdown.GetDisplayedTextOfDropdown().IsYes();

        DBService.I.Update(dbPrompt);
    }

    private void SaveChangesInPromptLoc()
    {
        dbPromptLoc.Text = promptTextInput.text;
        dbPromptLoc.Available = promptLocReadyDropdown.GetDisplayedTextOfDropdown().IsYes();

        DBService.I.Update(dbPromptLoc);
    }

    private bool PrepareForTestStart()
    {
        var canStart = PromptManager.I.TryGetPromptLocFromDB(
                promptName: Constants.TESTING_PROMPT_NAME, currentlySelectedLanguage,
                out var testPromptLoc);

        if (canStart && PromptManager.I.TryGetPromptTextInLanguage(
            Constants.CAN_END_CONVO_PROMPT_NAME, currentlySelectedLanguage, 
            out string convoEndingInstruction))
        {
            convoEndingInstruction = convoEndingInstruction.FormatInConvoEndString();

            var outreacherSystemMessage = testPromptLoc.Text
                .EnrichText(EndConvoAbility.Always, convoEndingInstruction)
                .FullyAssembledText;

            var passerbySystemMessage = promptTextInput.text
                .EnrichText(
                    Enum.Parse<EndConvoAbility>(endConvoAbilityDropdown.GetDisplayedTextOfDropdown()),
                    convoEndingInstruction)
                .FullyAssembledText;

            TestingUtilsGPT.InitTestConversations(outreacherSystemMessage, passerbySystemMessage);
        }
        else
        {
            SetTestButtonActive(true);
        }

        return canStart;
    }

    public void SetTestButtonActive(bool active)
    { 
        testPromptBtn.enabled = active; 
    }
}
