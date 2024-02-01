using Assets.Classes;
using System.Collections.Generic;

public static partial class ConversationConsts
{
    public static List<PlayerConvoBlock> P_toBeDone = new List<PlayerConvoBlock>() {
        new PlayerConvoBlock("Option1", new Traits(0,0,0), ToBeDone),
        new PlayerConvoBlock("Option2", new Traits(0,0,0), ToBeDone),
        new PlayerConvoBlock("Option3", new Traits(0,0,0), ToBeDone),
        new PlayerConvoBlock("Option4", new Traits(0,0,0), ToBeDone)
    };

    public static List<PlayerConvoBlock> P_OpeningLines = new List<PlayerConvoBlock>() {
        new PlayerConvoBlock("Are you against animal abuse?", new Traits(-1, 0, 1), npcAreYouAgainstAnimalAbuse),
        new PlayerConvoBlock("Do you have time?", new Traits(-1, 0, 0), npcDoYouHaveTime),
        new PlayerConvoBlock("Do you know what we're doing here?", new Traits(-1, 1, 0), npcKnowWhatWereDoing),
        new PlayerConvoBlock("Have you ever seen footage like this?", new Traits(-1, 1, 1), npcHaveYouSeenFootage),
        new PlayerConvoBlock("Hello. How does seeing this footage make you feel?", new Traits(-1, 1, 1), npcHowDoesFootageMakeYouFeel),
        new PlayerConvoBlock("Hello. How do you feel when you see these videos?", new Traits(-1, 1, 1), npcHowDoesFootageMakeYouFeel),
    };

    public static List<PlayerConvoBlock> P_AreYouAgainstAnimalAbuse = new List<PlayerConvoBlock>() {
        new PlayerConvoBlock("Are you against animal abuse?", new Traits(-1, 0, 1), npcAreYouAgainstAnimalAbuse),
        new PlayerConvoBlock("So would you say you're against animal cruelty?", new Traits(-1, 0, 1), npcAreYouAgainstAnimalAbuse),
        new PlayerConvoBlock("Do you consider yourself against animal cruelty?", new Traits(-1, 0, 1), npcAreYouAgainstAnimalAbuse),
    };

    public static List<PlayerConvoBlock> P_CanYouBeAgainstItWhilePayingForIt = new List<PlayerConvoBlock>() {
        new PlayerConvoBlock("Do you think it's possible to be against animal abuse while eating animal products and funding this?", new Traits(-1, 0, 1), npcAreYouAgainstAnimalAbuse),
        new PlayerConvoBlock("So would you say you're against animal cruelty?", new Traits(-1, 0, 1), npcAreYouAgainstAnimalAbuse),
    };
}
