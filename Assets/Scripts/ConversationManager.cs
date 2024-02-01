using Assets.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager I => instance;
    static ConversationManager instance;

    private static System.Random rng;
    private PasserbyAI talkingTo;

    private void Start()
    {
        instance = this;
        rng = new System.Random();
    }

    public void TriggerStartDialogue(PasserbyAI passerby)
    {
        talkingTo = passerby;
        talkingTo.BeApproached(PlayerController.I.transform.gameObject);
        ConversationUI.I.StartDialogue();
    }

    public List<PlayerConvoBlock> GetFirstPlayerOptions()
    {
        return SelectUpToFromCollection<PlayerConvoBlock>(4, ConversationConsts.P_OpeningLines);
    }

    public NPCConvoBlock GetNPCAnswer(IConversationBlock conversationBlock)
    {
        return (NPCConvoBlock)SelectUpToFromCollection(1, conversationBlock.ResponsePool)[0];
    }

    public List<PlayerConvoBlock> GetPlayerOptionsAfter(IConversationBlock conversationBlock)
    {
        return SelectUpToFromCollection(4, conversationBlock.ResponsePool).Select(x => (PlayerConvoBlock)x).ToList();
    }

    public string GetResponseTo(IConversationBlock conversationBlock)
    {
        return ConversationConsts.RevealDiet(talkingTo.personality);
        //return $"{conversationBlock.Text}? What's that supposed to mean?";
    }

    public void TriggerEndDialogue()
    {
        talkingTo.EndConversation();
        talkingTo = null;

        PlayerController.I.EndConversation();
    }

    private List<T> SelectUpToFromCollection<T>(int amount, List<T> collection)
    {
        if (collection == null || collection.Count == 0) //TODO solve properly
        {
            return new List<T>();
        }
        if (collection?.Count() <= amount) { return collection.ToList(); }

        var indexes = new int[amount];
        for (int i = 0; i < amount; i++)
        {
            int newIndex;
            do
            {
                newIndex = rng.Next(collection.Count());
            } while (indexes.Contains(newIndex));

            indexes[i] = newIndex;
        }
        var result = new List<T>();
        foreach (var i in indexes)
        {
            result.Add(collection[i]);
        }
        return result;
    }
}
