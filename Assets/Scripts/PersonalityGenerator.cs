using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalityGenerator : MonoBehaviour
{
    public static PersonalityGenerator I => instance;
    static PersonalityGenerator instance;

    [Header("Traits")]
    public int MaxPatience = 10;
    public int MaxBaseAwareness = 100;
    public int MaxBaseCompassion = 100;

    [Header("Diet")]
    [Range(0, 100)] public int ChickenOdds;
    [Range(0, 100)] public int CowOdds;
    [Range(0, 100)] public int PigOdds;
    [Range(0, 100)] public int FishOdds;
    [Range(0, 100)] public int DairyOdds;
    [Range(0, 100)] public int EggsOdds;
    [Range(0, 100)] public int HoneyOdds;

    public static string PromptsDir = "./Assets/Prompts/";
    public static string PromptBankFileName = "_PromptBank";

    private static System.Random rng;
    private int rangeMax = 100;
    private List<(AnimalExploitationsInDiet, int)> _tupleList;
    private List<string> promptFileNames;

    private void Start()
    {
        instance = this;
        rng = new System.Random();
         _tupleList = new List<(AnimalExploitationsInDiet, int)>(){
            (AnimalExploitationsInDiet.Chicken, ChickenOdds),
            (AnimalExploitationsInDiet.Cow, CowOdds),
            (AnimalExploitationsInDiet.Pig, PigOdds),
            (AnimalExploitationsInDiet.Fish, FishOdds),
            (AnimalExploitationsInDiet.Dairy, DairyOdds),
            (AnimalExploitationsInDiet.Eggs, EggsOdds),
            (AnimalExploitationsInDiet.Honey, HoneyOdds),
        };

        //var newPropmt = "You were on your way home from work when you noticed people with TV screens that play video footage of animals in slaughterhouses. You stopped to watch for a bit and one of them approached you.\r\nYou're tired after a long day and not really in the mood to talk. Yes, animals are suffering but people need to eat. Meat is healthy, not unlike the superprocessed substitute nonsense that vegans eat. You will never go vegan.\r\nTry to mimic a spoken conversation.\r\nKeep your responses short and to the point. Don't assume the gender of the person you're responding to.";
        
        //ConvoUtilsGPT.SerializePrompt(newPropmt, "unaproachable_itiswhatitis");

        promptFileNames = ConvoUtilsGPT.GetPromptBank();
    }

    public PersonalityCore GetNewPersonality()
    {
        var diet = AddFlagsIfOdds(AnimalExploitationsInDiet.None);
        var promptName = promptFileNames[rng.Next(promptFileNames.Count)];
        var prompt = ConvoUtilsGPT.GetPromptByFileName(promptName);
        
        var pc = new PersonalityCore()
        {
            Traits = new Traits(rng.Next(MaxPatience), rng.Next(MaxBaseAwareness), rng.Next(MaxBaseCompassion)),
            Diet = diet,
            PersonalityPrompt = prompt
        };

        return pc;
    }

    private AnimalExploitationsInDiet AddFlagsIfOdds(AnimalExploitationsInDiet addingTo)
    {
        foreach (var tuple in _tupleList)
        {
            if (rng.Next(rangeMax) <= tuple.Item2)
            {
                addingTo = addingTo | tuple.Item1;
            }
        }
        return addingTo;
    }

}
