namespace Assets.Structs
{
    public struct LangPrompt
    {
        public int LangId;
        public int PromptId;

        public LangPrompt(int langId, int promptId)
        {
            LangId = langId;
            PromptId = promptId;
        }

        public static LangPrompt FromPromptLoc(PromptLoc promptLoc)
            => new LangPrompt(promptLoc.LangId, promptLoc.PromptId);
    }
}
