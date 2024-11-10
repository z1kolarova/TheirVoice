using Assets.Classes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonalityGenerator : MonoBehaviour
{
    public static PersonalityGenerator I => instance;
    static PersonalityGenerator instance;

    [Range(0, 100)] public int EndingConversationAbilityChance;

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

    private int rangeMax = 100;
    private List<(AnimalExploitationsInDiet, int)> _tupleList;
    private List<PromptLabel> promptBank;
    private List<PromptLabel> availablePool = new List<PromptLabel>();
    private Dictionary<string, int> currentlyInScene;

    private void Start()
    {
        instance = this;
         _tupleList = new List<(AnimalExploitationsInDiet, int)>(){
            (AnimalExploitationsInDiet.Chicken, ChickenOdds),
            (AnimalExploitationsInDiet.Cow, CowOdds),
            (AnimalExploitationsInDiet.Pig, PigOdds),
            (AnimalExploitationsInDiet.Fish, FishOdds),
            (AnimalExploitationsInDiet.Dairy, DairyOdds),
            (AnimalExploitationsInDiet.Eggs, EggsOdds),
            (AnimalExploitationsInDiet.Honey, HoneyOdds),
        };

        //var newPropmtText = "" +
        //    "General desired response format:\r\nTry to mimic a spoken conversation.\r\nBe informal and use short sentencens. Between 1 and 3 sentences is the ideal response length, but you can rarely go for up to 6 sentences.\r\nDon't entertain hypotheticals.\r\nBring up your opinions in a way that fits. Be vocal about meat being healthy.\r\nBe impolite and rude if you feel attacked or when you get asked the same question again. You can even reach to insults.\r\nDon't assume the gender of the person you're responding to.\r\n\r\nContext:\r\nYou were on your way home from work when you noticed people with TV screens that play video footage of animals in slaughterhouses. You stopped to watch for a bit and one of them approached you.\r\n\r\nYou're tired after a long day, looking forward to enjoying a steak for dinner and this really ruins your mood. Yes, animals are suffering but people need to eat, big deal. Meat is healthy, unlike the superprocessed vegan substitute nonsense . Meat will always be better and there's no reason to feel guilty about it." +
        //    "";
        //var mightEverEndConvo = EndConvoAbility.Always;
        //ArgumentationTag tags = ArgumentationTag.Nutrition | ArgumentationTag.HumanSuperiority;
        
        //var newPrompt = new Prompt { Text = newPropmtText, EndConvoAbility = mightEverEndConvo, Excuses = tags };
        //ConvoUtilsGPT.SerializePrompt(newPrompt, "guiltfree_meat_is_healthy_and_it_is_what_it_is");

        //var text2 = "" +
        //    "You're a tattoo artist on your way home. You noticed people with TV screens that play quite graphic video footage of animals on farms and in slaughterhouses. You stopped to watch for a bit and one activist approached you.\r\n\r\nYou eat meat, but your girlfriend is vegetarian, so you've had some discussions about animals and the meat industry before.\r\nEating meat is natural - lions eat gazelles, snakes eat mice... And humans have teeth for chewing meat.\r\nBring up your reasons for eating meat.\r\n\r\nWhile your girlfriend loves animals, you don't really care about them.\r\n\r\nBring up your personal life - your girlfriend, how she respects that you eat meat even though she doesn't.\r\n\r\nGeneral desired response format:\r\nBe informal and use short sentencens. 1 or 2 sentences is the ideal response length, but you can rarely go even for 4 sentences.\r\nTry to mimic a spoken conversation." +
        //    "";
        //var atec2 = EndConvoAbility.Sometimes;
        //ArgumentationTag tags2 = ArgumentationTag.ItsNature;

        //var prompt2 = new Prompt { Text = text2, EndConvoAbility = atec2, Excuses = tags2 };
        //ConvoUtilsGPT.SerializePrompt(prompt2, "meateater_with_vegetarian_girlfriend");

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
        var diet = AddFlagsIfOdds(AnimalExploitationsInDiet.None);
        var promptLabel = availablePool.Count > 0 
            ? availablePool[RngUtils.Rng.Next(availablePool.Count)] 
            : promptBank[RngUtils.Rng.Next(promptBank.Count)];

        var promptText = ConvoUtilsGPT.GetPromptTextByLabel(promptLabel);

        var prompt = ConvoUtilsGPT.CreatePrompt(promptText, promptLabel.EndConvoAbility, EndingConversationAbilityChance);
        
        var pc = new PersonalityCore()
        {
            PromptLabel = promptLabel,
            Prompt = prompt,
            Traits = new Traits(RngUtils.Rng.Next(MaxPatience), RngUtils.Rng.Next(MaxBaseAwareness), RngUtils.Rng.Next(MaxBaseCompassion)),
            Diet = diet,
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

    private AnimalExploitationsInDiet AddFlagsIfOdds(AnimalExploitationsInDiet addingTo)
    {
        foreach (var tuple in _tupleList)
        {
            if (RngUtils.RollWithinLimitCheck(tuple.Item2, rangeMax))
            {
                addingTo = addingTo | tuple.Item1;
            }
        }
        return addingTo;
    }

}
