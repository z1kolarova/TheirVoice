using Assets.Classes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager I => instance;
    static ConversationManager instance;
    
    [HideInInspector] public bool InDialog = false;

    private static System.Random rng;
    private PasserbyAI talkingTo;

    private void Start()
    {
        instance = this;
        rng = new System.Random();
    }

    public void TriggerStartDialogue(PasserbyAI passerby)
    {
        InDialog = true;
        talkingTo = passerby;
        var origState = passerby.State;
        talkingTo.BeApproached(PlayerController.I.transform.gameObject);
        if (Utilities.ConversationMode == ConversationModes.Premade)
        {
            ConversationUI.I.StartDialogue(npcInterested: origState == PasserbyStates.Watching);
        }
        else
        {
            ConvoUtilsGPT.InitNewConvoWithPrompt(talkingTo.personality.PersonalityPrompt);
            ConversationUIChatGPT.I.StartDialogue(npcInterested: origState == PasserbyStates.Watching);
        }
    }

    public List<PlayerConvoBlock> GetFirstPlayerOptions(bool npcInterested = true)
    {
        //return SelectUpToFromCollection<PlayerConvoBlock>(4, ConversationConsts.P_OpeningLines);
        if (npcInterested)
            //return ConvoUtils.GetResponsePoolByName<PlayerConvoBlock>("P_OpeningLines");
            return ConvoUtils.GetResponsePoolByName<PlayerConvoBlock>("Sample_P_OpeningLines");
        else
            return ConvoUtils.GetResponsePoolByName<PlayerConvoBlock>("P_DoYouHaveTime");
    }

    public NPCConvoBlock GetNPCAnswerTo(PlayerConvoBlock conversationBlock)
    {
        var responsePool = ConvoUtils.GetResponsePoolByName<NPCConvoBlock>(conversationBlock.ResponsePoolName);
        return SelectUpToFromCollection(1, responsePool)[0];
    }

    public List<PlayerConvoBlock> GetPlayerOptionsAfter(NPCConvoBlock conversationBlock)
    {
        var responsePool = ConvoUtils.GetResponsePoolByName<PlayerConvoBlock>(conversationBlock.ResponsePoolName);
        return SelectUpToFromCollection(4, responsePool);
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
        InDialog = false;

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
