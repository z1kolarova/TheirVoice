using Assets.Classes;
using System.Collections.Generic;

public static class ConversationConsts {
    public static string[] tempDialogSentences = new[] { "plants feel pain", "lions though" };

    public static List<ConversationBlock> openingLines = new List<ConversationBlock>() {
        new ConversationBlock{ Text = "Are you against animal abuse?", ReactionValue = 2},
        new ConversationBlock{ Text = "Do you have time?", ReactionValue = 0},
        new ConversationBlock{ Text = "Do you know what we're doing here?", ReactionValue = 1},
        new ConversationBlock{ Text = "Have you ever seen footage like this?", ReactionValue = 3},
    };
}
