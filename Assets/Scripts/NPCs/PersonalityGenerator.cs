using Assets.Classes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonalityGenerator : MonoBehaviour
{
    public static PersonalityGenerator I => instance;
    static PersonalityGenerator instance;

    [Range(0, 100)] public int EndingConversationAbilityChance;

    private int rangeMax = 100;
    private List<PromptLabel> promptBank;
    private List<PromptLabel> availablePool = new List<PromptLabel>();
    private Dictionary<string, int> currentlyInScene;

    private void Start()
    {
        instance = this;

        promptBank = ConvoUtilsGPT.GetPromptBank();

        currentlyInScene = new Dictionary<string, int>();
        foreach (var promptLabel in promptBank)
        {
            currentlyInScene.Add(promptLabel.Name, 0);
        }

        availablePool = new List<PromptLabel>(promptBank);
    }

    public PersonalityCore GetNewPersonality()
    {
        var promptLabel = availablePool.Count > 0 
            ? availablePool[RngUtils.Rng.Next(availablePool.Count)] 
            : promptBank[RngUtils.Rng.Next(promptBank.Count)];

        var promptText = ConvoUtilsGPT.GetPromptTextByLabel(promptLabel);

        var prompt = ConvoUtilsGPT.CreatePrompt(promptText, promptLabel.EndConvoAbility, EndingConversationAbilityChance);
        
        var pc = new PersonalityCore()
        {
            PromptLabel = promptLabel,
            Prompt = prompt,
        };

        if (availablePool.Count > 0)
        {
            availablePool.Remove(promptLabel);
        }

        currentlyInScene[promptLabel.Name]++;

        return pc;
    }

    public void RemoveFromPromptLabelsInScene(PromptLabel promptLabel)
    {
        currentlyInScene[promptLabel.Name]--;
        if (currentlyInScene[promptLabel.Name] == 0)
        {
            availablePool.Add(promptLabel);
        }
    }
}
