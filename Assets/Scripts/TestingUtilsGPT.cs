using Assets.Classes;
using Assets.Enums;

public static class TestingUtilsGPT
{
    public static TestingConvoParticipant Outreacher = new TestingConvoParticipant();
    public static TestingConvoParticipant Passerby = new TestingConvoParticipant();

    public static bool TestingInProgress = false;

    public static void StartConversationAsOutreacher()
    {
        Outreacher.RequestResponseTo(GetPersonDescribingMessage());
        TestingInProgress = true;
    }

    public static void InitTestConversations(string outreacherSystemMessage, string passerbySystemMessage)
    {
        Outreacher.ResetWithSystemMessage(outreacherSystemMessage);
        Passerby.ResetWithSystemMessage(passerbySystemMessage);
    }

    public static void GetOutreacherResponseTo(string msgText)
    {
        Outreacher.RequestResponseTo(msgText);
    }
    public static void GetPasserbyResponseTo(string msgText)
    {
        Passerby.RequestResponseTo(msgText);
    }

    public static string GetPersonDescribingMessage()
    {
        return "-a person stopped and is watching the footage-";
    }

    public static EnrichedPromptLocText EnrichText(this string promptText, EndConvoAbility endConvoAbility,
        string endConvoInstruction, int chanceIfSometimes = 50)
    {
        bool willBeAbleToEndConvo = false;
        switch (endConvoAbility)
        {
            case EndConvoAbility.Never:
                break;
            case EndConvoAbility.Sometimes:
                if (RngUtils.RollWithinLimitCheck(chanceIfSometimes))
                {
                    willBeAbleToEndConvo = true;
                    promptText += endConvoInstruction;
                }
                break;
            case EndConvoAbility.Always:
                willBeAbleToEndConvo = true;
                promptText += endConvoInstruction;
                break;
        }

        return new EnrichedPromptLocText
        {
            CanEndConvoThisTime = willBeAbleToEndConvo,
            FullyAssembledText = promptText
        };
    }

    public static void ExportConversationToFile(this TestingConvoParticipant participant, string dirPath, string fileName)
    {
        Utils.WriteFileContents(dirPath, fileName, 
            participant.ConversationHistory.TranscribeConversation());
    }
}
