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

    private Dictionary<string, int> promptIdDic = null;
    public Dictionary<string, int> PromptIdDic
    {
        get
        {
            if (promptIdDic == null)
            {
                promptIdDic = DBService.I.Prompts.ToDictionary(p => p.Name, p => p.Id);
            }
            return promptIdDic;
        }
    }

    private List<string> promptNames = null;
    public List<string> PromptNames
    {
        get
        {
            if (promptNames == null)
            {
                promptNames = PromptIdDic.Keys.ToList();
                promptNames.Sort();
            }
            return promptNames;
        }
    }

    //private List<PromptInMainBank> mainBankPrompts = null;
    //public List<PromptInMainBank> MainBankPrompts
    //{ 
    //    get 
    //    {
    //        if (mainBankPrompts == null)
    //        {
    //            LoadMainPromptBank();
    //        }
    //        return mainBankPrompts;
    //    } 
    //}

    private Dictionary<LangPrompt, bool> langPromptAvailabilityDic = new Dictionary<LangPrompt, bool>();
    private Dictionary<LangPrompt, string> langPromptCachedTextDic = new Dictionary<LangPrompt, string>();

    private void Awake()
    {
        if (I == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitDB();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region MainBank
    //private void LoadMainPromptBank()
    //{
    //    var promptBankFilePath = Utils.EnsureFileExists(Constants.PromptsDir, Constants.PromptBankFileName);

    //    using (StreamReader sr = new StreamReader(promptBankFilePath))
    //    using (JsonReader jr = new JsonTextReader(sr))
    //    {
    //        mainBankPrompts = Utils.Serializer.Deserialize<List<PromptInMainBank>>(jr) ?? new List<PromptInMainBank>();
    //    }
    //}
    #endregion MainBank

    #region Availability
    //public void EnsureAvailabilitiesLoaded(string language)
    //{
    //    var langKey = new LangPrompt { Language = language, PromptFileName = language };
    //    if (!langPromptAvailabilityDic.ContainsKey(langKey))
    //    {
    //        var langDir = Path.Combine(Constants.PromptsDir, language);
    //        var langPromptBank = Path.Combine(langDir, Constants.PromptBankFileName);
    //        List<string> langAvailablePromptNames;

    //        Utils.EnsureFileExists(langDir, Constants.PromptBankFileName);
    //        using (StreamReader sr = new StreamReader(langPromptBank))
    //        using (JsonReader jr = new JsonTextReader(sr))
    //        {
    //            langAvailablePromptNames = Utils.Serializer.Deserialize<List<string>>(jr) ?? new List<string>();
    //        }

    //        foreach (var name in langAvailablePromptNames)
    //        {
    //            langPromptAvailabilityDic.Add(new LangPrompt { Language = language, PromptFileName = name }, true);
    //        }

    //        langPromptAvailabilityDic.Add(langKey, true);
    //    }
    //}

    public bool EnsureAvailabilitiesLoaded(int langId)
    {
        var langKey = new LangPrompt { LangId = langId, PromptId = 0 };
        var alreadyLoaded = langPromptAvailabilityDic.ContainsKey(langKey);
        if (!alreadyLoaded)
        {
            var availablePromptLocs = DBService.I.PromptLocs
                .Where(pl => pl.LangId == langId)
                .ToList();

            foreach (var promptLoc in availablePromptLocs)
            {
                langPromptAvailabilityDic.Add(new LangPrompt { 
                    LangId = langId, 
                    PromptId = promptLoc.PromptId 
                }, true);
            }

            langPromptAvailabilityDic.Add(langKey, true);
        }

        return alreadyLoaded;
    }

    public bool GetPromptAvailabilityInLang(string promptName, string language)
    {
        var langId = LanguageManager.I.LangIdDic[language];
        EnsureAvailabilitiesLoaded(langId);
        var key = new LangPrompt(langId, PromptManager.I.PromptIdDic[promptName]);
        langPromptAvailabilityDic.TryGetValue(key, out bool available);
        return available;
    }

    public void SetPromptAvailabilityInLang(PromptLoc promptLoc, bool newAvailability)
    {
        promptLoc.Available = newAvailability;
        DBService.I.Update(promptLoc);

        if (EnsureAvailabilitiesLoaded(promptLoc.LangId))
        {
            var key = LangPrompt.FromPromptLoc(promptLoc);
            langPromptAvailabilityDic[key] = newAvailability;
        }
    }

    //public void SaveLangAvailabilitiesFile(string language)
    //{
    //    langPromptAvailabilityDic.Where(de => de.Key.Language == language && de.Value)
    //        .Select(de => de.Key.PromptName).ToList();
    //}
    #endregion Availability

    #region Fulltext
    //public bool TryGetPromptTextInLanguage(string promptName, string language, out string fullPromptText)
    //{
    //    var fileName = promptName + ".txt";
    //    fullPromptText = "";

    //    var key = new LangPrompt { Language = language, PromptFileName = fileName };
    //    if (langPromptCachedTextDic.TryGetValue(key, out fullPromptText))
    //        return true;

    //    if (TryReadPromptTextInLanguageFromFile(fileName, language, out fullPromptText))
    //    {
    //        langPromptCachedTextDic.Add(key, fullPromptText);
    //        return true;
    //    }
    //    return false;
    //}
    public bool TryGetPromptTextInLanguage(string promptName, string language, out string fullPromptText)
    {
        fullPromptText = "";

        var key = new LangPrompt { LangId = LanguageManager.I.LangIdDic[language], PromptId = PromptManager.I.PromptIdDic[promptName] };
        if (langPromptCachedTextDic.TryGetValue(key, out fullPromptText))
            return true;

        if (TryGetPromptLocFromDB(promptName, language, out PromptLoc promptLoc))
        {
            langPromptCachedTextDic.Add(key, promptLoc.Text);
            return true;
        }
        return false;
    }

    public bool DropPromptTextInLanguageFromCache(string promptName, string language)
    {
        var key = new LangPrompt { LangId = LanguageManager.I.LangIdDic[language], PromptId = PromptManager.I.PromptIdDic[promptName] };
        return langPromptCachedTextDic.Remove(key);
    }

    //public bool TryReadPromptTextInLanguageFromFile(string fileName, string language, out string fullPromptText)
    //{
    //    fullPromptText = "";
    //    var filePath = Path.Combine(Constants.PromptsDir, language, fileName);

    //    if (File.Exists(filePath))
    //    {
    //        fullPromptText = File.ReadAllText(filePath);
    //        return true;
    //    }
    //    return false;
    //}

    public bool TryGetPromptLocFromDB(string promptName, string language, out PromptLoc promptLoc)
    {
        promptLoc = DBService.I.PromptLocs.FirstOrDefault(pl 
            => pl.PromptId == PromptManager.I.PromptIdDic[promptName] 
            && pl.LangId == LanguageManager.I.LangIdDic[language]);

        return promptLoc != null;
    }
    #endregion Fulltext

    #region SQLite
    private void InitDB()
    {
        Debug.Log("začátek initDB");

        var prompts = DBService.I.Prompts.ToList();
        foreach (var prompt in prompts)
        {
            Debug.Log(prompt);
        }
    }

    #endregion SQLite
}
