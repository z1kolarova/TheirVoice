using Assets.Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerManagePromptsModal : JustCloseModal
{
    public static ServerManagePromptsModal I => instance;
    static ServerManagePromptsModal instance;

    [SerializeField] TMP_Dropdown languageSelectionDropdown;

    [SerializeField] private Button addNewPromptBtn;

    [SerializeField] private GameObject promptEntryTemplate;

    [SerializeField] private GameObject promptDisplayArea;

    private List<PromptInMainBank> mainBankPrompts = new List<PromptInMainBank>();
    private List<GameObject> displayedPromptEntries = new List<GameObject>();
    private Dictionary<(string, string), bool> langPromptAvailabilityDic = new Dictionary<(string, string), bool>();

    private float PromptEntryHeight => promptEntryTemplate.GetComponent<RectTransform>()?.rect.height ?? 0;
    private float PromptEntrySpacing => promptDisplayArea.GetComponent<VerticalLayoutGroup>()?.spacing ?? 0;
    private float PromptDisplayAreaTopAndBottomPadding()
    { 
        var vlg = promptDisplayArea.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
	    {
            return vlg.padding.top + vlg.padding.bottom;
	    }
        return 0;
    } 

    private string CurrentlySelectedLanguage => languageSelectionDropdown?.options[languageSelectionDropdown.value].text;

    protected override void Awake()
    {
        instance = this;

        addNewPromptBtn.onClick.AddListener(() =>
        {
            SaveAllPromptsOfLanguage(CurrentlySelectedLanguage);
        });

        languageSelectionDropdown.onValueChanged.AddListener(newValue =>
        {
            var langName = languageSelectionDropdown.options[newValue].text;
            LoadPromptsOfLanguage(langName);
        });

    }

    protected override void Start()
    {
        base.Start();
        LoadMainPromptBank();
    }

    public override void Display()
    {
        base.Display();
        PopulateDropdownWithLanguages();
    }

    private void LoadMainPromptBank()
    {
        //ServerSideManagerUI.I.WriteLineToOutput("I am trying to load");
        var promptBankFilePath = Utilities.EnsureFileExists(Constants.PromptsDir, Constants.PromptBankFileName);

        using (StreamReader sr = new StreamReader(promptBankFilePath))
        using (JsonReader jr = new JsonTextReader(sr))
        {
            mainBankPrompts = Utilities.Serializer.Deserialize<List<PromptInMainBank>>(jr) ?? new List<PromptInMainBank>();
        }

        foreach (var prompt in mainBankPrompts)
        {
            CreateAndAddPromptEntry(prompt);
        }

        var parentRect = promptDisplayArea.GetComponent<RectTransform>();
        if (parentRect != null)
        {
            var spacingHeight = displayedPromptEntries.Count > 1 ? PromptEntrySpacing * (displayedPromptEntries.Count - 1) : 0;
            var settingHeight = PromptEntryHeight * displayedPromptEntries.Count + spacingHeight + PromptDisplayAreaTopAndBottomPadding();
            parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, settingHeight);
        }
    }

    private void PopulateDropdownWithLanguages()
    {
        languageSelectionDropdown.options.Clear();

        var langs = Directory.GetDirectories(Constants.PromptsDir)
            .Select(d => Path.GetFileName(d))
            .ToList();

        langs.Remove("Plaintext");

        foreach (var lang in langs)
        {
            languageSelectionDropdown.options.Add(new TMP_Dropdown.OptionData(lang));
        }

        languageSelectionDropdown.value = langs.Contains("English") ? langs.IndexOf("English") : 0;
        languageSelectionDropdown.RefreshShownValue();
    }

    private void LoadPromptsOfLanguage(string language)
    {
        //var dir = Path.Combine(Constants.PromptsDir, language);
        //var promptFiles = Directory.GetFiles(dir);

        //var promptBankFilePath = Path.Combine(Constants.PromptsDir, Constants.PromptBankFileName);

        //List<PromptInMainBank> result = new List<PromptInMainBank>();

        //using (StreamReader sr = new StreamReader(promptBankFilePath))
        //using (JsonReader jr = new JsonTextReader(sr))
        //{
        //    result = Utilities.Serializer.Deserialize<List<PromptInMainBank>>(jr);
        //}

        //displayedPrompts.Clear();
        //foreach (var psl in result)
        //{
        //    psl.Active = true;
        //    Debug.Log($"{psl.Active} {psl.Name} {psl.GeneralConvoEndingAbility}");
        //    CreateAndAddPromptEntry(psl);
        //}

        //EmptyDisplayedList();

        if (!langPromptAvailabilityDic.ContainsKey((language, language)))
        {
            var langDir = Path.Combine(Constants.PromptsDir, language);
            var langPromptBank = Path.Combine(langDir, Constants.PromptBankFileName);
            List<string> langAvailablePromptNames;

            Utilities.EnsureFileExists(langDir, Constants.PromptBankFileName);
            using (StreamReader sr = new StreamReader(langPromptBank))
            using (JsonReader jr = new JsonTextReader(sr))
            {
                langAvailablePromptNames = Utilities.Serializer.Deserialize<List<string>>(jr) ?? new List<string>();
            }

            foreach (var name in langAvailablePromptNames)
            {
                langPromptAvailabilityDic.Add((language, name), true);
            }

            langPromptAvailabilityDic.Add((language, language), true);
        }

        UpdateLangAvailabilities(language);

        //foreach (var prompt in mainBankPrompts)
        //{
        //    CreateAndAddPromptEntry(prompt, language);
        //}
    }

    private void SaveAllPromptsOfLanguage(string language)
    {
        //foreach (var psl in displayedPrompts)
        //{
        //    var fileName = psl.Name;
        //}
    }

    private void UpdateLangAvailabilities(string language)
    {
        foreach (var peGameObject in displayedPromptEntries)
        {
            var promptEntry = peGameObject.GetComponent<PromptEntry>();
            var langAvailability = langPromptAvailabilityDic.TryGetValue((language, promptEntry.GetName()), out var isAvailable);
            peGameObject.GetComponent<PromptEntry>().SetLangAvailability(langAvailability ? isAvailable : false);
        }
    }

    private void CreateAndAddPromptEntry(PromptInMainBank mainPrompt)
    {
        var psl = new PromptSettingsLabel() { 
            Active = mainPrompt.Active,
            Name = mainPrompt.Name,
            GeneralConvoEndingAbility = mainPrompt.GeneralConvoEndingAbility,
            AvailableInCurrentLanguage = false
        };
        GameObject newEntry = Instantiate(promptEntryTemplate);
        newEntry.GetComponent<PromptEntry>().AssignLabel(psl);
        newEntry.transform.SetParent(promptDisplayArea.transform, false);
        displayedPromptEntries.Add(newEntry);
    }

    private void EmptyDisplayedList()
    {
        for (int i = displayedPromptEntries.Count - 1; i >= 0; i--)
        {
            RemoveAndDestroyPromptEntry(displayedPromptEntries[i]);
        }
    }

    private void RemoveAndDestroyPromptEntry(GameObject entry)
    {
        displayedPromptEntries.Remove(entry);
        Destroy(entry);
    }
}
