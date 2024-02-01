using System.Collections.Generic;

namespace Assets.Classes
{
    public interface IConversationBlock
    {
        public string Text { get; set; }
        public IEnumerable<IConversationBlock> ResponsePool { get; set; }
    }

    public class PlayerConvoBlock : IConversationBlock
    {
        public string Text { get; set; }
        public Traits Impact { get; set; }
        public IEnumerable<IConversationBlock> ResponsePool { get; set; }

        public PlayerConvoBlock(string text, Traits impact, IEnumerable<IConversationBlock> responses)
        {
            Text = text;
            Impact = impact;
            ResponsePool = responses;
        }

        //public static PlayerConvoBlock PlaceHolder => new PlayerConvoBlock("Placeholder option", new Traits(-1, 0, 0), ConversationConsts.ToBeDone);
    }
    public class NPCConvoBlock : IConversationBlock
    {
        public string Text { get; set; }
        public IEnumerable<IConversationBlock> ResponsePool { get; set; }
        public NPCConvoBlock(string text, IEnumerable<IConversationBlock> responses)
        {
            Text = text;
            ResponsePool = responses;
        }
    }
}

