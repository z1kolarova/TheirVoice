using Assets.Classes;

public static class Constants
{
    public static string PromptsDir = "./Assets/Prompts/";
    public static string PromptBankFileName = "_PromptBank.json";
    public static int USER_MSG_CHAR_LIMIT = 500;
    public static string CONVO_END_STRING = "#END_OF_CONVO#";
    public static string CONVO_END_INSTRUCTION = $"\r\nYou can choose to end the conversation whenever you decide (to end the conversation, append \"{CONVO_END_STRING}\" to the last message).";
    public static string NOT_INTERESTED_PROMPT_NAME = "_wasnt_watching_footage";
    public static ConversationModes ConversationMode = ConversationModes.RealGPT;
}
