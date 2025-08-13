using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonalityGenerator : MonoBehaviour
{
    public static PersonalityGenerator I => instance;
    static PersonalityGenerator instance;

    [Range(0, 100)] public int EndingConversationAbilityChance;

    private Dictionary<string, int> currentlyInScene;

    private List<Prompt> promptBank;
    private List<Prompt> availablePool;

    private void Start()
    {
        instance = this;

        promptBank = ClientDataManager.I.LangIdPromptDic[UserSettingsManager.I.ConversationLanguage.Id];

        currentlyInScene = new Dictionary<string, int>();
        foreach (var prompt in promptBank)
        {
            currentlyInScene.Add(prompt.Name, 0);
        }

        availablePool = new List<Prompt>(promptBank);
    }

    #region reworked prompts
    public PersonalityCore GetNewPersonality()
    {
        var prompt = availablePool.Count > 0
            ? availablePool[RngUtils.Rng.Next(availablePool.Count)]
            : promptBank[RngUtils.Rng.Next(promptBank.Count)];

        var promptText = ConvoUtilsGPT.GetPromptTextInCurrentLanguage(prompt.Id);

        var pc = new PersonalityCore()
        {
            Prompt = prompt
        };

        if (availablePool.Count > 0)
        {
            availablePool.Remove(prompt);
        }

        currentlyInScene[prompt.Name]++;

        return pc;
    }

    public void RemoveFromPromptsInScene(Prompt prompt)
    {
        currentlyInScene[prompt.Name]--;
        if (currentlyInScene[prompt.Name] == 0)
        {
            availablePool.Add(prompt);
        }
    }
    #endregion reworked prompts
}
