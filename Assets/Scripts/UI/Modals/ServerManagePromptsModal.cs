using Assets.Classes;
using System.Collections.Generic;
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

    private List<GameObject> displayedPromptEntries = new List<GameObject>();

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

    public string CurrentlySelectedLanguage { get; private set; } = "English";

    protected override void Awake()
    {
        instance = this;

        addNewPromptBtn.onClick.AddListener(() =>
        {
            ServerEditPromptModal.I.Display();
            ServerEditPromptModal.I.Populate(new MinimalPromptSkeleton(), ServerManagePromptsModal.I.CurrentlySelectedLanguage);
        });

        languageSelectionDropdown.onValueChanged.AddListener(newValue =>
        {
            CurrentlySelectedLanguage = languageSelectionDropdown.options[newValue].text;
            UpdateLangAvailabilities(CurrentlySelectedLanguage);
        });

    }

    protected override void Start()
    {
        base.Start();
        LoadPromptEntries();
    }

    public override void Display()
    {
        base.Display();
        languageSelectionDropdown.PopulateDropdownAndPreselect(PromptManager.I.Languages, CurrentlySelectedLanguage);
    }

    private void LoadPromptEntries()
    {
        foreach (var prompt in PromptManager.I.MainBankPrompts)
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

    private void UpdateLangAvailabilities(string language)
    {
        foreach (var peGameObject in displayedPromptEntries)
        {
            var promptEntry = peGameObject.GetComponent<PromptEntry>();
            promptEntry.UpdatePromptAvailability(language);
        }
    }

    private void CreateAndAddPromptEntry(PromptInMainBank mainPrompt)
    {
        GameObject newEntry = Instantiate(promptEntryTemplate);
        newEntry.GetComponent<PromptEntry>().Populate(mainPrompt, CurrentlySelectedLanguage);
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
