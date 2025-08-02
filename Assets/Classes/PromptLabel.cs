using Assets.Enums;

namespace Assets.Classes
{
    public class PromptLabel
    {
        public string Name { get; set; }
        public EndConvoAbility EndConvoAbility { get; set; }

        public PromptLabel()
        {
        }
        public PromptLabel(string name, EndConvoAbility endConvoAbility)
        {
            Name = name;
            EndConvoAbility = endConvoAbility;
        }
    }
}

