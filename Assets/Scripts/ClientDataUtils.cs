using Assets.Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public static class ClientDataUtils
{
    #region available languages
    public static DataRequester AvailableLangsRequester = new DataRequester();

    public static void ProcessAvailableLanguages(string serialisedAvailableLanguages)
    {
        var languages = JsonConvert.DeserializeObject<Language[]>(serialisedAvailableLanguages);
        Debug.Log("I got this many languages: " + languages.Length);
        ClientDataManager.I.SetLanguages(languages);
        AvailableLangsRequester.UpdateDataReceivedAndProcessed();
    }
    #endregion available languages

    #region system prompts
    public static DataRequester SystemPromptsRequester = new DataRequester();

    public static void ProcessSystemPrompts(string serialisedSystemPrompts)
    {
        var prompts = JsonConvert.DeserializeObject<Prompt[]>(serialisedSystemPrompts);
        Debug.Log("I got this many system prompts: " + prompts.Length);
        foreach (var prompt in prompts)
        {
            ClientDataManager.I.AddSystemPrompt(prompt.Name, prompt.Id);
        }
        SystemPromptsRequester.UpdateDataReceivedAndProcessed();

        RequestSystemPromptLocsForCurrentLanguage();

        /*TODO: Currently system prompts are only requested after language is selected.
         * They could be requested earlier, but localisation of system prompts needs to
         * wait for language.
         */
    }

    public static void RequestSystemPromptLocsForCurrentLanguage()
    {
        foreach (var promptId in ClientDataManager.I.SystemPromptNameIdDic.Values)
        {
            PromptLocRequester.RequestData((promptId, UserSettingsManager.I.ConversationLanguage.Id));
        }
    }

    public static int GetSystemPromptId(string promptName)
        => ClientDataManager.I.SystemPromptNameIdDic[promptName];
    #endregion system prompts

    #region available prompts
    public static DataRequester AvailablePromptsRequester = new DataRequester();

    public static void ProcessAvailablePrompts(string serialisedAvailablePrompts)
    {
        var prompts = JsonConvert.DeserializeObject<Prompt[]>(serialisedAvailablePrompts);
        Debug.Log("I got this many prompts: " + prompts.Length);
        ClientDataManager.I.AddPrompts(UserSettingsManager.I.ConversationLanguage.Id, prompts);
        AvailablePromptsRequester.UpdateDataReceivedAndProcessed();
    }
    #endregion available prompts

    #region promptLocs
    public static MultiDataRequester<(int, int)> PromptLocRequester 
        = new MultiDataRequester<(int, int)>();

    private static Dictionary<(int, int), List<string>> chunkStorageDic = new Dictionary<(int, int), List<string>>();
    public static Dictionary<(int, int), List<string>> ChunkStorageDic => chunkStorageDic;

    public static void CachePromptLocText(int promptId, int langId)
    {
        var promptLocText = ChunkStorageDic[(promptId, langId)].DechunkStringChunks();
        ClientDataManager.I.AddPromptLoc(promptId, langId, promptLocText);
        PromptLocRequester.UpdateDataReceivedAndProcessed((promptId, langId));
    }
    #endregion promptLocs
}
