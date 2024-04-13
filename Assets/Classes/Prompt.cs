namespace Assets.Classes
{
    public class Prompt
    {
        public string Text { get; set; }
        public EndConvoAbility EndConvoAbility { get; set; }
        public ArgumentationTag Excuses { get; set; } = ArgumentationTag.None;
    }
}

