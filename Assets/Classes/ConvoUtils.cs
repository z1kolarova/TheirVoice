using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Assets.Classes
{
    public static class ConvoUtils
    {
        public static string ConvoBlocksDir = "./Assets/ConversationBlocks/";

        public static List<T> GetResponsePoolByName<T>(string name) where T : IConversationBlock
        {
            var path = Path.Combine(ConvoBlocksDir, $"{name}.json");
            List<T> result;

            using (StreamReader sr = new StreamReader(path))
            using (JsonReader jr = new JsonTextReader(sr))
            {
                result = Utilities.Serializer.Deserialize<List<T>>(jr);
            }

            return result;
        }
        public static void SerializeResponsePool<T>(ICollection<T> convoBlocks, string filePath) where T : IConversationBlock
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Utilities.Serializer.Serialize(writer, convoBlocks);
            }
        }
        public static void SerializeOneAsResponsePool<T>(T convoBlock, string filePath) where T : IConversationBlock
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Utilities.Serializer.Serialize(writer, new T[] { convoBlock });
            }
        }
    }
}
