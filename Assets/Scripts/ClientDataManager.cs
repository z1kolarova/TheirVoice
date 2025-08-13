using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClientDataManager : MonoBehaviour
{
    public static ClientDataManager I => instance;
    static ClientDataManager instance;
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

    #region languages
    private List<Language> languages;
    public List<Language> Languages { get { return languages; } }

    public void SetLanguages(Language[] availableLanguages)
    {
        languages = availableLanguages.ToList();
    }
    #endregion languages

    #region system prompts
    private Dictionary<string, int> systemPromptNameIdDic = new Dictionary<string, int>();
    public Dictionary<string, int> SystemPromptNameIdDic { get { return systemPromptNameIdDic; } }

    public void AddSystemPrompt(string systemPromptName, int promptId)
    {
        SystemPromptNameIdDic[systemPromptName] = promptId;
    }
    #endregion system prompts

    #region available prompts in languages
    // key = langId
    // value = list of prompts that can be used AND are available in that language

    private Dictionary<int, List<Prompt>> langIdPromptDic = new Dictionary<int, List<Prompt>>();
    public Dictionary<int, List<Prompt>> LangIdPromptDic { get { return langIdPromptDic; } }

    public void AddPrompts(int langId, IEnumerable<Prompt> prompts)
    {
        LangIdPromptDic[langId] = prompts.ToList();
    }
    #endregion available prompts in languages

    #region promptLocs
    // key = (promptId, langId)
    // value = text of corresponding PromptLoc

    private Dictionary<(int, int), string> promptIdLangIdTextDic = new Dictionary<(int, int), string>();
    public Dictionary<(int, int), string> PromptLocTextDic { get { return promptIdLangIdTextDic; } }

    public void AddPromptLoc(int promptId, int langId, string promptLocText)
    {
        PromptLocTextDic[(promptId, langId)] = promptLocText;
    }
    #endregion promptLocs
}
