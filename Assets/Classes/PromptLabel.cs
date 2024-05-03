namespace Assets.Classes
{
    public class PromptLabel
    {
        public string Name { get; set; }
        public EndConvoAbility EndConvoAbility { get; set; }
        public ArgumentationTag Tags { get; set; } = ArgumentationTag.None;

        public PromptLabel()
        {
        }
        public PromptLabel(string name, EndConvoAbility endConvoAbility, ArgumentationTag tags)
        {
            Name = name;
            EndConvoAbility = endConvoAbility;
            Tags = tags;
        }
    }
}

