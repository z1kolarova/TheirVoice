using Assets.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class ConversationConsts
{
    public static List<PlayerConvoBlock> Sample_P_HowDoesThisMakeYouFeel = new List<PlayerConvoBlock>() {
        new PlayerConvoBlock("Hello. Have you ever seen footage like this?", new Traits(-1, 1, 1), Sample_NPC_1),
        new PlayerConvoBlock("Hello. How does seeing this footage make you feel?", new Traits(-1, 1, 1), Sample_NPC_1),
        new PlayerConvoBlock("Hello. How do you feel when you see these videos?", new Traits(-1, 1, 1), Sample_NPC_1),
        new PlayerConvoBlock("Hello. Can you tell me what's your reaction when you these videos?", new Traits(-1, 1, 1), Sample_NPC_1_1),
    };

    public static List<NPCConvoBlock> Sample_NPC_1 = new List<NPCConvoBlock>() {
        new NPCConvoBlock("I just can't watch this stuff! " +
            "It always makes me really sad so I avoid watching anything like that.", Sample_P_WereShowingStandardPractices),
    };

    public static List<NPCConvoBlock> Sample_NPC_1_1 = new List<NPCConvoBlock>() {
        new NPCConvoBlock("It's really horrible and that place should be shut down.", Sample_P_WereShowingStandardPractices),
    };

    public static List<PlayerConvoBlock> Sample_P_WereShowingStandardPractices = new List<PlayerConvoBlock>() {
        new PlayerConvoBlock("This is footage of the standard practices that happen every day all over the world " +
            "in the industries that abuse animals. Would you say you're against animal abuse?", new Traits(-1, 1, 1), Sample_NPC_2),
        new PlayerConvoBlock("We're showing what goes on  in the meat, dairy, egg, fishing and other industries that involve animals. " +
            "Do you think this treatment of animals is cruel?", new Traits(-1, 1, 1), Sample_NPC_2),
        new PlayerConvoBlock("These videos were taken in real farms, slaughterhouses and other places. " +
            "All of this is legal and meets the welfare standards. Are you against animal abuse?", new Traits(-1, 1, 1), Sample_NPC_2),
        new PlayerConvoBlock("What all the clips have in common is that they show the reality behind animal products. " +
            "Meat, eggs, dairy, fur. Is animal abuse something you're against?", new Traits(-1, 1, 1), Sample_NPC_2),
    };

    public static List<NPCConvoBlock> Sample_NPC_2 = new List<NPCConvoBlock>() {
        new NPCConvoBlock("Yes. Of course I'm against animal abuse!", ToBeDonePlayer),
    };

    public static List<PlayerConvoBlock> Sample_P_CanYouBeAgainstWhileFunding = new List<PlayerConvoBlock>() {
        new PlayerConvoBlock("Do you think it's possible to be against animal abuse while consuming animal products?",
            new Traits(-1, 1, 1), Sample_NPC_3_1),
        new PlayerConvoBlock("Would you say one can be against animal cruelty while buying the products of it?",
            new Traits(-1, 1, 1), Sample_NPC_3_1),
        new PlayerConvoBlock("Is it possible to be against animal abuse and still consume meat and other animal products?",
            new Traits(-1, 1, 1), Sample_NPC_3_1),
        new PlayerConvoBlock("Is in your opinion paying money to buy animal products aligned with being against animal abuse?",
            new Traits(-1, 1, 1), Sample_NPC_3_2),
    };

    public static List<NPCConvoBlock> Sample_NPC_3_1 = new List<NPCConvoBlock>() {
        new NPCConvoBlock("Yes. Of course it is! I don't eat meat because I WANT the animals to be killed. " +
            "Believe me, if I could get meat without the animals dying I would. But until lab-grown meat becomes common," +
            "I'll stick with buying from local free-range farms.", Sample_P_LocalFreeRange),
    };
    private static List<NPCConvoBlock> Sample_NPC_3_2 = new List<NPCConvoBlock>() {
        new NPCConvoBlock("That's a very interesting question. I hate to say it but I guess not. " +
            "But I also don't think we can just stop consuming them.", ToBeDonePlayer),
    };

    public static List<PlayerConvoBlock> Sample_P_LocalFreeRange = new List<PlayerConvoBlock>() {
        new PlayerConvoBlock("But free-range is barely better than others. The footage we have here includes free-range, " +
            "all kinds of certified and humane. Can you think of a humane way I could kill you?",
            new Traits(-1, 1, 1), Sample_NPC_3_1),
        new PlayerConvoBlock("The animals are still killed and you're paying for that to keep happening. Does your ability to eat meat " +
            "have more value than their lives?",
            new Traits(-1, 1, 1), Sample_NPC_3_1),
        new PlayerConvoBlock("Free-range cows still end up in a slaughterhouse with a boltgun to their head. " +
            "Would you be ok with a slightly better life if you were still going to end up killed at a fraction of your lifespan?",
            new Traits(-1, 1, 1), Sample_NPC_3_1),
        new PlayerConvoBlock("What is your motivation for eating meat?",
            new Traits(-1, 1, 1), Sample_NPC_3_1),
    };
}
