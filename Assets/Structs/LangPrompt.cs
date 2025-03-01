namespace Assets.Structs
{
    public struct LangPrompt
    {
        public string Language;
        public string PromptFileName;

        public LangPrompt(string language, string promptFileName)
        {
            Language = language;
            PromptFileName = promptFileName;
        }
    }
}
