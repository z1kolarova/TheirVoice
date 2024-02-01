using System;
using System.Collections.Generic;

public static partial class ConversationConsts 
{
    private static System.Random rnd = new Random();
    private static string RandomLineFrom(string[] lineSet)
    {
        return lineSet[rnd.Next(lineSet.Length)];
    }

    private static Dictionary<AnimalExploitationsInDiet, string> eatingAnimals
        = new Dictionary<AnimalExploitationsInDiet, string>() {
            [AnimalExploitationsInDiet.Chicken] = "chicken",
            [AnimalExploitationsInDiet.Cow] = "beef",
            [AnimalExploitationsInDiet.Pig] = "pork",
            [AnimalExploitationsInDiet.Fish] = "fish",
            [AnimalExploitationsInDiet.Dairy] = "dairy products",
            [AnimalExploitationsInDiet.Drink] = "milk",
            [AnimalExploitationsInDiet.Eggs] = "eggs",
            [AnimalExploitationsInDiet.Honey] = "honey",
        };
    private static string[] NPC_AlreadyPlantBased = new[] {
        "I don't eat any animal products at all.",
        "I love animals, so I don't eat them.",
        "I'm vegan, actually."
    };
    private static string[] NPC_EatsMeat = new[] {
        "I do eat meat, yeah.",
        "Of course I buy meat.",
        "I've always eaten meat. Why wouldn't I?",
        "Meat is good for you to be strong and healthy."
    };

    //public static List<ConversationBlock> P_OpeningLines = new List<ConversationBlock>() {
    //    new ConversationBlock{ Text = "Are you against animal abuse?", ReactionValue = 2},
    //    new ConversationBlock{ Text = "Do you have time?", ReactionValue = 0},
    //    new ConversationBlock{ Text = "Do you know what we're doing here?", ReactionValue = 1},
    //    new ConversationBlock{ Text = "Have you ever seen footage like this?", ReactionValue = 3},
    //};

    //public static List<ConversationBlock> P_TestingSet = new List<ConversationBlock>() {
    //    new ConversationBlock{ Text = "Option1", ReactionValue = 2},
    //    new ConversationBlock{ Text = "Option2", ReactionValue = 0},
    //    new ConversationBlock{ Text = "Option3", ReactionValue = 1},
    //    new ConversationBlock{ Text = "Option4", ReactionValue = 3},
    //};

    public static string RevealDiet(PersonalityCore person)
    {
        if (person.IsFullyPlantBased())
        {
            return RandomLineFrom(NPC_AlreadyPlantBased);
        }

        if (person.EatsMeat())
        {
            return RandomLineFrom(NPC_EatsMeat); // + " " + DiscloseEatenAnimals(person.Diet);
        }

        return "I don't know what to tell you...";
    }

    public static string DiscloseEatenAnimals(AnimalExploitationsInDiet diet)
    {
        var eats = new List<AnimalExploitationsInDiet>();
        var eatsFlags = AnimalExploitationsInDiet.None;
        var flagCount = 0;
        foreach (var item in Utilities.ValueList<AnimalExploitationsInDiet>())
        {
            if (item == AnimalExploitationsInDiet.None)
            {
                continue;
            }
            if (AnimalExploitationsInDiet.Flesh.HasFlag(item) && diet.HasFlag(item))
            {
                eats.Add(item & (~eatsFlags));
                eatsFlags = eatsFlags & eats[flagCount];
                flagCount++;
            }
        }
        var sentence = flagCount > 2 ? "I eat all kinds of meat: " : "But only ";


        for (int i = 0; i < flagCount; i++)
        {
            if (i == flagCount - 2 && i > 0)
            {
                sentence += " and ";
            }
            sentence += eatingAnimals[eats[i]];
            if (i < flagCount - 2 && i > 0)
            {
                sentence += ", ";
            }
        }

        sentence += ".";
        return sentence;
    }
}
