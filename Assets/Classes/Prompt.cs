using System.Collections.Generic;

namespace Assets.Classes
{
    public class Prompt
    {
        public EndConvoAbility GeneralConvoEndingAbility { get; set; }
        public bool CanEndConvoThisTime { get; set; }
        public string Text { get; set; }
    }

    public class PromptInMainBank
    {
        public bool Active { get; set; }
        public string Name { get; set; }
        public EndConvoAbility GeneralConvoEndingAbility { get; set; }
    }

    public class PromptSettingsLabel : PromptInMainBank
    {
        public bool AvailableInCurrentLanguage { get; set; }
    }

    public class EditablePrompt : PromptSettingsLabel
    {
        public string Text { get; set; }
        public List<string> Tags { get; set; }
    }
}
