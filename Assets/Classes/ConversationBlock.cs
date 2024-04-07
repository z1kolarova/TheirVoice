using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Assets.Classes
{
    public interface IConversationBlock
    {
        public string Text { get; set; }
    }

    public class PlayerConvoBlock : IConversationBlock
    {
        public string Text { get; set; }
        public Traits Impact { get; set; }
        public List<NPCConvoBlock> ResponsePool { get; set; }

        public PlayerConvoBlock(string text, Traits impact, List<NPCConvoBlock> responses)
        {
            Text = text;
            Impact = impact;
            ResponsePool = responses;
        }

        //public static PlayerConvoBlock PlaceHolder => new PlayerConvoBlock("Placeholder option", new Traits(-1, 0, 0), ConversationConsts.ToBeDone);
    }
    public class NPCConvoBlock : IConversationBlock
    {
        private static JsonSerializer serializer;
        public string Text { get; set; }
        public List<PlayerConvoBlock> ResponsePool { get; set; }
        public NPCConvoBlock(string text, List<PlayerConvoBlock> responses)
        {
            Text = text;
            ResponsePool = responses;
            serializer = new JsonSerializer();
            Serialize();
        }

        public void Serialize() {
            using (StreamWriter sw = new StreamWriter("./Save/myJson.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, this);
            }
        }
        
        // // To deserialize:
        // using (StreamReader sr = new StreamReader(StateLoadingConst.GameStatePath))
        //     using (JsonReader jr = new JsonTextReader(sr)) {
        //     gameState = serializer.Deserialize<GameState>(jr);
        // }
    }
}

