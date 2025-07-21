using Assets.Classes;
using Assets.Enums;
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
    [SerializeField] TMP_Text fileExistsLabel;
    [SerializeField] TMP_Dropdown endConvoAbilityDropdown;
    [SerializeField] TMP_Dropdown languageSelectionDropdown;
    [SerializeField] TMP_Dropdown promptReadyDropdown;

    [SerializeField] TMP_InputField promptTextInput;

    [Header("Buttons")]
    [SerializeField] private Button testPromptBtn;
    [SerializeField] private Button saveChangesBtn;

    private MinimalPromptSkeleton prompt;
    private string originalPromptName;
    private string originalPromptText;
    private string currentlySelectedLanguage = "English";

    protected override void Awake()
    {
        instance = this;

        saveChangesBtn.onClick.AddListener(() =>
        {
            SaveChangesInPrompt();
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
        languageSelectionDropdown.PopulateDropdownAndPreselect(LanguageManager.I.LanguageNames, currentlySelectedLanguage);
        promptReadyDropdown.PopulateDropdownAndPreselect(Utils.NoYesSelectOptions);
    }

    public override void Display()
    {
        base.Display();
    }

    public void Populate(MinimalPromptSkeleton promptSkeleton, string language)
    {
        prompt = promptSkeleton;
        originalPromptName = prompt.Name;
        fileNameInput.text = prompt.Name;

        var lbl = prompt.EndConvoAbility.ToString();
        endConvoAbilityDropdown.SelectLabelInDropdown(lbl);
        languageSelectionDropdown.SelectLabelInDropdown(language);
        RefreshForLanguage(language);
    }

    private void RefreshForLanguage(string language)
    {
        if (prompt == null)
            return;

        PromptManager.I.TryGetPromptLocFromDB(prompt.Name, language, out var promptLoc);

        //var fileExists = PromptManager.I.TryGetPromptTextInLanguage(prompt.Name, language, out originalPromptText);

        fileExistsLabel.text = (promptLoc?.Available ?? false).YesOrNo();
        promptTextInput.text = promptLoc?.Text ?? "";
    }

    private void SaveChangesInPrompt()
    {
        var hasPromptChange = false;
        if (fileNameInput.text != originalPromptName)
        {
            hasPromptChange = true;

            //TODO: handle file name change
            // - rename plaintext file of each language
            // - rename it in all prompt bank files

            // - or maybe just give them IDs...
        }
        //if (fileNameInput.text == originalPromptName && promptTextInput.text != originalPromptText)
        //{
        //    var dirPath = Path.Combine(Constants.PromptsDir, currentlySelectedLanguage);
        //    Utils.WriteFileContents(dirPath, fileNameInput.text + ".txt", promptTextInput.text);
        //    PromptManager.I.DropPromptTextInLanguageFromCache(prompt.Name, currentlySelectedLanguage);
        //    fileExistsLabel.text = PromptManager.I.TryGetPromptTextInLanguage(prompt.Name, currentlySelectedLanguage, out _).YesOrNo();
        //}
    }
}
