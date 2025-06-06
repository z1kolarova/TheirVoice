using Assets.Classes;
using Assets.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager I => instance;
    static ConversationManager instance;
    
    [HideInInspector] public bool IsInDialogue = false;

    private static System.Random rng;
    private PasserbyAI talkingTo;
    private bool passerbyWasWatching;

    private void Start()
    {
        instance = this;
        rng = new System.Random();
        //EstablishNeededConnections();
    }

    private async void EstablishNeededConnections()
    {
        var authenticated = await ClientSideManager.I.AuthenticateClient();
        if (authenticated)
        {
            await ClientSideManager.I.JoinPublicLobbyAndRelay();
        }
        else
        {
            Debug.Log("Authentication failed miserably and we have a problem...");
            //NetworkManagerUI.I.WriteLineToOutput("Authentication failed miserably and we have a problem...");
        }
    }

    public void TriggerStartDialogue(PasserbyAI passerby)
    {
        IsInDialogue = true;
        talkingTo = passerby;
        passerbyWasWatching = passerby.State == PasserbyStates.Watching;
        talkingTo.BeApproached(PlayerController.I.transform.gameObject);

        var promptLabelToUse = passerbyWasWatching ? talkingTo.personality.PromptLabel : ConvoUtilsGPT.notInterestedPromptLabel;
        var promptToUse = passerbyWasWatching ? talkingTo.personality.Prompt : ConvoUtilsGPT.CreateNotInterestedPrompt();

        Debug.Log($"Has all needed connections: {ClientSideManager.I.HasAllNeededConnections}");
        if (!ClientSideManager.I.HasAllNeededConnections || Utils.ConversationMode == ConversationModes.Premade)
        {
            ConversationUI.I.StartDialogue(npcInterested: passerbyWasWatching);
        }
        else
        {
            PersonalityInfoUI.I.SetActive(true);
            PersonalityInfoUI.I.GetAttributesForDisplay(promptLabelToUse.Name, promptToUse.GeneralConvoEndingAbility, promptToUse.CanEndConvoThisTime);
            ConvoUtilsGPT.InitNewConvoWithPrompt(promptToUse.Text);
            ConversationUIChatGPT.I.StartDialogue(npcInterested: passerbyWasWatching);
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
        IsInDialogue = false;

        PersonalityInfoUI.I.SetActive(false);
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
