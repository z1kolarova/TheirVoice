using Assets.Classes;
using Assets.Structs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PromptManager : MonoBehaviour
{
    public static PromptManager I => instance;
    static PromptManager instance;

    private List<string> languages = null;
    public List<string> Languages
    {
        get
        {
            if (languages == null)
            {
                LoadLanguages();
            }
            return languages;
        }
    }

    private List<PromptInMainBank> mainBankPrompts = null;
    public List<PromptInMainBank> MainBankPrompts
    { 
        get {
            if (mainBankPrompts == null)
            {
                LoadMainPromptBank();
            }
            return mainBankPrompts;
        } 
    }

    private Dictionary<LangPrompt, bool> langPromptAvailabilityDic = new Dictionary<LangPrompt, bool>();
    private Dictionary<LangPrompt, string> langPromptCachedTextDic = new Dictionary<LangPrompt, string>();

    private void Awake()
    {
        if (I == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Languages
    private void LoadLanguages()
    {
        languages = Directory.GetDirectories(Constants.PromptsDir)
            .Select(d => Path.GetFileName(d))
            .ToList();

        languages.Remove("Plaintext");
        languages.Sort();
    }

    #endregion Languages

    #region MainBank
    private void LoadMainPromptBank()
    {
        var promptBankFilePath = Utils.EnsureFileExists(Constants.PromptsDir, Constants.PromptBankFileName);

        using (StreamReader sr = new StreamReader(promptBankFilePath))
        using (JsonReader jr = new JsonTextReader(sr))
        {
            mainBankPrompts = Utils.Serializer.Deserialize<List<PromptInMainBank>>(jr) ?? new List<PromptInMainBank>();
        }
    }
    #endregion MainBank

    #region Availability
    public void EnsureAvailabilitiesLoaded(string language)
    {
        var langKey = new LangPrompt { Language = language, PromptFileName = language };
        if (!langPromptAvailabilityDic.ContainsKey(langKey))
        {
            var langDir = Path.Combine(Constants.PromptsDir, language);
            var langPromptBank = Path.Combine(langDir, Constants.PromptBankFileName);
            List<string> langAvailablePromptNames;

            Utils.EnsureFileExists(langDir, Constants.PromptBankFileName);
            using (StreamReader sr = new StreamReader(langPromptBank))
            using (JsonReader jr = new JsonTextReader(sr))
            {
                langAvailablePromptNames = Utils.Serializer.Deserialize<List<string>>(jr) ?? new List<string>();
            }

            foreach (var name in langAvailablePromptNames)
            {
                langPromptAvailabilityDic.Add(new LangPrompt { Language = language, PromptFileName = name }, true);
            }

            langPromptAvailabilityDic.Add(langKey, true);
        }
    }

    public bool GetPromptAvailabilityInLang(string promptFileName, string language)
    {
        EnsureAvailabilitiesLoaded(language);
        var key = new LangPrompt(language, promptFileName);
        langPromptAvailabilityDic.TryGetValue(key, out bool available);
        return available;
    }
    #endregion Availability
    
    #region Fulltext
    public bool TryGetPromptTextInLanguage(string promptName, string language, out string fullPromptText)
    {
        var fileName = promptName + ".txt";
        fullPromptText = "";

        var key = new LangPrompt { Language = language, PromptFileName = fileName };
        if (langPromptCachedTextDic.TryGetValue(key, out fullPromptText))
            return true;

        if (TryReadPromptTextInLanguageFromFile(fileName, language, out fullPromptText))
        {
            langPromptCachedTextDic.Add(key, fullPromptText);
            return true;
        }
        return false;
    }

    public bool TryReadPromptTextInLanguageFromFile(string fileName, string language, out string fullPromptText)
    {
        fullPromptText = "";
        var filePath = Path.Combine(Constants.PromptsDir, language, fileName);

        if (File.Exists(filePath))
        {
            fullPromptText = File.ReadAllText(filePath);
            return true;
        }
        return false;
    }
    #endregion Fulltext
}
