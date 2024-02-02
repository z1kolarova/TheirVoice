using Assets.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class ConversationConsts
{
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

    private static List<NPCConvoBlock> npcAreYouAgainstAnimalAbuse = new List<NPCConvoBlock>() {
        new NPCConvoBlock("Yes, of course I am!", ToBeDonePlayer),
        new NPCConvoBlock("Of course! I love animals and I can't stand to see them getting hurt!", ToBeDonePlayer),
        new NPCConvoBlock("I don't do any protests like you or anything, but yeah.", ToBeDonePlayer),
        new NPCConvoBlock("I don't care about animals.", ToBeDonePlayer),
        new NPCConvoBlock("I'd never hurt an animal. I have a rescue dog and he's my best friend.", ToBeDonePlayer),
    };

    private static List<NPCConvoBlock> npcDoYouHaveTime = new List<NPCConvoBlock>()
    {
        new NPCConvoBlock("Sorry, I'm in a rush.", ToBeDonePlayer),
        new NPCConvoBlock("Uh... sure. What is this about?", ToBeDonePlayer),
    };

    private static List<NPCConvoBlock> npcKnowWhatWereDoing = new List<NPCConvoBlock>()
    {
        new NPCConvoBlock("Sorry, I'm in a rush.", ToBeDonePlayer),
        new NPCConvoBlock("No and I'm not interested.", ToBeDonePlayer),
        new NPCConvoBlock("You're protesting for animals?", ToBeDonePlayer),
        new NPCConvoBlock("No, what's going on here?", ToBeDonePlayer),
    };

    private static List<NPCConvoBlock> npcHaveYouSeenFootage = new List<NPCConvoBlock>()
    {
        new NPCConvoBlock("I have, but I can't watch it. I hate seeing videos like that.", ToBeDonePlayer),
        new NPCConvoBlock("No. Never. It's really horrible.", ToBeDonePlayer),
        new NPCConvoBlock("Yeah. It's really important to make the standards better and make the farmers treat the animals better.", ToBeDonePlayer),
        new NPCConvoBlock("No and I don't want to. It makes me sick.", ToBeDonePlayer),
        new NPCConvoBlock("Makes me hungry.", ToBeDonePlayer),
    };

    private static List<NPCConvoBlock> npcHowDoesFootageMakeYouFeel = new List<NPCConvoBlock>()
    {
        new NPCConvoBlock("It's disgusting.", ToBeDonePlayer),
        new NPCConvoBlock("I feel sad.", ToBeDonePlayer),
        new NPCConvoBlock("Yeah. It's really important to make the standards better and make the farmers treat the animals better.", ToBeDonePlayer),
        new NPCConvoBlock("No and I don't want to. It makes me sick.", ToBeDonePlayer),
        new NPCConvoBlock("Makes me hungry.", ToBeDonePlayer),
    };
}
