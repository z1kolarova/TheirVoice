using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Assets.Classes
{
    public interface IConversationBlock
    {
        public string Text { get; set; }
        public string ResponsePoolName { get; set; }
    }

    public class PlayerConvoBlock : IConversationBlock
    {
        public string Text { get; set; }
        public Traits Impact { get; set; }
        public string ResponsePoolName { get; set; }

        private static JsonSerializer serializer;
        private static JsonSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new JsonSerializer();
                }
                return serializer;
            }
        }
        public static void SerializeResponsePool(ICollection<PlayerConvoBlock> convoBlocks, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Serializer.Serialize(writer, convoBlocks);
            }
        }
        public static void SerializeOneAsResponsePool(PlayerConvoBlock convoBlock, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Serializer.Serialize(writer, new PlayerConvoBlock[] { convoBlock });
            }
        }

        public static List<PlayerConvoBlock> GetResponsePoolByName(string name)
        {
            var path = Path.Combine(Utilities.ConvoBlocksDir, $"{name}.json");
            List<PlayerConvoBlock> result;

            using (StreamReader sr = new StreamReader(path))
            using (JsonReader jr = new JsonTextReader(sr))
            {
                result = Serializer.Deserialize<List<PlayerConvoBlock>>(jr);
            }

            return result;
        }
    }

    public class NPCConvoBlock : IConversationBlock
    {
        public string Text { get; set; }
        public string ResponsePoolName { get; set; }

        private static JsonSerializer serializer;
        private static JsonSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new JsonSerializer();
                }
                return serializer;
            }
        }
        
        public static void SerializeResponsePool(ICollection<NPCConvoBlock> convoBlocks, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Serializer.Serialize(writer, convoBlocks);
            }
        }
        public static void SerializeOneAsResponsePool(NPCConvoBlock convoBlock, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Serializer.Serialize(writer, new NPCConvoBlock[] { convoBlock});
            }
        }

        public static List<NPCConvoBlock> GetResponsePoolByName(string name)
        {
            var path = Path.Combine(Utilities.ConvoBlocksDir, $"{name}.json");
            List<NPCConvoBlock> result;

            using (StreamReader sr = new StreamReader(path))
            using (JsonReader jr = new JsonTextReader(sr))
            {
                result = Serializer.Deserialize<List<NPCConvoBlock>>(jr);
            }

            return result;
        }
    }
}

