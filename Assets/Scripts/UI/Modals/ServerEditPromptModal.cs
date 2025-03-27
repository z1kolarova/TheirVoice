using Assets.Classes;
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

    [SerializeField] TMP_InputField promptTextInput;

    [Header("Buttons")]
    [SerializeField] private Button testPromptBtn;
    [SerializeField] private Button saveChangesBtn;

    private MinimalPromptSkeleton prompt;
    private string originalFileName;
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

        endConvoAbilityDropdown.PopulateDropdownAndPreselect(Utils.ValueList<EndConvoAbility>().Select(x => x.ToString()).ToList());
        languageSelectionDropdown.PopulateDropdownAndPreselect(PromptManager.I.Languages, currentlySelectedLanguage);
    }

    public override void Display()
    {
        base.Display();
    }

    public void Populate(MinimalPromptSkeleton promptSkeleton, string language)
    {
        prompt = promptSkeleton;
        originalFileName = prompt.Name;
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

        var fileExists = PromptManager.I.TryGetPromptTextInLanguage(prompt.Name, language, out originalPromptText);

        fileExistsLabel.text = fileExists.YesOrNo();
        promptTextInput.text = originalPromptText;
    }

    private void SaveChangesInPrompt()
    {
        if (fileNameInput.text != originalFileName)
        {
            //TODO: handle file name change
            // - rename plaintext file of each language
            // - rename it in all prompt bank files

            // - or maybe just give them IDs...
        }
        if (fileNameInput.text == originalFileName && promptTextInput.text != originalPromptText)
        {
            var dirPath = Path.Combine(Constants.PromptsDir, currentlySelectedLanguage);
            Utils.WriteFileContents(dirPath, fileNameInput.text + ".txt", promptTextInput.text);
        }

    }
}
