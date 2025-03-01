using System.Collections.Generic;

namespace Assets.Classes
{
    public class Prompt
    {
        public EndConvoAbility GeneralConvoEndingAbility { get; set; }
        public bool CanEndConvoThisTime { get; set; }
        public string Text { get; set; }
    }

    public class PromptInMainBank : MinimalPromptSkeleton
    {
        public bool Active { get; set; }
    }

    //public class CompletePrompt
    //{
    //    string fileName;
    //    string language;
    //    string promptText;
    //    EndConvoAbility endConvoAbility;
    //    List<string> tags;
    //}

    public class MinimalPromptSkeleton
    {
        public string Name { get; set; }
        public EndConvoAbility EndConvoAbility { get; set; }
    }

    public class LanguageSpecificPrompt : MinimalPromptSkeleton
    {
        public string Language { get; set; }
        public string PromptText { get; set; }
    }

    public class PromptEntryContent : MinimalPromptSkeleton
    {
        public bool Active { get; set; }
        public bool AvailableInCurrentLanguage { get; set; }
    }
}
