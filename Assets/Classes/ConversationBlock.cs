namespace Assets.Classes
{
    public interface IConversationBlock
    {
        public string Text { get; set; }
        public string ResponsePoolName { get; set; }
        public bool EndsConvo { get; set; }
    }

    public class PlayerConvoBlock : IConversationBlock
    {
        public string Text { get; set; }
        public string ResponsePoolName { get; set; }
        public bool EndsConvo { get; set; } = false;
    }

    public class NPCConvoBlock : IConversationBlock
    {
        public string Text { get; set; }
        public string ResponsePoolName { get; set; }
        public bool EndsConvo { get; set; } = false;
    }
}

