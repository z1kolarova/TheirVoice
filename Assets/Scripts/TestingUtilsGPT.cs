using Assets.Classes;
using Assets.Enums;
using System.IO;
using System.Linq;

public static class TestingUtilsGPT
{
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

    public static bool Decide(this EndConvoAbility endConvoAbility) 
        => endConvoAbility switch
            { 
                EndConvoAbility.Always => true,
                EndConvoAbility.Never => false,
                _ => RngUtils.RollWithinLimitCheck(ConvoUtilsGPT.ConvoEndOddsIfSometimes)
            };

    public static EnrichedPromptLocText EnrichText(this string promptText, bool canEndConvo,
        string endConvoInstruction)
    {
        if (canEndConvo)
        {
            promptText += endConvoInstruction;
        }

        return new EnrichedPromptLocText
        {
            CanEndConvoThisTime = canEndConvo,
            FullyAssembledText = promptText
        };
    }

    #region test file exports
    public static string GetTestingExportDirectory(this string languageName)
        => Path.Combine(Constants.TestConvoOutputDir, languageName);

    public static string TestReportFileName(this string promptName)
        => promptName + Utils.GetNowFileTimestamp() + ".txt";

    public static void ExportPasserbyReportFile(this TestingConvo testingConvo)
    {
        Utils.WriteFileContents(testingConvo.TestedLanguage.GetTestingExportDirectory(),
            testingConvo.Passerby.PromptName.TestReportFileName(),
            testingConvo.Passerby.ConversationHistory.TranscribeConversation());
    }

    #region outreacher full text
    public static void ExportOutreacherIfNeeded(this TestingConvo testingConvo)
    {
        if (testingConvo.NeedsToLogOutreacher())
        {
            testingConvo.ExportTestOutreacherToFile();
            testingConvo.ToggleNeedToExportOutreacher();
        }
    }

    public static bool NeedsToLogOutreacher(this TestingConvo testingConvo)
        => DBServiceUtils.NeedsToLogFull(
            PromptManager.GetPromptId(testingConvo.Outreacher.PromptName),
            LanguageManager.GetLangId(testingConvo.TestedLanguage));

    public static bool NeedsToLogTestOutreacher(int langId)
        => DBServiceUtils.NeedsToLogFull(PromptManager.GetPromptId(Constants.TESTING_PROMPT_NAME), langId);

    public static void ExportTestOutreacherToFile(this TestingConvo testingConvo)
    {
        if (PromptManager.I.TryGetPromptTextInLanguage(testingConvo.Outreacher.PromptName,
            testingConvo.TestedLanguage, out string systemPromptText))
        {
            var dirPath = testingConvo.TestedLanguage.GetTestingExportDirectory();
            var fileName = testingConvo.Outreacher.PromptName.TestReportFileName();
            Utils.WriteFileContents(dirPath, fileName, systemPromptText);
        }
    }

    private static void ToggleNeedToExportOutreacher(this TestingConvo testingConvo)
    {
        var promptId = PromptManager.GetPromptId(testingConvo.Outreacher.PromptName);
        var langId = LanguageManager.GetLangId(testingConvo.TestedLanguage);

        var ptl = DBService.I.PromptTestLoggings.FirstOrDefault(ptl
            => ptl.PromptId == promptId
            && ptl.LangId == langId);

        DBServiceUtils.ToggleNeedToLogFull(ptl);
    }
    #endregion outreacher full text  
    
    #endregion test file exports
}
